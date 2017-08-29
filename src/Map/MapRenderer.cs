/*
 *  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 *  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 *  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 *  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
 *  REMAINS UNCHANGED.
 *
 *  REPO: http://www.github.com/tomwilsoncoder/RTS
*/
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

public unsafe class MapRenderer {
    private Camera p_Camera;
    private Game p_Game;
    private Map p_Map;
    private object p_Mutex = new object();

    private bool p_UpdateVisibleBlocks = true;
    private List<VisibleBlock> p_VisibleBlocks;

    public MapRenderer(Game game, Map map, Camera camera) {
        p_Game = game;
        p_Map = map;
        p_Camera = camera;
        
        //hook change events to trigger a recalculation of
        //what blocks are visible on screen
        camera.CameraChanged += delegate(Camera c) {
            p_UpdateVisibleBlocks = true;
        };
        game.Window.Resize += delegate(object sender, EventArgs e) {
            p_UpdateVisibleBlocks = true;
        };
    }

    public void Draw(IRenderContext context, IRenderer renderer) {
        //get all the visible blocks
        if (p_UpdateVisibleBlocks) {
            lock (p_Mutex) {
                p_VisibleBlocks = getVisibleBlocks(context);
                p_UpdateVisibleBlocks = false;
            }
        }
        
        List<VisibleBlock> visible = VisibleBlocks;
        if (visible.Count == 0) { return; }

        //grab the LOS matrix so we can quickly deturmine if a block is
        //within LOS.
        bool* los = (bool*)0;
        Fog fog = p_Game.CurrentPlayer.Fog;
        fog.Lock();
        if (p_Game.EnableLOS) {
            los = fog.GetLOSMatrix();
        }

        //fire the event for the beginning of a frame render
        if (BeginRenderFrame != null) {
            BeginRenderFrame(
                this,
                context,
                renderer);
        }

        Camera camera = p_Game.Camera;
        int blockSize = camera.BlockSize;

        #region initial render pass
        Point cursorPos = p_Game.PointToClient(Cursor.Position);
        foreach (VisibleBlock block in visible) {
            drawBlock(
                context,
                renderer,
                block,
                los);
        }
        #endregion

        #region secondary pass
        /*
            for anything that requires us to render over other blocks. 
        */
        foreach (VisibleBlock block in visible) {
            /*draw coastline*/
            if ((*block.Block).TypeID != Globals.TERRAIN_WATER) {
                drawCoastBlock(
                    context,
                    renderer,
                    block);
            }

            /*fire block render*/
            if (BlockRender != null) {
                BlockRender(
                    this,
                    context,
                    renderer,
                    block);
            }
        }
        #endregion

        //fire event after frame finished
        if (EndRenderFrame != null) {
            EndRenderFrame(
                this,
                context,
                renderer);
        }

        fog.Unlock();
    }

    public VisibleBlock GetBlockAtPoint(IRenderContext context, Point pt) {
        bool found;
        VisibleBlock ret = TryGetBlockAtPoint(context, pt, out found);
        if (!found) {
            throw new Exception("No block was found at the position: " + pt);
        }
        return ret;
    }
    public VisibleBlock TryGetBlockAtPoint(IRenderContext context, Point pt) {
        bool f;
        return TryGetBlockAtPoint(context, pt, out f);
    }
    public VisibleBlock TryGetBlockAtPoint(IRenderContext context, Point pt, out bool found) {
        List<VisibleBlock> visible = GetBlocksInRegion(
            context,
            new Rectangle(pt, Size.Empty),
            false);
        if (visible.Count == 0) {
            found = false; 
            return default(VisibleBlock); 
        }
        found = true;
        return visible[0];
    }

    public List<VisibleBlock> GetBlocksInRegion(IRenderContext context, Rectangle region) {
        return GetBlocksInRegion(context, region, true);
    }
    public List<VisibleBlock> GetBlocksInRegion(IRenderContext context, Rectangle region, bool clip) {
        List<VisibleBlock> buffer = new List<VisibleBlock>();
        Camera cam = p_Camera;
        int x = region.X;
        int y = region.Y;
        int w = region.Width;
        int h = region.Height;
        int blockSize = cam.BlockSize;

        //negative size of region? if so, adjust location of region accordingly.
        bool regionChange = false;
        if (w < 0) {
            w = -w;
            x -= w;
            regionChange = true;
        }
        if (h < 0) {
            h = -h;
            y -= h;
            regionChange = true;
        }
        if (regionChange) {
            region = new Rectangle(x, y, w, h);
        }


        //get all visible blocks
        List<VisibleBlock> visible = VisibleBlocks;
        if (visible.Count == 0) { return new List<VisibleBlock>(); }

        //get all the blocks that interesect with this region
        foreach (VisibleBlock block in visible) {
            if (block.Block == (Block*)0) { continue; }
            Rectangle rect = new Rectangle(
                block.RenderX, block.RenderY,
                blockSize + 1, blockSize + 1);

            if (rect.IntersectsWith(region)) {
                buffer.Add(block);
            }
        }
        return buffer;
    }

    public void Invalidate() { p_UpdateVisibleBlocks = true; }

    private void drawBlock(IRenderContext context, IRenderer renderer, VisibleBlock vBlock, bool* los) {
        Block block = *vBlock.Block;
        Color color = Globals.COLOR_TERRAIN_GRASS;
        Camera camera = p_Camera;
        int blockSize = camera.BlockSize;

        switch (block.TypeID) {
            case Globals.TERRAIN_WATER: color = Globals.COLOR_TERRAIN_WATER; break;
            case Globals.RESOURCE_WOOD: color = Color.Brown; break;
            case Globals.RESOURCE_FOOD: color = Color.Red; break;
            case Globals.RESOURCE_STONE: color = Color.Gray; break;
            case Globals.RESOURCE_GOLD: color = Color.Gold; break;
        }

        /*add depth to water and grass*/
        if (block.TypeID == Globals.TERRAIN_GRASS) {
            color = getColorAtPoint(
                Globals.COLOR_TERRAIN_GRASS, Color.Black,
                (block.Height * 1.0f / 150));
        }
        if (block.TypeID == Globals.TERRAIN_WATER) {
            color = getColorAtPoint(
                Globals.COLOR_TERRAIN_WATER, changeLight(Globals.COLOR_TERRAIN_WATER, .4f),
                (block.Height * 1.0f / 100));
        }

        if (block.Selected) {
            color = Color.White;
        }

        //render the block
        renderer.SetBrush(new SolidBrush(color));
        renderer.FillQuad(vBlock.RenderX, vBlock.RenderY, camera.BlockSize, camera.BlockSize);

        /*draw LOS*/
        if (los != (bool*)0) {
            bool hasLOS = *(los + (vBlock.BlockY * p_Map.Width) + vBlock.BlockX);
            color = Color.FromArgb(hasLOS ? 0 : 140, Color.Black);

            renderer.SetBrush(new SolidBrush(color));
            renderer.FillQuad(
                vBlock.RenderX,
                vBlock.RenderY,
                blockSize,
                blockSize);
        }
    }
    private void drawCoastBlock(IRenderContext context, IRenderer renderer, VisibleBlock vBlock) {
        //get info
        Map map = p_Map;
        Block* matrix = map.GetBlockMatrix();
        Block* block = vBlock.Block;
        int size = p_Camera.BlockSize;
        int width = map.Width;
        int height = map.Height;
        int blockSize = p_Camera.BlockSize;
        int shadowSize = 10;

        //coastal?
        Direction direction;
        if(!isCoastline(block, matrix, out direction, vBlock.BlockX, vBlock.BlockY, width,height)){
            return;
        }


        /*define gradient start/end x/y*/
        int gradientStartX, gradientStartY,
            gradientEndX, gradientEndY;
        Color gColor1 = changeLight(Globals.COLOR_TERRAIN_WATER, 0.4f);
        Color gColor2 = Color.Transparent;

        #region draw basic border
        /*
            reduce down the direction to get each individual direction
            by using bitwise operators.
        */
        byte directionANDCurrent = (byte)0x08;
        int dX = 0, dY = 0;
        while (directionANDCurrent != 0) {
            Direction current = (direction & (Direction)directionANDCurrent);
            directionANDCurrent >>= 1;

            if (current == Direction.NONE) { continue; }
            
            int x, y, w, h;
            x = y = w = h = 0;

            /**/
            bool isNorth = current == Direction.NORTH;
            bool isSouth = current == Direction.SOUTH;
            bool isWest = current == Direction.WEST;
            bool isEast = current == Direction.EAST;

            //reset gradient position
            gradientStartX = gradientStartY =
            gradientEndX = gradientEndY = 0;

            #region North/South
            if (isNorth || isSouth) {
                w = blockSize;
                h = shadowSize;
            }
            if (isNorth) { 
                y -= shadowSize; 
                dY = -1;
                gradientEndY = -shadowSize - 1;
            }
            if (isSouth) { 
                y = blockSize; 
                dY = 1;
                gradientStartY = blockSize - 1;
                gradientEndY = blockSize + shadowSize + 1;
            }
            #endregion

            #region West/East
            if (isWest || isEast) {
                w = shadowSize;
                h = blockSize;
            }
            if (isWest) { 
                x = -shadowSize; 
                dX = -1;
                gradientEndX = -shadowSize - 1;
            }
            if (isEast) { 
                x = blockSize; 
                dX = 1;
                gradientStartX = blockSize - 1;
                gradientEndX = blockSize + shadowSize + 1;
            }
            #endregion 

            gradientStartX += vBlock.RenderX;
            gradientStartY += vBlock.RenderY;
            gradientEndX += vBlock.RenderX;
            gradientEndY += vBlock.RenderY;

            x += vBlock.RenderX;
            y += vBlock.RenderY;
            renderer.SetBrush(new LinearGradientBrush(
                new Point(gradientStartX, gradientStartY),
                new Point(gradientEndX, gradientEndY),
                gColor1, gColor2));
            renderer.FillQuad(x, y, w, h);
        }
        #endregion


        gradientStartX = gradientStartY =
        gradientEndX = gradientEndY = 0;

        #region draw diagonal
        //can be diagonal?
        if (dX == 0 || dY == 0) {
            return;
        }



        /*get render location of the square*/
        int rX = vBlock.RenderX;
        int rY = vBlock.RenderY;
        if (dX < 0) { rX -= shadowSize; }
        else { rX += blockSize; }
        if (dY < 0) { rY -= shadowSize; }
        else { rY += blockSize; }


        /*define gradient information (default to NORTH_WEST)*/
        int centerOffsetX = shadowSize;
        int centerOffsetY = shadowSize;
        int rectOffsetX = 0;
        int rectOffsetY = 0;

        /*north east?*/
        if (dX > 0 && dY < 0) {
            centerOffsetX = 0;
            rectOffsetX += shadowSize;
        }
        /*south west?*/
        if (dX < 0 && dY > 0) {
            centerOffsetY = 0;
            rectOffsetY += shadowSize;
        }
        /*south east*/
        if (dX > 0 && dY > 0) {
            centerOffsetX = 0;
            centerOffsetY = 0;
            rectOffsetX += shadowSize;
            rectOffsetY += shadowSize;
        }

        GraphicsPath path = new GraphicsPath();
        path.AddRectangle(
            new Rectangle(
                rX - rectOffsetX, rY - rectOffsetY,
                shadowSize * 2, shadowSize * 2));

        PathGradientBrush brush = new PathGradientBrush(path);
        brush.CenterPoint = new PointF(rX + centerOffsetX, rY + centerOffsetY);
        brush.CenterColor = gColor1;
        brush.SurroundColors = new Color[] { gColor2 };
        renderer.SetBrush(brush);

        renderer.FillQuad(
            rX, rY,
            shadowSize, shadowSize);

        #endregion
    }

    private Color getColorAtPoint(Color source, Color target, float p) {
        if ((target.ToArgb() & 0x00ffffff) > (source.ToArgb() & 0x00ffffff)) {
            Color t = source;
            source = target;
            target = t;
        }

        int deltaR = target.R - source.R;
        int deltaG = target.G - source.G;
        int deltaB = target.B - source.B;


        return Color.FromArgb(
            source.A,

            source.R + (int)(deltaR * p),
            source.G + (int)(deltaG * p),
            source.B + (int)(deltaB * p));


    }

    private Color changeLight(Color c, float p) {
        return Color.FromArgb(
            c.A,
            (int)(c.R * p),
            (int)(c.G * p),
            (int)(c.B * p));
    }

    private List<VisibleBlock> getVisibleBlocks(IRenderContext context) {
        List<VisibleBlock> buffer = new List<VisibleBlock>();
        p_Map.Lock();

        //get the map information
        int width = p_Map.Width;
        int height = p_Map.Height;

        //get the camera info
        Camera cam = p_Camera;
        int blockSize = cam.BlockSize;
        int camX = cam.X;
        int camY = cam.Y;
        int screenWidth = context.Width;
        int screenHeight = context.Height;

        //define x,y so we know where in the matrix we are
        int x = 0, y = 0;        
        int rX = -cam.X;
        int rY = -cam.Y;

        //define the offset of where in the matrix we start actually searching.
        int offsetX = (int)Math.Floor(camX * 1.0f / blockSize);
        int offsetY = (int)Math.Floor(camY * 1.0f / blockSize);
        if (offsetX < 0) { offsetX = 0; }
        if (offsetY < 0) { offsetY = 0; }

        x = offsetX;
        y = offsetY;

        if (x >= width || y >= height) {
            p_Map.Unlock();
            return new List<VisibleBlock>();
        }

        //point the render x/y to the offset location.
        rX += offsetX * blockSize;
        rY += offsetY * blockSize;

        //read the matrix
        Block* matrix = p_Map.GetBlockMatrix();
        Block* ptr = matrix + (offsetY * width) + offsetX;
        Block* ptrEnd = matrix + (width * height);

        Block* ptrLineEnd = matrix + (offsetY * width) + width;
        
        if (ptr >= ptrEnd || ptr < matrix) {
            p_Map.Unlock();
            return new List<VisibleBlock>();
        }

        while (ptr <= ptrEnd) {
            //read the block
            Block* block = ptr++;

            //is the block visible?
            bool visible = true;
            if (rY >= screenHeight) { break; }
            if (rX >= screenWidth) {
                //skip line
                ptr = ptrLineEnd + offsetX;
                ptrLineEnd += width;

                //reset x/y
                x = offsetX;
                rX = -camX + (offsetX * blockSize);
                
                y++;
                rY += blockSize;
                if (y == height) { break; }
                continue;
            }
            if (rX + blockSize <= 0 || rY + blockSize <= 0) {
                visible = false;
            }

            //add to return if it is visible
            if (visible) {                  
                buffer.Add(new VisibleBlock() { 
                    RenderX = rX, 
                    RenderY = rY,
                    BlockX = x,
                    BlockY = y,
                    Block = block
                });
            }

            x++;
            rX += blockSize;

            //x at end of row?
            if (x == width) {                 
                x = offsetX; 
                y++;

                //end of the rows?
                if (y == height) { break; }

                ptr = ptrLineEnd + offsetX;
                ptrLineEnd += width;

                rX = -cam.X + (offsetX * blockSize);
                rY += blockSize;
            }
            
        }


        //filter fog?
        if (p_Game.EnableFog) {
            List<VisibleBlock> fogBuffer =
                p_Game.CurrentPlayer.Fog.Filter(
                    buffer);
            buffer = null;
            return fogBuffer;
        }

        return buffer;
    }

    private bool isCoastline(Block* ptr, Block* matrix, out Direction direction, int x, int y, int width, int height) {
        Block* ptrEnd = matrix + (width * height);

        //get adjacent blocks
        Block* top = ptr - width;
        Block* left = ptr - 1;
        Block* right = ptr + 1;
        Block* bottom = ptr + width;
        
        //check 
        int typeID = Globals.TERRAIN_WATER;
        direction = Direction.NONE;

        if ((*ptr).TypeID == Globals.TERRAIN_WATER) { return false; }

        if (y > 0 && (*top).TypeID == typeID) {
            direction |= Direction.NORTH;
        }
        if (x > 0 && (*left).TypeID == typeID) {
            direction |= Direction.WEST;
        }
        if (y < height && bottom < ptrEnd && (*bottom).TypeID == typeID) {
            direction |= Direction.SOUTH;
        }
        if (x < width && (*right).TypeID == typeID) {
            direction |= Direction.EAST;
        }


        return (direction != Direction.NONE);
    }

    public List<VisibleBlock> VisibleBlocks { 
        get {
            lock (p_Mutex) {
                return p_VisibleBlocks;
            }
        } 
    }

    public event OnBeginFrameRenderEventHandler BeginRenderFrame;
    public event OnEndFrameRenderEventHandsler EndRenderFrame;
    public event OnBlockRenderEventHandler BlockRender;

    public delegate void OnBeginFrameRenderEventHandler(MapRenderer sender, IRenderContext context, IRenderer renderer);
    public delegate void OnEndFrameRenderEventHandsler(MapRenderer sender, IRenderContext context, IRenderer renderer);
    public delegate void OnBlockRenderEventHandler(MapRenderer sender, IRenderContext context, IRenderer renderer, VisibleBlock block);

}
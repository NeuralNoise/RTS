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

        //draw
        Camera camera = p_Game.Camera;
        int blockWidth = camera.BlockWidth;
        int blockHeight = camera.BlockHeight;
        
        Point cursorPos = p_Game.PointToClient(Cursor.Position);
        foreach (VisibleBlock block in visible) {
            drawBlock(
                context,
                renderer,
                block,
                los);

            //fire event to draw a block
            if (BlockRender != null) {
                BlockRender(
                    this,
                    context,
                    renderer,
                    block);
            }
        }

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
        int blockW = cam.BlockWidth;
        int blockH = cam.BlockHeight;

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
                blockW + 1, blockH + 1);

            if (rect.IntersectsWith(region)) {
                buffer.Add(block);
            }
        }
        return buffer;
    }

    public void Invalidate() { p_UpdateVisibleBlocks = true; }

    private void drawBlock(IRenderContext context, IRenderer renderer, VisibleBlock vBlock, bool* los) {
        Block block = *vBlock.Block;
        Color color = Color.Green;
        Camera camera = p_Camera;
        int blockWidth = camera.Width;
        int blockHeight=camera.Height;

        switch (block.TypeID) { 
            case BlockType.TERRAIN_WATER: color = Color.DodgerBlue;  break;
            case BlockType.RESOURCE_WOOD: color = Color.Brown; break;
            case BlockType.RESOURCE_FOOD: color = Color.Red; break;
            case BlockType.RESOURCE_STONE: color = Color.Silver; break;
            case BlockType.RESOURCE_GOLD: color = Color.Gold; break;
        }

        if (block.Selected) {
            color = Color.White;
        }

        //render the block
        renderer.SetBrush(new SolidBrush(color));
        renderer.FillQuad(vBlock.RenderX, vBlock.RenderY, camera.BlockWidth, camera.BlockHeight);

        if (block.TypeID == BlockType.TERRAIN_WATER &&
            vBlock.ShadowDirection != null) {
            List<Direction> directions = vBlock.ShadowDirection;


            //grab block size
            int bWidth = camera.BlockHeight;
            int bHeight = camera.BlockWidth;

            //
            foreach (Direction d in directions) {
                drawWaterShadow(
                    renderer,
                    d,
                    vBlock,
                    bWidth,
                    bHeight);
            }

        }

        /*draw LOS*/
        if (los != (bool*)0) {
            bool hasLOS = *(los + (vBlock.BlockY * p_Map.Width) + vBlock.BlockX);
            color = Color.FromArgb(hasLOS ? 0 : 140, Color.Black);

            renderer.SetBrush(new SolidBrush(color));
            renderer.FillQuad(
                vBlock.RenderX,
                vBlock.RenderY,
                blockWidth,
                blockHeight);
        }
    }
    private List<VisibleBlock> getVisibleBlocks(IRenderContext context) {
        List<VisibleBlock> buffer = new List<VisibleBlock>();
        p_Map.Lock();

        //get the map information
        int width = p_Map.Width;
        int height = p_Map.Height;

        //get the camera info
        Camera cam = p_Camera;
        int blockWidth = cam.BlockWidth;
        int blockHeight = cam.BlockHeight;
        int camX = cam.X;
        int camY = cam.Y;
        int screenWidth = context.Width;
        int screenHeight = context.Height;

        //define x,y so we know where in the matrix we are
        int x = 0, y = 0;        
        int rX = -cam.X;
        int rY = -cam.Y;

        //define the offset of where in the matrix we start actually searching.
        int offsetX = (int)Math.Floor(camX * 1.0f / blockWidth);
        int offsetY = (int)Math.Floor(camY * 1.0f / blockHeight);
        if (offsetX < 0) { offsetX = 0; }
        if (offsetY < 0) { offsetY = 0; }

        x = offsetX;
        y = offsetY;

        if (x >= width || y >= height) {
            p_Map.Unlock();
            return new List<VisibleBlock>();
        }

        //point the render x/y to the offset location.
        rX += offsetX * blockWidth;
        rY += offsetY * blockHeight;

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
                rX = -camX + (offsetX * blockWidth);
                
                y++;
                rY += blockHeight;
                if (y == height) { break; }
                continue;
            }
            if (rX + blockWidth <= 0 || rY + blockHeight <= 0) {
                visible = false;
            }

            //add to return if it is visible
            if (visible) {
                /*is this block a water block?
                  if so, deturmine it's shadow
                 */
                List<Direction> directions = null;
                if ((*block).TypeID == BlockType.TERRAIN_WATER) {
                    directions = getAdjacentGrassDirection(x, y);
                }


                buffer.Add(new VisibleBlock() { 
                    RenderX = rX, 
                    RenderY = rY,
                    BlockX = x,
                    BlockY = y,
                    Block = block,
                    ShadowDirection = directions
                });
            }

            x++;
            rX += blockWidth;

            //x at end of row?
            if (x == width) {                 
                x = offsetX; 
                y++;

                //end of the rows?
                if (y == height) { break; }

                ptr = ptrLineEnd + offsetX;
                ptrLineEnd += width;

                rX = -cam.X + (offsetX * blockWidth);
                rY += blockHeight;
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

    private void drawWaterShadow(IRenderer renderer, Direction direction, VisibleBlock block, int blockWidth, int blockHeight) {
        const int shadowWidth = 10;
        
        /*get the bounds of the shadow*/
        int boundsX = 0, boundsY = 0;
        int boundsWidth = 0, boundsHeight = 0;
        getShadowBounds(
            direction,
            blockWidth, blockHeight,
            ref boundsX,
            ref boundsY,
            ref boundsWidth,
            ref boundsHeight,
            shadowWidth);
        boundsX += block.RenderX;
        boundsY += block.RenderY;

        renderer.SetBrush(Brushes.LightBlue);
        
        renderer.FillQuad(
            boundsX, boundsY,
            boundsWidth, boundsHeight);
    }
    private void swap<T>(ref T v1, ref T v2) {
        T tmp = v2;
        v2 = v1;
        v1 = tmp;
    }
    private void getShadowBounds(Direction direction, int blockWidth, int blockHeight, 
                                 ref int x, ref int y, ref int width, ref int height, int shadowWidth) { 
        
        //default width/height to block size
        width = blockWidth;
        height = blockHeight;

        //deturmine size
        if (
            (direction & Direction.NORTH) == Direction.NORTH ||
            (direction & Direction.SOUTH) == Direction.SOUTH) {
                height = shadowWidth;
        }
        if (
            (direction & Direction.EAST) == Direction.EAST ||
            (direction & Direction.WEST) == Direction.WEST) {
                width = shadowWidth;
        }

        //deturmine location
        x = y = 0;
        if ((direction & Direction.EAST) == Direction.EAST) {
            x = blockWidth - shadowWidth;
        }
        if ((direction & Direction.SOUTH) == Direction.SOUTH) {
            y = blockHeight - shadowWidth;
        }
    }


    private List<Direction> getAdjacentGrassDirection(int x, int y) {
        Map map = p_Map;
        int width = map.Width;
        int height = map.Height;

        //get the block matrix
        map.Lock();
        Block* matrix = map.GetBlockMatrix();

        //define the locations of all 8 adjacent nodes (including diagonal)
        Point[] points = new Point[] {
            new Point(x - 1, y),
            new Point(x,     y - 1),
            new Point(x + 1, y),
            new Point(x,     y + 1),

            new Point(x - 1, y - 1),
            new Point(x + 1, y - 1),
            new Point(x - 1, y + 1),
            new Point(x + 1, y + 1)

        };

        List<Direction> buffer = new List<Direction>();

        //iterate through all adjacent locations
        int l = points.Length;
        for(int c = 0; c < l; c++) {
            Point p = points[c];
            int pX = p.X;
            int pY = p.Y;

            //valid?
            if (pX < 0 || pY < 0 ||
               pX >= width || pY >= height) {
                   continue;
            }
               
            //resolve the block at this x/y
            Block* block = matrix + (pY * width) + pX;

            //grass?
            if ((*block).TypeID == BlockType.TERRAIN_WATER) {
                continue;
            }

            //deturmine the direction of this point
            Direction direction = Direction.NONE;
            if (pX < x) { direction |= Direction.WEST; }
            else if(pX > x) { direction |= Direction.EAST; }

            if (pY < y) { direction |= Direction.NORTH; }
            else if(pY > y) { direction |= Direction.SOUTH; }

            //add
            buffer.Add(direction);
        }

        if (buffer.Count == 0) { return null; }

        //clean up
        map.Unlock();
        return buffer;

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
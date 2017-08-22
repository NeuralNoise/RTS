using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;

public unsafe class Map : IDisposable {
    private Game p_Game;
    private Block* p_Matrix;
    private bool* p_ConcreteMatrix;
    private int p_Width, p_Height;
    private BlockStates<ResourceState> p_States;
    private object p_Mutex = new object();

    private bool p_UpdateVisibleBlocks = true;
    private LinkedList<VisibleBlock> p_VisibleBlocks;

    public Map(Game game, int width, int height) {
        p_States = new BlockStates<ResourceState>();
        p_Game = game;
        p_Width = width;
        p_Height = height;

        //allocate a block of memory to store the matrix
        p_Matrix = (Block*)Marshal.AllocHGlobal(
            (width * height) * sizeof(Block));

        //initialize every block
        Block* ptr = p_Matrix;
        Block* ptrEnd = p_Matrix + (width * height);
        while (ptr != ptrEnd) {
            (*ptr).Selected = false;
            (*(ptr++)).StateID = -1;            
        }

        //generate resources
        generateResource(0, 10); //wood
        generateResource(1, 175); //food
        generateResource(2, 250); //gold
        generateResource(3, 200); //stone
        updateConcreteMatrix();

        //hook change events to trigger a recalculation of
        //what blocks are visible on screen
        game.Camera.CameraChanged += delegate(Camera camera) {
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
        
        LinkedList<VisibleBlock> visible = VisibleBlocks;
        if (visible.Count == 0) { return; }

        //grab the LOS matrix so we can quickly deturmine if a block is
        //within LOS.
        bool* los = (bool*)0;
        Fog fog = p_Game.CurrentPlayer.Fog;
        fog.Lock();
        if (p_Game.EnableLOS) {
            los = fog.GetLOSMatrix();
        }

        //draw
        Camera camera = p_Game.Camera;
        int blockWidth = camera.BlockWidth;
        int blockHeight = camera.BlockHeight;

        LinkedListNode<VisibleBlock> current = visible.First;
        while (current != null) {
            VisibleBlock block = current.Value;
            RenderBlock(
                context,
                renderer,
                block,
                los,
                blockWidth,
                blockHeight);
            current = current.Next;
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
        LinkedList<VisibleBlock> visible = GetBlocksInRegion(
            context,
            new Rectangle(pt, Size.Empty),
            false);
        if (visible.Count == 0) {
            found = false; 
            return default(VisibleBlock); 
        }
        found = true;
        return visible.First.Value;
    }

    public LinkedList<VisibleBlock> GetBlocksInRegion(IRenderContext context, Rectangle region) {
        return GetBlocksInRegion(context, region, true);
    }
    public LinkedList<VisibleBlock> GetBlocksInRegion(IRenderContext context, Rectangle region, bool clip) {
        LinkedList<VisibleBlock> buffer = new LinkedList<VisibleBlock>();
        Camera cam = p_Game.Camera;
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
        LinkedList<VisibleBlock> visible = VisibleBlocks;
        if (visible.Count == 0) { return new LinkedList<VisibleBlock>(); }
        int matrixW, matrixH;
        VisibleBlock[,] matrix = ProjectVisibleToMatrix(visible, out matrixW, out matrixH);

        //get all the blocks that interesect with this region
        for (int cX = 0; cX < matrixW; cX++) {
            for (int cY = 0; cY < matrixH; cY++) { 
                VisibleBlock block = matrix[cX, cY];
                if (block.Block == (Block*)0) { continue; }
                Rectangle rect = new Rectangle(
                    block.RenderX, block.RenderY,
                    blockW + 1, blockH + 1);

                if (rect.IntersectsWith(region)) {
                    buffer.AddLast(block);
                }
            }
        }



        return buffer;
        /*calculate the block at the location of the region so we can then do a 
          matrix scan over the visible blocks based off the regions size.*/
        
        int firstX = (int)((x - matrix[0,0].RenderX) * 1.0f / blockW);
        int firstY = (int)((y - matrix[0,0].RenderY) * 1.0f / blockH);

        /*exceed?*/
        if (firstX < 0) {
            if (clip) { firstX = 0; }
            else { return new LinkedList<VisibleBlock>(); }
        }
        if (firstY < 0) {
            if (clip) { firstY = 0; }
            else { return new LinkedList<VisibleBlock>(); }
        }
        if (firstX >= matrixW || firstY >= matrixH) {
            return new LinkedList<VisibleBlock>();
        }

        /*calculate the size of the selection blocks*/
        int selectW = (int)Math.Ceiling(w * 1.0f / blockW);
        int selectH = (int)Math.Ceiling(h * 1.0f / blockW);

        /*get all*/
        int destX = firstX + selectW;
        int destY = firstY + selectH;
        if (destX > matrixW) { destX = matrixW; }
        if (destY > matrixH) { destY = matrixH; }

        for (int cX = firstX; cX < destX; cX++) {
            for (int cY = firstY; cY < destY; cY++) {
                /*does the block intersect with the region*/
                VisibleBlock block = matrix[cX, cY];
                Rectangle bounds = new Rectangle(
                    block.RenderX, block.RenderY,
                    blockW, blockH);

                if (bounds.IntersectsWith(region)) {
                    buffer.AddLast(block);
                }
            }
        }

        return buffer;
    }

    public VisibleBlock[,] ProjectVisibleToMatrix(LinkedList<VisibleBlock> blocks, out int w, out int h) { 
        //get the min/max coords to deturmine the size
        int minX, minY, maxX, maxY;
        minX = minY = int.MaxValue;
        maxX = maxY = 0;
        foreach (VisibleBlock b in blocks) {
            if (b.BlockX < minX) { minX = b.BlockX; }
            if (b.BlockY < minY) { minY = b.BlockY; }
            if (b.BlockX > maxX) { maxX = b.BlockX; }
            if (b.BlockY > maxY) { maxY = b.BlockY; }
        }

        //check size
        w = maxX - minX + 1;
        h = maxY - minY + 1;
        if (w == 0 || h == 0) { return null; }

        //create and project onto the matrix.
        VisibleBlock[,] matrix = new VisibleBlock[w, h];
        foreach (VisibleBlock b in blocks) {
            matrix[b.BlockX - minX, b.BlockY - minY] = b;
        }
        return matrix;
    }

    public void Invalidate() { p_UpdateVisibleBlocks = true; }

    public void Lock() { Monitor.Enter(p_Mutex); }
    public void Unlock() { Monitor.Exit(p_Mutex); }

    public bool* GetConcreteMatrix() { return p_ConcreteMatrix; }
    public Block* GetBlockMatrix() { return p_Matrix; }

    private void RenderBlock(IRenderContext context, IRenderer renderer, VisibleBlock vBlock, bool* los, int width, int height) {
        Block block = *vBlock.Block;
        Color color = Color.Lime;

        if (block.StateID != -1) {
            ResourceState state = p_States.Resolve(block.StateID);
            switch (state.ResourceID) {
                case 3: color = Color.Gray; break;
                case 2: color = Color.Gold; break;
                case 1: color = Color.Red; break;
                case 0: color = Color.Brown; break;
            }
        }

        if (los != (byte*)0) {
            //bool hasLOS = fog.HasLOS(vBlock.BlockX, vBlock.BlockY);
            bool hasLOS = *(los + (vBlock.BlockY * p_Width) + vBlock.BlockX);
            color = Color.FromArgb(hasLOS ? 255 : 140, color);
        }

        if (block.Selected) {
            color = Color.Blue;
        }

        renderer.SetBrush(new SolidBrush(color));
        renderer.FillQuad(vBlock.RenderX, vBlock.RenderY, width, height);

        if (block.Selected) {
            renderer.SetBrush(new SolidBrush(Color.FromArgb(
                150, 255, 255, 255)));
            renderer.FillQuad(vBlock.RenderX, vBlock.RenderY, width, height);
        }
    }
    
    private Block* translateToPointer(int x, int y) {
        return p_Matrix + (y * p_Width) + x;
    }
    private void generateResource(int resourceID, int rarity) {
        Random random = new Random();

        Console.WriteLine("Generating " + resourceID);

        //iterate through every block
        Block* ptr = p_Matrix;
        Block* ptrEnd = p_Matrix + (p_Width * p_Height);

        while (ptr != ptrEnd) {
            //if a random number between 0 and the rarity hits
            //half way, we create a resource block.
            if (random.Next(0, rarity) == (rarity / 2)) {
                //randomly generate an amount
                double amount = Math.Ceiling(random.NextDouble() * 10000);

                //create a resource state
                ResourceState state = null;
                if (p_States.Has((*ptr).StateID)) {
                    state = p_States.Resolve((*ptr).StateID);
                    state.Change(
                        resourceID,
                        amount);
                    continue;
                }

                state = new ResourceState(resourceID, amount);
                int stateID = p_States.RegisterState(state);
                
                //apply
                (*(ptr)).StateID = stateID;
            }

            ptr++;
        }

    }
    private LinkedList<VisibleBlock> getVisibleBlocks(IRenderContext context) {
        LinkedList<VisibleBlock> buffer = new LinkedList<VisibleBlock>();
        
        //get the camera info
        Camera cam = p_Game.Camera;
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

        if (x >= p_Width || y >= p_Height) {
            return new LinkedList<VisibleBlock>();
        }

        //point the render x/y to the offset location.
        rX += offsetX * blockWidth;
        rY += offsetY * blockHeight;

        //read the matrix
        Block* ptr = p_Matrix + (offsetY * p_Width) + offsetX;
        Block* ptrEnd = p_Matrix + (p_Width * p_Height);

        Block* ptrLineEnd = p_Matrix + (offsetY * p_Width) + p_Width;
        
        if (ptr >= ptrEnd || ptr < p_Matrix) { return new LinkedList<VisibleBlock>(); }

        while (ptr <= ptrEnd) {
            //read the block
            Block* block = ptr++;

            //is the block visible?
            bool visible = true;
            if (rY >= screenHeight) { break; }
            if (rX >= screenWidth) {
                //skip line
                ptr = ptrLineEnd + offsetX;
                ptrLineEnd += p_Width;

                //reset x/y
                x = offsetX;
                rX = -camX + (offsetX * blockWidth);
                
                y++;
                rY += blockHeight;
                if (y == p_Height) { break; }
                continue;
            }
            if (rX + blockWidth <= 0 || rY + blockHeight <= 0) {
                visible = false;
            }

            //add to return if it is visible
            if (visible) {
                buffer.AddLast(new VisibleBlock() { 
                    RenderX = rX, 
                    RenderY = rY,
                    BlockX = x,
                    BlockY = y,
                    Block = block
                });
            }

            x++;
            rX += blockWidth;

            //x at end of row?
            if (x == p_Width) {                 
                x = offsetX; 
                y++;

                //end of the rows?
                if (y == p_Height) { break; }

                ptr = ptrLineEnd + offsetX;
                ptrLineEnd += p_Width;

                rX = -cam.X + (offsetX * blockWidth);
                rY += blockHeight;
            }
            
        }


        //filter fog?
        if (p_Game.EnableFog) {
            LinkedList<VisibleBlock> fogBuffer =
                p_Game.CurrentPlayer.Fog.Filter(
                    buffer);
            buffer = null;
            return fogBuffer;
        }

        return buffer;
    }
    private void updateConcreteMatrix() {
        Lock();

        //has the concrete matrix not been allocated?
        if (p_ConcreteMatrix == (bool*)0) {
            p_ConcreteMatrix = (bool*)Marshal.AllocHGlobal(
                p_Width * p_Height);
        }

        //just populate the concerete matrix depending on
        //it's block counterpart having a resource state id (basically not -1)
        bool* ptr = p_ConcreteMatrix;
        Block* blockPtr = p_Matrix;
        bool* ptrEnd = p_ConcreteMatrix + (p_Width * p_Height);
        while (ptr != ptrEnd) {
            *(ptr++) = (*(blockPtr++)).StateID != -1;
        }

        //clean up
        Unlock();

    }

    public BlockStates<ResourceState> Resources { get { return p_States; } }
    public LinkedList<VisibleBlock> VisibleBlocks { 
        get {
            lock (p_Mutex) {
                return p_VisibleBlocks;
            }
        } 
    }
    public int Width { get { return p_Width; } }
    public int Height { get { return p_Height; } }

    public Block this[int x, int y] {
        get {
            return *translateToPointer(x, y);
        }
        set {
            *translateToPointer(x, y) = value;
        }
    }

    public void Dispose() {
        Marshal.FreeHGlobal((IntPtr)(void*)p_Matrix);
        Marshal.FreeHGlobal((IntPtr)(void*)p_ConcreteMatrix);
    }
    ~Map() {
        Dispose();
    }
}
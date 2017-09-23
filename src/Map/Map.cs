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
using System.IO;
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
    private object p_Mutex = new object();

    private bool[,] load(string filename, out Point start, out Point end) {
        FileStream fs = new FileStream(filename, FileMode.Open);
        byte[] buffer = new byte[fs.Length];


        start = new Point(fs.ReadByte() << 8 | fs.ReadByte(), fs.ReadByte() << 8 | fs.ReadByte());
        end = new Point(fs.ReadByte() << 8 | fs.ReadByte(), fs.ReadByte() << 8 | fs.ReadByte());
        
        fs.Read(buffer, 0, 21 - 8);

        bool[,] matrix = new bool[1000, 1000];

        for (int y = 0; y < 1000; y++) {
            for (int x = 0; x < 1000; x++) {
                matrix[x, y] = fs.ReadByte() == 0;
            }
        }

        fs.Close();
        return matrix;
    }
    private void test() {
        Point st = Point.Empty;
        Point en = Point.Empty;
        
        bool[,] m = load("paths/maze.astar", out st, out en);

        
        //st = new Point(200, 0);
        //en = new Point(999, 999);

        //st = new Point(3, 0);
        //en = new Point(7, 9);

        for (int x = 0; x < 1000; x++) {
            for (int y = 0; y < 1000; y++) {
                Block* b = translateToPointer(x, y);
                (*b).TypeID = (short)(m[x, y] ? Globals.RESOURCE_WOOD : Globals.TERRAIN_GRASS);
            }
        }

        updateConcreteMatrix();

        IPathfinderContext ctx = Pathfinder.ASCreateContext(p_Width, p_Height);
        
        while (true) {
            int time = Environment.TickCount;
            List<Point> path = Pathfinder.ASSearch(
                ctx,
                st,
                en,
                p_ConcreteMatrix);

            Console.WriteLine((Environment.TickCount - time) + "ms for " + path.Count + " items");

            for (int c = 0; c < path.Count; c++) {
                Point p = path[c];
                Block* bl = translateToPointer(p.X, p.Y);
                (*bl).Selected = true;
            }
            break;

        }

    }

    public Map(Game game, int width, int height) {
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
            (*ptr).TypeID = 0;
            ptr++;
        }

        //generate resources
        generateMap();
        updateConcreteMatrix();

        pathfindBench();
    }

    private void pathfindBench() {
        int total = 0;
        int ticks = 0;
        while (true) {
            break;
            int time = Environment.TickCount;

            List<Point> path = Pathfinder.ASSearch(
                Point.Empty,
                new Point(p_Width - 1, p_Height - 1),
                p_ConcreteMatrix,
                p_Width,
                p_Height);

            time = Environment.TickCount - time;
            total += time;

            Console.WriteLine("Time: " + time + "ms for " + path.Count + " points");
            Console.WriteLine("Average: " + (total / (++ticks)) + "ms");
        }
    }

    public void Lock() { Monitor.Enter(p_Mutex); }
    public void Unlock() { Monitor.Exit(p_Mutex); }

    public bool* GetConcreteMatrix(bool onLand) {
        return p_ConcreteMatrix;
    }
    public Block* GetBlockMatrix() { return p_Matrix; }

    private Block* translateToPointer(int x, int y) {
        return p_Matrix + (y * p_Width) + x;
    }

    private void generateMap() {
        MapGenerator.GenerateTerrain(this);
        MapGenerator.GenerateResources(this);
    }
    private bool inRange(int value, int start, int end) {
        return
            value >= start &&
            value <= end;
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
        bool* ptrEnd = ptr + (p_Width * p_Height);
        Block* blockPtr = p_Matrix;
        while (ptr != ptrEnd) {
            Block block = *blockPtr++;

            (*ptr++) = block.TypeID >= Globals.TERRAIN_END;
        }

        //clean up
        Unlock();
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
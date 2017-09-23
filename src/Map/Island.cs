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
using System.Collections.Generic;
using System.Runtime.InteropServices;

public sealed unsafe class Island {
    private int p_X, p_Y;
    private int p_Width, p_Height;

    private List<IntPtr> p_Blocks;
    private Map p_Map;

    public Island(Map map, List<IntPtr> blocks, int x, int y, int width, int height) {
        p_X = x;
        p_Y = y;
        p_Width = width;
        p_Height = height;

        p_Blocks = blocks;
        p_Map = map;
    }

    public int X { get { return p_X; } }
    public int Y { get { return p_Y; } }
    public int Width { get { return p_Width; } }
    public int Height { get { return p_Height; } }


    public static List<Island> DetectIslands(Map map) {
        int width = map.Width;
        int height = map.Height;
        int blockCount = width * height;

        Block* matrix = map.GetBlockMatrix();

        //
        List<Island> islands = new List<Island>();

        //allocate and initialize the search matrix
        //so we can check which blocks we have already
        //added/searched.
        searchBlock* searchMatrix = (searchBlock*)Marshal.AllocHGlobal(
            sizeof(searchBlock) * blockCount);
        Block* matrixPtr = matrix;
        searchBlock* ptr = searchMatrix;
        searchBlock* ptrEnd = ptr + blockCount;
        Block* mapPtr = matrix;
        while (ptr != ptrEnd) {
            searchBlock* current = ptr++;
            (*current).searched = false;
            (*current).block = matrixPtr++;
        }
    
        //iterate through every block until we find every island
        ptr = searchMatrix;
       
        matrixPtr = matrix;
        int currentIslandIndex = 0;
        while (ptr != ptrEnd) {
            Block block = *matrixPtr++;
            
            //searched before?
            searchBlock* current = ptr++;
            if ((*current).searched) { continue; }

            //land?
            if (block.TypeID != Globals.TERRAIN_WATER) { 
                //we hit an island
                islandInfo info = floodSearch(
                    current,
                    currentIslandIndex++,
                    width,
                    searchMatrix,
                    ptrEnd);

                //
                Island i = new Island(
                    map,
                    info.blocks,
                    info.minX,
                    info.minY,
                    info.maxX - info.minX,
                    info.maxY - info.minY);
            }
        }

        return islands;
    }

    private static islandInfo floodSearch(searchBlock* block, int islandIndex, int rowWidth, searchBlock* start, searchBlock* end) {
        //create a queue to store pending blocks to
        //be processed.
        PtrQueue queue = new PtrQueue(10000);
        queue.Push(block);
        int count = 1;

        //count how many blocks we found
        int found = 0;

        //used to detect the size and location
        //of the island.
        int minX = 0, minY = 0;
        int maxX = 0, maxY = 0;
        minX = minY = int.MaxValue;

        //process every block
        List<IntPtr> blocks = new List<IntPtr>();
        while (count != 0) {
            searchBlock* ptr = (searchBlock*)queue.Pop();
            count--;

            //verify not out of bounds
            if (ptr < start || ptr >= end) { continue; }
            searchBlock deref = *ptr;

            //searched already?
            if (deref.searched) { continue; }

            //mark as processed
            (*ptr).searched = true;

            //we hit the water?
            Block b = (*deref.block);
            if (b.TypeID == Globals.TERRAIN_WATER) { continue; }
            found++;

            //add the block to the island
            blocks.Add((IntPtr)deref.block);

            //calculate x/y and do min/max to 
            //detect island size/location
            int x = (int)((long)(ptr - start) % rowWidth);
            int y = (int)((long)(ptr - start) / rowWidth);
            if (x < minX) { minX = x; }
            if (y < minY) { minY = y; }
            if (x > maxX) { maxX = x; }
            if (y > maxY) { maxY = y; }

            #region reiterate for all 8 neighbors
            /*top row*/
            queue.Push(ptr - rowWidth);
            queue.Push(ptr - rowWidth - 1);
            queue.Push(ptr - rowWidth + 1);

            /*bottom row*/
            queue.Push(ptr + rowWidth);
            queue.Push(ptr + rowWidth - 1);
            queue.Push(ptr + rowWidth + 1);

            /*left/right*/
            queue.Push(ptr - 1);
            queue.Push(ptr + 1);

            count += 8;
            #endregion
        }

        return new islandInfo { 
            count = found,
            minX = minX,
            minY = minY,
            maxX = maxX,
            maxY = maxY,
            blocks = blocks
        };
    }

    private struct islandInfo {
        public int minX, minY, maxX, maxY;
        public int count;
        public List<IntPtr> blocks;
    }
    private struct searchBlock {
        public bool searched;
        public Block* block;
    }
}
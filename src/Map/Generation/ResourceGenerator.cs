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

public static unsafe partial class MapGenerator {
    public static void GenerateResources(Map map) {
        Random seed = new Random();
        Block* matrix = map.GetBlockMatrix();
        int width = map.Width;
        int height = map.Height;

            
        /*gold*/
        generateResource(
            Globals.RESOURCE_GOLD,
            matrix,
            width, height,
            5,
            seed);
        
        /*stone*/
        generateResource(
            Globals.RESOURCE_STONE,
            matrix,
            width, height,
            5,
            seed);

        /*food*/
        generateResource(
            Globals.RESOURCE_FOOD,
            matrix,
            width, height,
            5,
            seed);
    }

    private static void generateResource(short type, Block* matrix, int width, int height, int maxCount, Random seed) {
        Block* ptr = matrix;
        Block* ptrEnd = matrix + (width * height);

        //iterate through every block
        while (ptr != ptrEnd) {
            Block* current = ptr++;

            //eligible for a resource?
            if ((*current).TypeID != Globals.TERRAIN_GRASS) {
                continue;
            }
            if (seed.Next(300) != 10) { continue; } /*1 in 100 chance of a resource*/

            Block* nextBlock = current;
            int currentCount = 0;
            int currentMaxCount = seed.Next(0, maxCount);
            while (currentCount != currentMaxCount) {                 
                //out of bounds?
                if (nextBlock < matrix ||
                    nextBlock >= ptrEnd) { break; }

                //place this resource
                if ((*nextBlock).TypeID == Globals.TERRAIN_GRASS) {
                    (*nextBlock).TypeID = type;
                }

                //go to the next resource
                int deltaX = 0;
                int deltaY = 0;
                while (deltaX == 0 && deltaY == 0) {
                    deltaX = seed.Next(-1, 1);
                    deltaY = seed.Next(-1, 1);

                    //do not allow diagonals.
                    if (deltaX != 0 && deltaY != 0) {
                        deltaX = 0;
                        deltaY = 0;
                    }
                }
                
                nextBlock += (deltaY * width) + deltaX;
                currentCount++;
            }
        }

    }
}
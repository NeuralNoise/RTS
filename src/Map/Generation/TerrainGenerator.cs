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

    public static void GenerateTerrain(Map map) {
        Block* matrix = map.GetBlockMatrix();
        int width = map.Width;
        int height = map.Height;


        //define the noise generators for the terrain/resource
        Random seed = new Random();
        PerlinNoise terrainNoise = new PerlinNoise(seed);
            /*new PerlinNoise(
            1,
            .1,
            1,
            1,
            seed.Next(0, int.MaxValue));
            */

        //use perlin noise to generate terrain (check if
        //the heighmap for each pixel is within a range for a resource
        Block* ptr = matrix;
        Block* ptrEnd = matrix + (width * height);
        int x = 0, y = 0;
        while (ptr != ptrEnd) {
            Block* block = (ptr++);

            //get the heightmap value for this pixel 
            //within a 1-100 scale
            double gen = terrainNoise.GetHeight(x, y, 50, 50);
            int value = (int)(Math.Abs(gen * 100));

            //deturmine the terrain block
            short blockType = Globals.TERRAIN_GRASS;
            if (value < 15) {
                blockType = Globals.TERRAIN_WATER;
                (*block).Height = (byte)(value * 1.0f * 2);
            }
            else { blockType = Globals.TERRAIN_GRASS; }

            //set the block type
            (*block).TypeID = blockType;

            //update x/y
            x++;
            if (x == width) {
                x = 0;
                y++;
            }
        }

    }


    private static bool inRange(int val, int min, int max) {
        return val >= min && val <= max;
    }
}
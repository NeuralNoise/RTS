/*
 *  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 *  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 *  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 *  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
 *  REMAINS UNCHANGED.
 *
 *  REPO: http://www.github.com/tomwilsoncoder/RTS
*/


public static class BlockType {
    public const short TERRAIN_END = 5;

    public const short TERRAIN_GRASS = 0;
    public const short TERRAIN_WATER = 1;

    #region Resources (range 5-15)
    public const short RESOURCE_START = 5;
    public const short RESOURCE_END = 15;

    public const short RESOURCE_WOOD = 5;
    public const short RESOURCE_FOOD = 6;
    public const short RESOURCE_STONE = 7;
    public const short RESOURCE_GOLD = 8;
    #endregion
}
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

public unsafe struct VisibleBlock {
    public Block* Block;

    public int BlockX;
    public int BlockY;

    public int RenderX;
    public int RenderY;

    /*only for drawing water!*/
    public List<Direction> ShadowDirection;
}
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
using System.Collections.Generic;

public class Entity : IAnimatePosition {
    private int p_X, p_Y;

    public bool Move(int dX, int dY) {
        return false;
    }
    public bool MoveAbs(int x, int y) {
        return Move(
            -p_X + x,
            -p_Y + y);
    }

    public int X { 
        get { return p_X; }
        set {
            MoveAbs(value, p_Y);
        }
    }
    public int Y {
        get { return p_Y; }
        set {
            MoveAbs(p_X, value);
        }
    }
}
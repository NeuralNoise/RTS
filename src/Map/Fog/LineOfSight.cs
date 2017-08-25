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

public class LineOfSight {

    private int p_X, p_Y;
    private int p_Radius;

    public LineOfSight(int centerX, int centerY, int radius) {
        p_X = centerX;
        p_Y = centerY;
        Radius = radius;
    }

    public int CenterX { 
        get { return p_X; }
        set { p_X = value; }
    }
    public int CenterY { 
        get { return p_Y; }
        set { p_Y = value; }
    }

    public int Radius {
        get { return p_Radius; }
        set {
            if (value < 3) { value = 3; }
            if (value > 20) { value = 20; }
            p_Radius = value;
        }
    }
}
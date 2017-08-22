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
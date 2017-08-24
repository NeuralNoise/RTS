/* 
 * This file is part of the RTS distribution (https://github.com/tomwilsoncoder/RTS)
 * 
 * This program is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
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
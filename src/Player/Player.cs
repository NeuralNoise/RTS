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

public class Player {
    private int p_Index;
    private Color p_Color; 
    private Fog p_Fog;

    public Player(Game game, int index, Color color) {
        p_Index = index;
        p_Color = color;

        p_Fog = new Fog(game.Map);
    }

    public int Index { get { return p_Index; } }
    public Color Color { get { return p_Color; } }
    public Fog Fog { get { return p_Fog; } }
}
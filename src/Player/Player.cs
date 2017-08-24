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
using System.Drawing;
using System.Collections.Generic;

public class Player {
    private int p_Index;
    private Color p_Color;
    private LinkedList<Unit> p_Units;
    private Fog p_Fog;

    public Player(Game game, int index, Color color) {
        p_Index = index;
        p_Color = color;

        p_Units = new LinkedList<Unit>();
        p_Fog = new Fog(game.Map);
    }

    public int Index { get { return p_Index; } }
    public Color Color { get { return p_Color; } }
    public Fog Fog { get { return p_Fog; } }
}
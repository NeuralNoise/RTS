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
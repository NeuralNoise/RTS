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

public class HUDMenu : UIContainer {

    private UILabel[] p_Resources = new UILabel[4];

    public HUDMenu(Game g) : base(g) {
        for (int c = 0; c < p_Resources.Length; c++) {
            p_Resources[c] = new UILabel("", new Font("Arial", 12f, FontStyle.Regular), g);
            AddControl(p_Resources[c]);
        }

        Location = Point.Empty;
        Height = 18;

        BackBrush = Brushes.Gray;
    }



    public override void Update() {
        for (int c = 0; c < p_Resources.Length; c++) {
            p_Resources[c].Text = 
                ResourceStockPile.GetName(c) + ": " + 
                ResourceStockPile.GetAmount(c).ToString("0");
        }

        int spacing = 10;
        int currentX = spacing;
        for (int c = 0; c < p_Resources.Length; c++) {
            p_Resources[c].Location = new Point(
                currentX,
                0);
            currentX += spacing + p_Resources[c].Width;
        }


        Width = Game.Window.ClientSize.Width;

        

        base.Update();
    }
}
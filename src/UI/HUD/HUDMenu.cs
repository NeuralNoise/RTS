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

public class HUDMenu : UIContainer {

    private UILabel[] p_Resources = new UILabel[4];

    public HUDMenu(Game g) : base(g) {
        for (int c = 0; c < p_Resources.Length; c++) {
            p_Resources[c] = new UILabel("", new Font("Arial", 10f, FontStyle.Bold), g);
            p_Resources[c].ForeBrush = Brushes.White;
            AddControl(p_Resources[c]);
        }

        Location = Point.Empty;
        Height = 20;

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
            UILabel label = p_Resources[c];
            label.Location = new Point(
                currentX,
                (int)Math.Floor((Height * 1.0f / 2) - (label.Height * 1.0f / 2)));
            currentX += spacing + p_Resources[c].Width;
        }


        Width = Game.Window.ClientSize.Width;

        

        base.Update();
    }
}
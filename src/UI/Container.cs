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
public class UIContainer : UIControl {
    private Brush p_BackBrush;


    public UIContainer(Game g) : base(g) { }

    public Brush BackBrush {
        get { return p_BackBrush; }
        set { p_BackBrush = value; }
    }

    public override void Draw(IRenderContext context, IRenderer renderer) {
        //draw background
        if (BackBrush != null) {
            renderer.SetBrush(p_BackBrush);
            renderer.FillQuad(
                X, Y, Width, Height);
        }

        //call draw for all children
        List<UIControl> children = Children;
        foreach (UIControl ctrl in children) {
            ctrl.Draw(context, renderer);
        }
    }
    public override void Update() {
        //call update for all children
        List<UIControl> children = Children;
        foreach (UIControl ctrl in children) {
            ctrl.Update();
        }
    }

}
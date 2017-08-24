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
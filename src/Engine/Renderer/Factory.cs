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
using System.Windows.Forms;
public static class RenderFactory {


    public static IRenderContext CreateContext(Graphics graphics, Size size) {
        return new GDIPRenderContext(
            graphics,
            size);
    }
    public static IRenderContext CreateContext(IntPtr hwnd) { 
        /*just return the GDI+ renderer for now.*/
        Control ctrl = Control.FromHandle(hwnd);

        return CreateContext(
           ctrl.CreateGraphics(),
           ctrl.ClientSize); 
    }
    public static IRenderer CreateRenderer() {
        /*just return the GDI+ renderer for now.*/
        return new GDIPRenderer();
    }

}
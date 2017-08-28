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
using System.Windows.Forms;
public static class RenderFactory {


    public static IRenderContext CreateContext(Graphics graphics, Size size) {
        //return new OpenGL.OpenGLContext(graphics, size);
        return new GDIPRenderContext(
            graphics,
            size);
    }
    public static IRenderContext CreateContext(IntPtr hwnd) { 
        /*just return the GDI+ renderer for now.*/
        Control ctrl = Control.FromHandle(hwnd);
        return new OpenGL.OpenGLContext(hwnd, ctrl.Size);

        return CreateContext(
           ctrl.CreateGraphics(),
           ctrl.ClientSize); 
    }
    public static IRenderer CreateRenderer() {
        return new OpenGL.OpenGLRenderer();

        /*just return the GDI+ renderer for now.*/
        return new GDIPRenderer();
    }

}
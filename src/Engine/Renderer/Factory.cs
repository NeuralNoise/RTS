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
using System.Runtime.InteropServices;

public static class RenderFactory {

    private static string p_Renderer = "opengl";

    public static IRenderContext CreateContext(Graphics graphics, Size size) {
        switch (p_Renderer) { 
            case "opengl":
                return new OpenGL.OpenGLContext(graphics, size);
            case "gdip":
            default:
                return new GDIPRenderContext(
                    graphics,
                    size);
        }
    }
    public static IRenderContext CreateContext(IntPtr hwnd) { 
        /*just return the GDI+ renderer for now.*/
        Control ctrl = Control.FromHandle(hwnd);
        
        switch(p_Renderer){
            case "opengl":
                return new OpenGL.OpenGLContext(hwnd, ctrl.ClientSize);
            case "gdi+":
            default:
                return CreateContext(
                   ctrl.CreateGraphics(),
                   ctrl.ClientSize);
        }
    }
    public static IRenderer CreateRenderer() {
        switch(p_Renderer){
            case "opengl":
                return new OpenGL.OpenGLRenderer();
            case "gdip":
            default:
                return new GDIPRenderer();
    }
    }



    public static void FailSafe() {
        //fallback on GDI+
        p_Renderer = "gdip";
    }
}
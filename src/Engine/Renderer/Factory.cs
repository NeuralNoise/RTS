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
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

public class GDIPRenderContext : IRenderContext, IDeviceContext {
    private Graphics p_Base;
    private int p_W, p_H;
    private bool p_Disposed;

    public GDIPRenderContext(Graphics ctx, Size size) { 
        p_Base = ctx;
        p_W = size.Width;
        p_H = size.Height;
    }

    public Graphics Graphics { get { return p_Base; } }

    public IFont AllocateFont(Font fnt) {
        throw new NotSupportedException();
    }
    public ITexture AllocateTexture(Bitmap bmp, string alias) {
        return new GDIPTexture(bmp);
    }
    public ITexture GetTexture(string alias) {
        throw new NotSupportedException();
    }

    public int Width { get { return p_W; } }
    public int Height { get { return p_H; } }

    public void Resize(int w, int h) {
        p_W = w;
        p_H = h;
    }

    public float DPIX { get { return p_Base.DpiX; } }
    public float DPIY { get { return p_Base.DpiY; } }

    public bool IsDisposed { get { return p_Disposed; } }
    public void Dispose() {
        if (p_Disposed) { return; }
        p_Base.Dispose();
        p_Disposed = true;
    }

    public IntPtr GetHdc() {
        return p_Base.GetHdc();
    }
    public void ReleaseHdc() {
        p_Base.ReleaseHdc();
    }
}
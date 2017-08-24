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
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
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;


using System.Windows.Forms; /*Specifically for MeasureText function*/

public class GDIPRenderer : IRenderer {
    private IRenderContext p_Context;
    
    private Bitmap p_Frame;
    private Graphics p_FrameBuffer;

    private bool p_InFrame;

    private Brush p_Brush;
    private Pen p_Pen;
    private Color p_Color;
    private Font p_Font;

    private bool p_Buffered;

    private object p_Mutex = new object();

    public IRenderContext Context { get { return p_Context; } }
    public Graphics FrameBuffer { get { return p_FrameBuffer; } }

    public void BeginFrame(IRenderContext ctx) {
        p_Context = ctx;
        p_InFrame = true;

        //create a new buffer?
        if (p_Frame == null ||
           ctx.Width != p_Frame.Width ||
           ctx.Height != p_Frame.Height) {
               p_Buffered = true;
               p_Frame = new Bitmap(ctx.Width, ctx.Height, PixelFormat.Format32bppPArgb);
               p_FrameBuffer = Graphics.FromImage(p_Frame);

               p_FrameBuffer.SmoothingMode = SmoothingMode.HighQuality;
               p_FrameBuffer.InterpolationMode = InterpolationMode.HighQualityBilinear;
               p_FrameBuffer.TextRenderingHint = TextRenderingHint.AntiAlias;
        }

       
    }
    public void BeginFrame(IRenderContext ctx, bool buffered) {
        if (buffered) {
            BeginFrame(ctx);
            return;
        }

        p_InFrame = true;
        p_Context = ctx;
        p_FrameBuffer = (ctx as GDIPRenderContext).Graphics;
    }
    public void EndFrame() {
        p_InFrame = false;
        if (!p_Buffered) {
            p_FrameBuffer.Flush();
            return; 
        }

        //draw the frame buffer to the context
        try {
            (p_Context as GDIPRenderContext).Graphics.DrawImageUnscaled(
                p_Frame,
                0, 0);
        }
        catch (Exception) { }

        //clean up
        p_Context = null;
    }

    public void SetContext(IRenderContext ctx) {
        if (p_InFrame) {
            throw new Exception("Cannot change context during a frame render!");
        }
        p_Context = ctx;
    }

    public void Lock() {
        Monitor.Enter(p_Mutex);
    }
    public void Unlock() {
        Monitor.Exit(p_Mutex);
    }

    public void SetBrush(Brush brush) { p_Brush = brush; }
    public void SetPen(Pen pen) { p_Pen = pen; }
    public void SetColor(Color color) { p_Color = color; }
    public void SetFont(Font font) { p_Font = font; }

    public void Clear() {
        p_FrameBuffer.Clear(p_Color);
    }

    public void DrawQuad(int x, int y, int w, int h) {
        p_FrameBuffer.DrawRectangle(
            p_Pen,
            x, y, w, h);
    }

    public void FillQuad(int x, int y, int w, int h) {
        p_FrameBuffer.FillRectangle(
            p_Brush,
            x, y, w, h);
    }

    public void DrawPoly(Point[] poly) {
        p_FrameBuffer.DrawPolygon(
            p_Pen,
            poly);
    }
    public void FillPoly(Point[] poly) {
        p_FrameBuffer.FillPolygon(
            p_Brush,
            poly);
    }

    public void DrawEllipse(int x, int y, int w, int h) {
        p_FrameBuffer.DrawEllipse(
            p_Pen,
            x, y, w, h);
    }
    public void FillEllipse(int x, int y, int w, int h) {
        p_FrameBuffer.FillEllipse(
            p_Brush,
            x, y, w, h);
    }

    public void DrawPath(GraphicsPath path) {
        p_FrameBuffer.DrawPath(
            p_Pen,
            path);
    }
    public void FillPath(GraphicsPath path) {
        p_FrameBuffer.FillPath(
            p_Brush,
            path);
    }

    public void DrawString(string str, int x, int y) {
        p_FrameBuffer.DrawString(
            str,
            p_Font,
            p_Brush,
            x, y);
    }
    public Size MeasureString(string str) {
        return TextRenderer.MeasureText(
            str,
            p_Font);
    }

    public void DrawImage(Bitmap bmp, int x, int y, int w, int h) {
        p_FrameBuffer.DrawImage(
            bmp,
            x, y, w, h);
    }
    public void DrawImageUnscaled(Bitmap bmp, int x, int y) {
        p_FrameBuffer.DrawImageUnscaled(
            bmp,
            x, y);
    }


    public override string ToString() {
        return "GDI+" + typeof(Graphics).Assembly.ImageRuntimeVersion;
    }
}
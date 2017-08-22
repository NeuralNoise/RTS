using System;
using System.Drawing;
using System.Drawing.Drawing2D;

public interface IRenderer {
    
    
    void BeginFrame(IRenderContext context);
    void BeginFrame(IRenderContext context, bool doubleBuffered);
    void EndFrame();

    void Lock();
    void Unlock();

    void SetColor(Color color);
    void SetBrush(Brush brush);
    void SetPen(Pen pen);
    void SetFont(Font font);

    void Clear();

    void DrawQuad(int x, int y, int w, int h);
    void FillQuad(int x, int y, int w, int h);

    void DrawPoly(Point[] p);
    void FillPoly(Point[] p);

    void DrawEllipse(int x, int y, int w, int h);
    void FillEllipse(int x, int y, int w, int h);

    void DrawPath(GraphicsPath path);
    void FillPath(GraphicsPath path);

    void DrawString(string str, int x, int y);
    Size MeasureString(string str);

    void DrawImage(Bitmap btm, int x, int y, int w, int h);
    void DrawImageUnscaled(Bitmap btm, int x, int y);

    IRenderContext Context { get; }
}
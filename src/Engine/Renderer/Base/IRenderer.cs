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
using System.Drawing.Drawing2D;

public interface IRenderer {
    
    
    void BeginFrame(IRenderContext context);
    void BeginFrame(IRenderContext context, bool doubleBuffered);
    void EndFrame();

    void Lock();
    void Unlock();

    void SetTexture(ITexture texture);
    void SetFont(Font font);
    void SetColor(Color color);
    void SetBrush(Brush brush);
    void SetPen(Pen pen);

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
    Size MeasureString(string str, Font font);

    void DrawTexture(int x, int y, int w, int h);
    void DrawTextureUnscaled(int x, int y);

    IRenderContext Context { get; }

    /// <summary>
    /// Returns the number of vertices on-screen.
    /// </summary>
    int Vertices { get; }

    void SetContext(IRenderContext ctx);
}
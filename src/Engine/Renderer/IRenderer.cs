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

    void SetContext(IRenderContext ctx);
}
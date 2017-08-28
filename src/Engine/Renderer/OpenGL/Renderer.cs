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
using System.Threading;

public static partial class OpenGL {
    public class OpenGLRenderer : IRenderer {
        private OpenGLContext p_Context;
        private object p_Mutex = new object();

        public void Lock() { Monitor.Enter(p_Mutex); }
        public void Unlock() { Monitor.Exit(p_Mutex); }

        private bool p_HasContext = false;

        public void BeginFrame(IRenderContext context) {
            if (!(context is OpenGLContext)) {
                throw new Exception("Invalid OpenGL context!");
            }

            p_Context = context as OpenGLContext;

            //swap to OpenGL context
            if (!p_HasContext) {
                wglMakeCurrent(
                    p_Context.DeviceContext,
                    p_Context.RenderContext);
                p_HasContext = true;
            }
            glEnable(BLEND);
        }
        public void BeginFrame(IRenderContext context, bool doubleBuffer) {
            BeginFrame(context);
        }

        public void SetContext(IRenderContext context) {
            throw new Exception("Unable");
        }

        public void EndFrame() {
            SwapBuffers(p_Context.DeviceContext);
        }

        public void SetTexture(Bitmap texture) { }
        public void SetFont(Font font) { }
        public void SetPen(Pen pen) { }
        public void SetBrush(Brush brush) {
            if (brush is SolidBrush) {
                SetColor((brush as SolidBrush).Color);
            }
            else {
                SetColor(Color.Red);
            }
        }
        public void SetColor(Color color) {
            glColor4ub(color.R, color.G, color.B, color.A);
        }

        public void Clear() {
            glBegin(QUADS);
            glVertex2f(-1f, -1f);
            glVertex2f(1f, -1f);
            glVertex2f(1f, 1f);
            glVertex2f(-1f, 1f);
            glEnd();
        }

        public void DrawQuad(int x, int y, int width, int height) { }
        public void FillQuad(int x, int y, int width, int height) {
            float xf, yf, wf, hf;
            PointF l = translate(new Point(x, y));
            PointF s = translate(new Point(x + width, y + height));
            xf = l.X; yf = l.Y;
            wf = s.X; hf = s.Y;


            glBegin(QUADS);
            glVertex2f(xf, yf);
            glVertex2f(wf, yf);
            glVertex2f(wf, hf);
            glVertex2f(xf, hf);

            glEnd();
        }

        public void DrawEllipse(int x, int y, int width, int height) { }
        public void FillEllipse(int x, int y, int width, int height) { }

        public void DrawString(string txt, int x, int y) { }

        public Size MeasureString(string str) { return Size.Empty; }

        public void DrawPoly(Point[] p) { }
        public void FillPoly(Point[] points) {

            glBegin(POLYGON);

            foreach (Point p in points) {

                PointF trans = translate(p);

                glVertex2f(
                    trans.X,
                    trans.Y);
            
            }

            glEnd();
            return;

        
        }

        public void DrawPath(GraphicsPath path) { }
        public void FillPath(GraphicsPath path) { }

        public void DrawTexture(int x, int y, int width, int height) { }
        public void DrawTextureUnscaled(int x, int y) { }


        public IRenderContext Context { get { return p_Context; } }


        private PointF translate(Point p) {
            float x = 0;
            float y = 0;


            x = ((p.X * 1.0f / p_Context.Width) * 2) - 1;
            y = 1 - ((p.Y * 1.0f / p_Context.Height) * 2);


            return new PointF(x, y);
        }


        public override string ToString() {
            int major, minor;

            //get opengl version
            glGetIntegerv(MAJOR_VERSION, out major);
            glGetIntegerv(MINOR_VERSION, out minor);

            return "OpenGL " + p_Context.Version;
        }
    }
}
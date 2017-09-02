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

using System.Runtime.InteropServices;
public static partial class OpenGL {
    public unsafe class OpenGLRenderer : IRenderer {
        private OpenGLContext p_Context;
        private object p_Mutex = new object();

        private int p_CurrentVertCount;
        private int p_TotalVertCount;

        public void Lock() { Monitor.Enter(p_Mutex); }
        public void Unlock() { Monitor.Exit(p_Mutex); }

        private bool p_HasContext = false;

        public void BeginFrame(IRenderContext context) {
            if (!(context is OpenGLContext)) {
                throw new Exception("OpenGL: Invalid context!");
            }

            p_Context = context as OpenGLContext;

            //swap to OpenGL context
            if (!p_HasContext) {
                wglMakeCurrent(
                    p_Context.DeviceContext,
                    p_Context.RenderContext);
                p_HasContext = true;
            }

            //resize?
            if (p_Context.SizeChanged) {
                int width = p_Context.Width, height = p_Context.Height;
                int centerX = width / 2, centerY = height / 2;

                glViewport(0, 0, width, height);
                glOrtho(-centerX, centerX, -centerY, centerY, -100, 100);
            }

            //reset tri count
            p_CurrentVertCount = 0;

            //setup orthoganol matrix so we are using screen coords instead 
            //of world coords
            glLoadIdentity();
            gluOrtho2D(0, p_Context.Width, p_Context.Height, 0);
            glMatrixMode(MODELVIEW);
            glPushMatrix();
            glLoadIdentity();
        }
        public void BeginFrame(IRenderContext context, bool doubleBuffer) {
            BeginFrame(context);
        }

        public void SetContext(IRenderContext context) {
            if (!(context is OpenGLContext)) {
                throw new Exception("OpenGL: Invalid context!");
            }
            p_Context = (OpenGLContext)context;
        }

        public void EndFrame() {
            glMatrixMode(PROJECTION);
            glPopMatrix();

            p_TotalVertCount = p_CurrentVertCount;

           
            SwapBuffers(p_Context.DeviceContext);
        }

        private OpenGLFont p_Font;
        private OpenGLTexture p_Texture;
        public void SetTexture(string alias) {
            SetTexture(p_Context.GetTexture(alias));
        }
        public void SetTexture(ITexture texture) {
            //valid?
            if (!(texture is OpenGLTexture)) {
                throw new Exception("OpenGL: Invalid texture!");
            }
            p_Texture = texture as OpenGLTexture;
        }
        public IFont SetFont(Font font) {
            p_Font = (OpenGLFont)p_Context.AllocateFont(font);
            return p_Font;
        }

        public void SetPen(Pen pen) {
            glLineWidth(pen.Width);
            SetColor(pen.Color);
        }
        public void SetBrush(Brush brush) {
            if (brush is SolidBrush) {
                SetColor((brush as SolidBrush).Color);
            }
            else {
                SetColor(Color.Transparent);
            }
        }
        public void SetColor(Color color) {
            glColor4ub(color.R, color.G, color.B, color.A);
        }

        public void Clear() {            
            //just fill a quad the size of the screen.
            FillQuad(0, 0, p_Context.Width, p_Context.Height);
        }

        public void DrawQuad(int x, int y, int width, int height) {
            glBegin(LINES); {
                /*top*/
                glVertex2f(x, y);
                glVertex2f(x + width, y);

                /*right*/
                glVertex2f(x + width, y);
                glVertex2f(x + width, y + height);

                /*bottom*/
                glVertex2f(x + width, y + height);
                glVertex2f(x, y + height);

                /*left*/
                glVertex2f(x, y + height);
                glVertex2f(x, y);
            }
            glEnd();

            p_CurrentVertCount += 8;
        }
        public void FillQuad(int x, int y, int width, int height) {
            glBegin(QUADS); {
                glVertex2f(x, y);
                glVertex2f(x + width, y);
                glVertex2f(x + width, y + height);
                glVertex2f(x, y + height);
            }

            glEnd();

            p_CurrentVertCount += 4;
        }

        public void DrawEllipse(int x, int y, int width, int height) { }
        public void FillEllipse(int x, int y, int width, int height) { }

        public void DrawString(string txt, int x, int y) {
            if (txt.Length == 0) { return; }

            //get the font selected.
            OpenGLFont font = p_Font;

            //is there any lines?
            if (txt.Contains("\n")) { 
                
                //recall DrawString per line since OpenGL
                //bitmap calllists do not support multi-line
                int currentY = y;
                string[] lines = txt.Split('\n');
                int lineLength = lines.Length;
                for (int c = 0; c < lineLength; c++) {
                    DrawString(
                        lines[c],
                        x,
                        currentY);

                    //move y to the start of the next line
                    currentY += font.METRIC.tmAscent;
                }

                return;
            }

            /*
                since OpenGL will render the bottom of the text
                along the y coord we give it, we need to offset
                it by the font height - the gap between the top
                of quad and the character.
            */
            y += font.METRIC.tmAscent;
            
            glRasterPos2f(x, y);

            //jump to the start of the compiled GL list for 
            //rendering the font bitmaps.
            glListBase(font.LIST);

            //call it.
            glCallLists(
                txt.Length,
                UNSIGNED_BYTE,
                txt);

            /*we assume every character is drawn as a textured quad...*/
            p_CurrentVertCount += (txt.Length << 2); // (*4)
        }
        public Size MeasureString(string str, IFont f) {
            //empty?
            if (String.IsNullOrEmpty(str)) { return Size.Empty; }

            //get the OpenGL font for this font.
            OpenGLFont font = (OpenGLFont)f;

            //calculate string height from line count
            string[] lines = str.Split('\n');
            int lineCount = lines.Length;
            int maxWidth = 0;

            //find the largest width of all the lines
            for (int y = 0; y < lineCount; y++) {

                //get the line render width.
                int lineWidth = 0;
                fixed (char* line = lines[y].ToCharArray()) {
                    //empty?
                    if (line == (char*)0) { continue; }

                    int lineLength = lines[y].Length;
                    char* ptr = line;
                    char* ptrEnd = ptr + lineLength;

                    while (ptr != ptrEnd) {
                        ABC g = font.GLYPHINFO[(int)(*ptr++)];
                        lineWidth +=
                            g.abcA +
                            (int)g.abcB +
                            g.abcC;
                    }
                }

                //largest width so far?
                if (lineWidth > maxWidth) {
                    maxWidth = lineWidth;
                        
                }

            }

            return new Size(
                maxWidth,
                font.METRIC.tmAscent * lineCount);
        }
        
        public int GetFontHeight(IFont font) {
            return ((OpenGLFont)font).METRIC.tmAscent;
        }
        public int GetCharWidth(char ch, IFont font) {
            OpenGLFont f = (OpenGLFont)font;
            ABC abc = f.GLYPHINFO[(int)ch];
            return abc.abcA +
                   (int)abc.abcB +
                   abc.abcC;
        }

        public void DrawPoly(Point[] points) {
            int l = points.Length;
            glBegin(LINE_LOOP); {                
                for (int c = 0; c < l; c++) {
                    Point p = points[c];
                    glVertex2f(
                        (float)p.X,
                        (float)p.Y);
                }
            }
            glEnd();

            p_CurrentVertCount += l;

        }
        public void FillPoly(Point[] points) {
            int l = points.Length;

            glBegin(POLYGON); {
                for (int c = 0; c < l; c++) {
                    Point p = points[c];
                    glVertex2f(
                        p.X,
                        p.Y);
                }
            }
            glEnd();

            p_CurrentVertCount += l;
        }

        public void DrawPath(GraphicsPath path) { }
        public void FillPath(GraphicsPath path) { }

        public void DrawTexture(int x, int y, int width, int height) {
            glEnable(TEXTURE_2D);

            glColor3f(1, 1, 1);
            glBindTexture(TEXTURE_2D, p_Texture.INDEX);

            /*draw quad for the texture*/
            glBegin(QUADS); {
                glTexCoord2f(0, 0);
                glVertex2f(x, y);

                glTexCoord2f(1, 0);
                glVertex2f(x + width, y);

                glTexCoord2f(1, 1);
                glVertex2f(x + width, y + height);

                glTexCoord2f(0, 1);
                glVertex2f(x, y + height);
            }
            glEnd();


            //clean up
            glDisable(TEXTURE_2D);
            p_CurrentVertCount += 4;

        }
        public void DrawTextureUnscaled(int x, int y) { 
            //just call drawtexture with the width/height of the 
            //assigned texture size.
            DrawTexture(
                x, y,
                p_Texture.Width,
                p_Texture.Height);

        }

        public IRenderContext Context { get { return p_Context; } }
        public int Vertices { get { return p_TotalVertCount; } }

        public override string ToString() {
            int major, minor;

            //get opengl version
            glGetIntegerv(MAJOR_VERSION, out major);
            glGetIntegerv(MINOR_VERSION, out minor);

            return "OpenGL " + p_Context.Version;
        }
    }
}
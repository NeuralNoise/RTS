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
using System.Drawing.Imaging;
using System.Threading;

public static unsafe partial class OpenGL {
    public class OpenGLContext : IRenderContext {
        private IntPtr p_Handle;
        private IntPtr p_DeviceContext;
        private IntPtr p_RenderContext;
        private Version p_Version;

        private OpenGLFont[] p_Fonts = new OpenGLFont[0];
        private OpenGLTexture[] p_Textures = new OpenGLTexture[0];

        private object p_Mutex = new object();

        private int p_Width, p_Height;
        private bool p_ResizeUpdate;

        public OpenGLContext(Graphics g, Size size) : this(fromGraphics(g), size) { }
        public OpenGLContext(IntPtr hwnd, Size size) {
            p_Handle = hwnd;

            //do not allow hwnd of zero (it get's the screen)
            if (hwnd == IntPtr.Zero) {
                throw new Exception("OpenGL: Invalid window handle!");
            }

            p_Width = size.Width;
            p_Height = size.Height;

            //get the device context
            p_DeviceContext = GetDC(p_Handle);

            //create the pixel format for the rendering context
            PIXELFORMATDESCRIPTOR format = new PIXELFORMATDESCRIPTOR() {
                nSize = 40, /*size of struct in memory*/
                nVersion = 1,
                dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER,
                iPixelType = PFD_TYPE_RGBA,
                cColorBits = 24, /*24 bits per pixel*/
                cDepthBits = 32,
                iLayerType = PFD_MAIN_PLANE
            };

            
            //attempt to register the pixel format
            int match = ChoosePixelFormat(p_DeviceContext, format);
            if (match == 0) {
                throw new Exception("OpenGL: Unable to discover a compatable pixel format. [" + GetLastError() + "]");
            }
            if (SetPixelFormat(p_DeviceContext, match, format) == 0) {
                throw new Exception("OpenGL: Unable to set pixel format. [" + GetLastError() + "]");
            }

            //create the opengl rendering context
            p_RenderContext = wglCreateContext(p_DeviceContext);

            //discover opengl version
            wglMakeCurrent(p_DeviceContext, p_RenderContext);
            int major, minor;
            glGetIntegerv(MAJOR_VERSION, out major);
            glGetIntegerv(MINOR_VERSION, out minor);
            p_Version = new Version(major, minor);

            //enable transparency
            glEnable(BLEND);
            glBlendFunc(SRC_ALPHA, ONE_MINUS_SRC_ALPHA);

            //switch out of the opengl render so the
            //rendering thread can own it.
            wglMakeCurrent(p_DeviceContext, IntPtr.Zero);
        }

        public void Resize(int width, int height) {
            p_Width = width;
            p_Height = height;

            //tell render thread to resize.
            p_ResizeUpdate = true;
        }

        public IFont AllocateFont(Font font) {
            Monitor.Enter(p_Mutex);

            //create a hash code for this font.
            int fontHash = createFontHash(font);

            //does this font already exist?
            int fontListLength = p_Fonts.Length;
            for (int c = 0; c < fontListLength; c++) {
                if (fontHash == p_Fonts[c].HASH) { 
                    //return the font
                    Monitor.Exit(p_Mutex);
                    return p_Fonts[c];
                }
            }

            /*select the font on the GDI stack*/
            IntPtr hfont = font.ToHfont();
            SelectObject(p_DeviceContext, hfont);

            /*create the internal struct we use to handle this fonts rendering.*/
            OpenGLFont buffer = new OpenGLFont { 
                LIST = glGenLists(256),
                HFONT = hfont,
                HASH = fontHash,
                GLYPHINFO = new ABC[256]
            };


            bool success = false;
            string errorString = "";
            /*get the initial gl bitmap lists for the font*/
            if (!wglUseFontBitmaps(p_DeviceContext, 0, 256, buffer.LIST)) {
                errorString = "OpenGL: Unable to create font bitmap! (" + GetLastError() + ")";
            }
            /*get the width information*/
            else if (!GetCharABCWidths(p_DeviceContext, 0, 255, buffer.GLYPHINFO)) {
                errorString = "OpenGL: Cannot retrieve font character width information!";
            }
            /*get the height information*/
            else if (!GetTextMetrics(p_DeviceContext, out buffer.METRIC)) {
                errorString = "OpenGL: Cannot retrieve font metrics";
            }
            else { success = true; }
            
            //success?
            if (!success) {
                Monitor.Exit(p_Mutex);
                throw new Exception(errorString);
            }

            //add this font to the stack
            Array.Resize(ref p_Fonts, p_Fonts.Length + 1);
            p_Fonts[p_Fonts.Length - 1] = buffer;
            Console.WriteLine("Added font: " + font);
            Monitor.Exit(p_Mutex);
            return buffer;
        }
        public ITexture AllocateTexture(Bitmap bmp, string alias) {
            Monitor.Enter(p_Mutex);
            Size size = bmp.Size;

            //has the texture already been defined?
            
            ITexture exist = GetTexture(alias);
            if (exist != null) {
                Monitor.Exit(p_Mutex);
                return exist;
            }

            //create the texture index
            int index;
            glGenTextures(1, out index);
            glBindTexture(TEXTURE_2D, index);
            
            //lock the bitmap so we can access its memory directly
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(Point.Empty, size),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            //copy bitmap data to the GPU
            glTexImage2D(
                TEXTURE_2D,
                0,
                RGBA,
                size.Width,
                size.Height,
                0,
                RGBA,
                UNSIGNED_BYTE,
                bmpData.Scan0);

            //clean up
            bmp.UnlockBits(bmpData);


            glTexParameteri(TEXTURE_2D, TEXTURE_MIN_FILTER, LINEAR);
            glTexParameteri(TEXTURE_2D, TEXTURE_MAG_FILTER, LINEAR);
            glTexParameteri(TEXTURE_2D, TEXTURE_WRAP_S, REPEAT);
            glTexParameteri(TEXTURE_2D, TEXTURE_WRAP_T, REPEAT);


            //add the texture
            OpenGLTexture buffer = new OpenGLTexture {
                Width = size.Width,
                Height = size.Height,
                INDEX = index,
                HASH = alias.GetHashCode()
            };

            Array.Resize(ref p_Textures, p_Textures.Length + 1);
            p_Textures[p_Textures.Length - 1] = buffer;
            Monitor.Exit(p_Mutex);
            return buffer;
        }

        public ITexture GetTexture(string alias) {
            //generate a hash for the alias so 
            //we can compare to other textures quickly
            int aliasHash = alias.GetHashCode();

            lock (p_Mutex) { 
                //look for the hash
                int l = p_Textures.Length;
                for (int c = 0; c < l; c++) {
                    OpenGLTexture texture = p_Textures[c];
                    if (texture.HASH == aliasHash) {
                        return texture;
                    }
                }
            }

            //not found
            return null;            
        }

        public int Width { get { return p_Width; } }
        public int Height { get { return  p_Height; } }

        public float DPIX { get { return 96; } }
        public float DPIY { get { return 96; } }

        public bool SizeChanged { 
            get {
                bool buffer = p_ResizeUpdate;
                p_ResizeUpdate = false;
                return buffer;
            }
        }
        public bool IsDisposed { get { return p_DeviceContext == IntPtr.Zero; } }

        public Graphics Graphics { get { return null; } }

        public IntPtr RenderContext { get { return p_RenderContext; } }
        public IntPtr DeviceContext { get { return p_DeviceContext; } }

        public IntPtr GetHdc() { return p_DeviceContext; }

        public Version Version { get { return p_Version; } }

        public void ReleaseHdc() {
            ReleaseDC(p_Handle, p_DeviceContext);
        }

        public void Dispose() {
            //deselect the rendering context
            //so we can destory it.
            IntPtr current = wglGetCurrentContext();
            if (current == p_RenderContext) {
                wglMakeCurrent(p_DeviceContext, IntPtr.Zero);
            }
           
            //destroy
            wglDeleteContext(p_RenderContext);

            //destroy device context
            bool success = DeleteDC(p_DeviceContext);
            
            p_DeviceContext = IntPtr.Zero;
        }

        private static IntPtr fromGraphics(Graphics g) { 
            //get the window handle 
            IntPtr dc = g.GetHdc();
            IntPtr hwnd = WindowFromDC(dc);

            if (hwnd == IntPtr.Zero) {
                throw new Exception("OpenGL: Unable to resolve window handle from Graphics object.");
            }

            return hwnd;
        }

        private int createFontHash(Font font) {
            //Style = (log2(8) = 3bits
            //Size =

            /*
                this function does not need to worry about
                hash collisions as there is no point in doing
                so. It is just to check if a font is already
                being used.
            */

            //generate a hash for the font name
            int hash = font.Name.GetHashCode();

            //just add the hash of the font size
            //to the name hash (in most scenarios,
            //the difference in either would make
            //a different hash.
            hash += font.Size.GetHashCode();

            //cut off all but 1 byte of the hash 
            //so we can fill in with the font size.
            hash &= 0x00ffffff;

            //add font style (which is 3 bits to the top end)
            hash |= ((byte)font.Style << 31);
            return hash;
        }

    }
}
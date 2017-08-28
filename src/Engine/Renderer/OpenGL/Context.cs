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
using System.Threading;

public static partial class OpenGL {
    public class OpenGLContext : IRenderContext {
        private IntPtr p_Handle;
        private IntPtr p_DeviceContext;
        private IntPtr p_RenderContext;
        private Version p_Version;

        private int p_Width, p_Height;

        private bool p_IsDisposed;

        public OpenGLContext(IntPtr hwnd, Size size) {
            p_Handle = hwnd;
            p_Width = size.Width;
            p_Height = size.Height;


            //get the device context
            p_DeviceContext = GetDC(hwnd);

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
                throw new Exception("OpenGL: Unable to discover a pixel format. [" + GetLastError() + "]");
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

            //switch out of the opengl render so the
            //rendering thread can own it.
            wglMakeCurrent(p_DeviceContext, IntPtr.Zero);
        }

        public void Resize(int width, int height) {
            p_Width = width;
            p_Height = height;
        }

        public int Width { get { return p_Width; } }
        public int Height { get { return  p_Height; } }

        public float DPIX { get { return 92; } }
        public float DPIY { get { return 92; } }

        public bool IsDisposed { get { return p_IsDisposed; } }

        public Graphics Graphics { get { return null; } }

        public IntPtr RenderContext { get { return p_RenderContext; } }
        public IntPtr DeviceContext { get { return p_DeviceContext; } }

        public IntPtr GetHdc() { return p_DeviceContext; }

        public Version Version { get { return p_Version; } }

        public void ReleaseHdc() {
            throw new Exception("Not supported");
        }

        public void Dispose() {
            p_IsDisposed = true;


        }
    }
}
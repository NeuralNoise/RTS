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
using System.Runtime.InteropServices;



public static unsafe partial class OpenGL {
    #region kernel32 imports
    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string lpFileName);
    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();
    [DllImport("kernel32.dll")]
    public static extern int MulDiv(int n, int numerator, int denominator);
    #endregion

    #region gdi32 imports
    [DllImport("gdi32.dll", SetLastError = true)]
    public unsafe static extern int ChoosePixelFormat(IntPtr hDC, PIXELFORMATDESCRIPTOR ppfd);
    [DllImport("gdi32.dll", SetLastError = true)]
    public unsafe static extern int SetPixelFormat(IntPtr hDC, int iPixelFormat, PIXELFORMATDESCRIPTOR ppfd);
    [DllImport("gdi32.dll")]
    public static extern int SwapBuffers(IntPtr hDC);
    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateFont(int h, int w, int e, int o, FontWeight weight,
                                           bool italic, bool underline, bool strikeout,
                                           FontCharSet charset, FontPrecision outPrecision, FontClipPrecision clipPrecision,
                                           FontQuality quality, FontPitchAndFamily pitchAFam, string face);

    [DllImport("gdi32.dll")]
    public static extern bool GetCharABCWidths(IntPtr dc, uint firstChar, uint lastChar, [Out] ABC[] lpabc);

    [DllImport("gdi32.dll")]
    public static extern bool GetTextMetrics(IntPtr dc, out TEXTMETRIC metric);

    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateDIBSection(
        IntPtr hdc,
        BITMAPINFO pbmi,
        uint iUsage,
        out IntPtr ppvBits,
        IntPtr hSection,
        uint dwOffset);
    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll", SetLastError = true)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
    [DllImport("gdi32.dll")]
    public static extern bool DeleteDC(IntPtr hDC);
    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    #endregion

    #region	opengl imports
    [DllImport("opengl32.dll")]
    public static extern void glGetIntegerv(uint name, out int value);

    [DllImport("opengl32.dll")]
    public static extern int wglMakeCurrent(IntPtr hdc, IntPtr hrc);
    [DllImport("opengl32.dll")]
    public static extern IntPtr wglCreateContext(IntPtr hdc);
    [DllImport("opengl32.dll")]
    public static extern IntPtr wglGetCurrentDC();
    [DllImport("opengl32.dll")]
    public static extern IntPtr wglGetCurrentContext();

    [DllImport("opengl32.dll")]
    public static extern bool wglUseFontBitmaps(IntPtr hdc, uint first, uint count, uint list);

    [DllImport("opengl32", EntryPoint = "wglUseFontOutlines", CallingConvention = CallingConvention.Winapi)]
    public static extern bool wglUseFontOutlines(
    IntPtr hDC,
    [MarshalAs(UnmanagedType.U4)] UInt32 first,
    [MarshalAs(UnmanagedType.U4)] UInt32 count,
    [MarshalAs(UnmanagedType.U4)] UInt32 listBase,
    [MarshalAs(UnmanagedType.R4)] Single deviation,
    [MarshalAs(UnmanagedType.R4)] Single extrusion,
    [MarshalAs(UnmanagedType.I4)] Int32 format,
    [Out]GLYPHMETRICSFLOAT[] lpgmf);


    [DllImport("opengl32.dll")]
    public static extern bool wglDeleteContext(IntPtr hrc);

    [DllImport("opengl32.dll")]
    public static extern void glOrtho(float left, float right, float bottom, float top, float nearVal, float farVal);

    [DllImport("opengl32.dll")]
    public static extern void glRasterPos2i(int x, int y);
    [DllImport("opengl32.dll")]
    public static extern void glRasterPos2f(float x, float y);

    [DllImport("opengl32.dll")]
    public static extern void glPushMatrix();
    [DllImport("opengl32.dll")]
    public static extern void glPopMatrix();

    [DllImport("opengl32.dll")]
    public static extern void glWindowPos2iARB(int x, int y);

    [DllImport("opengl32.dll")]
    public extern static void glGenTextures(int n, [Out] out int textures);
    [DllImport("opengl32.dll")]
    public extern static void glBindTexture(uint target, int texture);
    [DllImport("opengl32.dll")]
    public extern static void glTexCoord2f(float x, float y);
    [DllImport("opengl32.dll")]
    public static extern void glTexImage2D(
        uint target, int level, uint internalFormat, int width, int height, int border,
        uint format, uint type, IntPtr scan0);
    [DllImport("opengl32.dll")]
    public static extern void glDeleteTextures(int size, int texture);

    [DllImport("opengl32.dll")]
    public static extern void glTexParameteri(uint target, uint name, uint param);

    [DllImport("opengl32.dll")]
    public static extern void glClearColor(float red, float green, float blue, float alpha);
    [DllImport("opengl32.dll")]
    public static extern void glEnable(uint cap);
    [DllImport("opengl32.dll")]
    public static extern void glDisable(uint cap);
    [DllImport("opengl32.dll")]
    public static extern void glBlendFunc(uint sfactor, uint dfactor);
    [DllImport("opengl32.dll")]
    public static extern void glDepthFunc(uint func);
    [DllImport("opengl32.dll")]
    public static extern void glClear(uint mask);
    [DllImport("opengl32.dll")]
    public static extern void glLoadIdentity();
    [DllImport("opengl32.dll")]
    public static extern void glTranslatef(float x, float y, float z);
    [DllImport("opengl32.dll")]
    public static extern void glRotatef(float angle, float x, float y, float z);
    [DllImport("opengl32.dll")]
    public static extern void glBegin(uint mode);
    [DllImport("opengl32.dll")]
    public static extern void glMatrixMode(uint mode);
    [DllImport("opengl32.dll")]
    public static extern void glColor3f(float red, float green, float blue);
    [DllImport("opengl32.dll")]
    public static extern void glColor3ub(int r, int g, int b);
    [DllImport("opengl32.dll")]
    public static extern void glColor4ub(int r, int g, int b, int a);

    [DllImport("opengl32.dll")]
    public static extern uint glGenLists(int size);

    [DllImport("opengl32.dll")]
    public static extern void glLineWidth(float w);

    [DllImport("opengl32.dll")]
    public static extern void glVertex2i(int x, int y);
    [DllImport("opengl32.dll")]
    public static extern void glVertex2f(float x, float y);
    [DllImport("opengl32.dll")]
    public static extern void glVertex3f(float x, float y, float z);
    [DllImport("opengl32.dll")]
    public static extern void glEnd();
    [DllImport("opengl32.dll")]
    public static extern void glColor4ub(byte red, byte green, byte blue, byte alpha);
    [DllImport("opengl32.dll")]
    public static extern void glListBase(uint base_notkeyword);
    [DllImport("opengl32.dll")]
    public static extern void glScalef(float x, float y, float z);
    [DllImport("opengl32.dll")]
    public static extern void glDeleteLists(uint list, int range);
    [DllImport("opengl32.dll")]
    public static extern void glCallLists(int n, uint type, string lists);
    [DllImport("opengl32.dll")]
    public static extern void glFlush();
    [DllImport("opengl32.dll")]
    public static extern void glViewport(int x, int y, int width, int height);
    [DllImport("opengl32.dll")]
    public static extern void glShadeModel(uint mode);
    [DllImport("opengl32.dll")]
    public static extern void glClearDepth(double depth);
    [DllImport("opengl32.dll")]
    public static extern void glHint(uint target, uint mode);


    #endregion

    #region	user32 imports
    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromDC(IntPtr hdc);
    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    #endregion

    #region glu32 imports
    [DllImport("glu32.dll")]
    public static extern void gluPerspective(double fovy, double aspect, double zNear, double zFar);
    [DllImport("glu32.dll")]
    public static extern void gluOrtho2D(double left, double right, double bottom, double top);
    #endregion
}
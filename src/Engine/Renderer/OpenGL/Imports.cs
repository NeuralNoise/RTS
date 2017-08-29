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
    public static extern void glClearColor(float red, float green, float blue, float alpha);
    [DllImport("opengl32.dll")]
    public static extern void glEnable(uint cap);
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

    #region structs
    [StructLayout(LayoutKind.Explicit)]
    public struct PIXELFORMATDESCRIPTOR {
        [FieldOffset(0)]
        public UInt16 nSize;
        [FieldOffset(2)]
        public UInt16 nVersion;
        [FieldOffset(4)]
        public UInt32 dwFlags;
        [FieldOffset(8)]
        public Byte iPixelType;
        [FieldOffset(9)]
        public Byte cColorBits;
        [FieldOffset(10)]
        public Byte cRedBits;
        [FieldOffset(11)]
        public Byte cRedShift;
        [FieldOffset(12)]
        public Byte cGreenBits;
        [FieldOffset(13)]
        public Byte cGreenShift;
        [FieldOffset(14)]
        public Byte cBlueBits;
        [FieldOffset(15)]
        public Byte cBlueShift;
        [FieldOffset(16)]
        public Byte cAlphaBits;
        [FieldOffset(17)]
        public Byte cAlphaShift;
        [FieldOffset(18)]
        public Byte cAccumBits;
        [FieldOffset(19)]
        public Byte cAccumRedBits;
        [FieldOffset(20)]
        public Byte cAccumGreenBits;
        [FieldOffset(21)]
        public Byte cAccumBlueBits;
        [FieldOffset(22)]
        public Byte cAccumAlphaBits;
        [FieldOffset(23)]
        public Byte cDepthBits;
        [FieldOffset(24)]
        public Byte cStencilBits;
        [FieldOffset(25)]
        public Byte cAuxBuffers;
        [FieldOffset(26)]
        public SByte iLayerType;
        [FieldOffset(27)]
        public Byte bReserved;
        [FieldOffset(28)]
        public UInt32 dwLayerMask;
        [FieldOffset(32)]
        public UInt32 dwVisibleMask;
        [FieldOffset(36)]
        public UInt32 dwDamageMask;
    }

    [StructLayout(LayoutKind.Explicit)]
    public class BITMAPINFO  {
        [FieldOffset(0)]
        public Int32 biSize;
        [FieldOffset(4)]
        public Int32 biWidth;
        [FieldOffset(8)]
        public Int32 biHeight;
        [FieldOffset(12)]
        public Int16 biPlanes;
        [FieldOffset(14)]
        public Int16 biBitCount;
        [FieldOffset(16)]
        public Int32 biCompression;
        [FieldOffset(20)]
        public Int32 biSizeImage;
        [FieldOffset(24)]
        public Int32 biXPelsPerMeter;
        [FieldOffset(28)]
        public Int32 biYPelsPerMeter;
        [FieldOffset(32)]
        public Int32 biClrUsed;
        [FieldOffset(36)]
        public Int32 biClrImportant;
        [FieldOffset(40)]
        public Int32 colors;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class LOGFONT {
        public int lfHeight = 0;
        public int lfWidth = 0;
        public int lfEscapement = 0;
        public int lfOrientation = 0;
        public int lfWeight = 0;
        public byte lfItalic = 0;
        public byte lfUnderline = 0;
        public byte lfStrikeOut = 0;
        public byte lfCharSet = 0;
        public byte lfOutPrecision = 0;
        public byte lfClipPrecision = 0;
        public byte lfQuality = 0;
        public byte lfPitchAndFamily = 0;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName = string.Empty;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ABC {
        public int  abcA;
        public uint abcB;
        public int  abcC;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct TEXTMETRIC {
        public int tmHeight;
        public int tmAscent;
        public int tmDescent;
        public int tmInternalLeading;
        public int tmExternalLeading;
        public int tmAveCharWidth;
        public int tmMaxCharWidth;
        public int tmWeight;
        public int tmOverhang;
        public int tmDigitizedAspectX;
        public int tmDigitizedAspectY;
        public char tmFirstChar;
        public char tmLastChar;
        public char tmDefaultChar;
        public char tmBreakChar;
        public byte tmItalic;
        public byte tmUnderlined;
        public byte tmStruckOut;
        public byte tmPitchAndFamily;
        public byte tmCharSet;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GLYPHMETRICSFLOAT {
        public float gmfBlackBoxX;
        public float gmfBlackBoxY;
        public POINTFLOAT gmfptGlyphOrigin;
        public float gmfCellIncX;
        public float gmfCellIncY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTFLOAT {
        public float x;
        public float y;
    }
    #endregion

    #region constants

    public const uint WGL_FONT_POLYGONS = 0x01;


    public const uint MAJOR_VERSION = 0x821B;
    public const uint MINOR_VERSION = 0x821C;

    public enum FontWeight : int {
        FW_DONTCARE = 0,
        FW_THIN = 100,
        FW_EXTRALIGHT = 200,
        FW_LIGHT = 300,
        FW_NORMAL = 400,
        FW_MEDIUM = 500,
        FW_SEMIBOLD = 600,
        FW_BOLD = 700,
        FW_EXTRABOLD = 800,
        FW_HEAVY = 900,
    }
    public enum FontCharSet : byte {
        ANSI_CHARSET = 0,
        DEFAULT_CHARSET = 1,
        SYMBOL_CHARSET = 2,
        SHIFTJIS_CHARSET = 128,
        HANGEUL_CHARSET = 129,
        HANGUL_CHARSET = 129,
        GB2312_CHARSET = 134,
        CHINESEBIG5_CHARSET = 136,
        OEM_CHARSET = 255,
        JOHAB_CHARSET = 130,
        HEBREW_CHARSET = 177,
        ARABIC_CHARSET = 178,
        GREEK_CHARSET = 161,
        TURKISH_CHARSET = 162,
        VIETNAMESE_CHARSET = 163,
        THAI_CHARSET = 222,
        EASTEUROPE_CHARSET = 238,
        RUSSIAN_CHARSET = 204,
        MAC_CHARSET = 77,
        BALTIC_CHARSET = 186,
    }
    public enum FontPrecision : byte {
        OUT_DEFAULT_PRECIS = 0,
        OUT_STRING_PRECIS = 1,
        OUT_CHARACTER_PRECIS = 2,
        OUT_STROKE_PRECIS = 3,
        OUT_TT_PRECIS = 4,
        OUT_DEVICE_PRECIS = 5,
        OUT_RASTER_PRECIS = 6,
        OUT_TT_ONLY_PRECIS = 7,
        OUT_OUTLINE_PRECIS = 8,
        OUT_SCREEN_OUTLINE_PRECIS = 9,
        OUT_PS_ONLY_PRECIS = 10,
    }
    public enum FontClipPrecision : byte {
        CLIP_DEFAULT_PRECIS = 0,
        CLIP_CHARACTER_PRECIS = 1,
        CLIP_STROKE_PRECIS = 2,
        CLIP_MASK = 0xf,
        CLIP_LH_ANGLES = (1 << 4),
        CLIP_TT_ALWAYS = (2 << 4),
        CLIP_DFA_DISABLE = (4 << 4),
        CLIP_EMBEDDED = (8 << 4),
    }
    public enum FontQuality : byte {
        DEFAULT_QUALITY = 0,
        DRAFT_QUALITY = 1,
        PROOF_QUALITY = 2,
        NONANTIALIASED_QUALITY = 3,
        ANTIALIASED_QUALITY = 4,
        CLEARTYPE_QUALITY = 5,
        CLEARTYPE_NATURAL_QUALITY = 6,
    }
    [Flags]
    public enum FontPitchAndFamily : byte {
        DEFAULT_PITCH = 0,
        FIXED_PITCH = 1,
        VARIABLE_PITCH = 2,
        FF_DONTCARE = (0 << 4),
        FF_ROMAN = (1 << 4),
        FF_SWISS = (2 << 4),
        FF_MODERN = (3 << 4),
        FF_SCRIPT = (4 << 4),
        FF_DECORATIVE = (5 << 4),
    }

    

    #region PixelFormatDescriptor Flags

    public const byte PFD_TYPE_RGBA = 0;
    public const byte PFD_TYPE_COLORINDEX = 1;

    public const uint PFD_DOUBLEBUFFER = 1;
    public const uint PFD_STEREO = 2;
    public const uint PFD_DRAW_TO_WINDOW = 4;
    public const uint PFD_DRAW_TO_BITMAP = 8;
    public const uint PFD_SUPPORT_GDI = 16;
    public const uint PFD_SUPPORT_OPENGL = 32;
    public const uint PFD_GENERIC_FORMAT = 64;
    public const uint PFD_NEED_PALETTE = 128;
    public const uint PFD_NEED_SYSTEM_PALETTE = 256;
    public const uint PFD_SWAP_EXCHANGE = 512;
    public const uint PFD_SWAP_COPY = 1024;
    public const uint PFD_SWAP_LAYER_BUFFERS = 2048;
    public const uint PFD_GENERIC_ACCELERATED = 4096;
    public const uint PFD_SUPPORT_DIRECTDRAW = 8192;

    public const sbyte PFD_MAIN_PLANE = 0;
    public const sbyte PFD_OVERLAY_PLANE = 1;
    public const sbyte PFD_UNDERLAY_PLANE = -1;


    #endregion

    #region BitmapInfo Flags
    public static uint BI_RGB = 0;
    public static uint BI_RLE8 = 1;
    public static uint BI_RLE4 = 2;
    public static uint BI_BITFIELDS = 3;
    public static uint BI_JPEG = 4;
    public static uint BI_PNG = 5;

    public static uint DIB_RGB_COLORS = 0;
    public static uint DIB_PAL_COLORS = 1;
    #endregion

    #region GetTarget
    public const uint CURRENT_COLOR = 0x0B00;
    public const uint CURRENT_INDEX = 0x0B01;
    public const uint CURRENT_NORMAL = 0x0B02;
    public const uint CURRENT_TEXTURE_COORDS = 0x0B03;
    public const uint CURRENT_RASTER_COLOR = 0x0B04;
    public const uint CURRENT_RASTER_INDEX = 0x0B05;
    public const uint CURRENT_RASTER_TEXTURE_COORDS = 0x0B06;
    public const uint CURRENT_RASTER_POSITION = 0x0B07;
    public const uint CURRENT_RASTER_POSITION_VALID = 0x0B08;
    public const uint CURRENT_RASTER_DISTANCE = 0x0B09;
    public const uint POINT_SMOOTH = 0x0B10;
    public const uint POINT_SIZE = 0x0B11;
    public const uint POINT_SIZE_RANGE = 0x0B12;
    public const uint POINT_SIZE_GRANULARITY = 0x0B13;
    public const uint LINE_SMOOTH = 0x0B20;
    public const uint LINE_WIDTH = 0x0B21;
    public const uint LINE_WIDTH_RANGE = 0x0B22;
    public const uint LINE_WIDTH_GRANULARITY = 0x0B23;
    public const uint LINE_STIPPLE = 0x0B24;
    public const uint LINE_STIPPLE_PATTERN = 0x0B25;
    public const uint LINE_STIPPLE_REPEAT = 0x0B26;
    public const uint LIST_MODE = 0x0B30;
    public const uint MAX_LIST_NESTING = 0x0B31;
    public const uint LIST_BASE = 0x0B32;
    public const uint LIST_INDEX = 0x0B33;
    public const uint POLYGON_MODE = 0x0B40;
    public const uint POLYGON_SMOOTH = 0x0B41;
    public const uint POLYGON_STIPPLE = 0x0B42;
    public const uint EDGE_FLAG = 0x0B43;
    public const uint CULL_FACE = 0x0B44;
    public const uint CULL_FACE_MODE = 0x0B45;
    public const uint FRONT_FACE = 0x0B46;
    public const uint LIGHTING = 0x0B50;
    public const uint LIGHT_MODEL_LOCAL_VIEWER = 0x0B51;
    public const uint LIGHT_MODEL_TWO_SIDE = 0x0B52;
    public const uint LIGHT_MODEL_AMBIENT = 0x0B53;
    public const uint SHADE_MODEL = 0x0B54;
    public const uint COLOR_MATERIAL_FACE = 0x0B55;
    public const uint COLOR_MATERIAL_PARAMETER = 0x0B56;
    public const uint COLOR_MATERIAL = 0x0B57;
    public const uint FOG = 0x0B60;
    public const uint FOG_INDEX = 0x0B61;
    public const uint FOG_DENSITY = 0x0B62;
    public const uint FOG_START = 0x0B63;
    public const uint FOG_END = 0x0B64;
    public const uint FOG_MODE = 0x0B65;
    public const uint FOG_COLOR = 0x0B66;
    public const uint DEPTH_RANGE = 0x0B70;
    public const uint DEPTH_TEST = 0x0B71;
    public const uint DEPTH_WRITEMASK = 0x0B72;
    public const uint DEPTH_CLEAR_VALUE = 0x0B73;
    public const uint DEPTH_FUNC = 0x0B74;
    public const uint ACCUM_CLEAR_VALUE = 0x0B80;
    public const uint STENCIL_TEST = 0x0B90;
    public const uint STENCIL_CLEAR_VALUE = 0x0B91;
    public const uint STENCIL_FUNC = 0x0B92;
    public const uint STENCIL_VALUE_MASK = 0x0B93;
    public const uint STENCIL_FAIL = 0x0B94;
    public const uint STENCIL_PASS_DEPTH_FAIL = 0x0B95;
    public const uint STENCIL_PASS_DEPTH_PASS = 0x0B96;
    public const uint STENCIL_REF = 0x0B97;
    public const uint STENCIL_WRITEMASK = 0x0B98;
    public const uint MATRIX_MODE = 0x0BA0;
    public const uint NORMALIZE = 0x0BA1;
    public const uint VIEWPORT = 0x0BA2;
    public const uint MODELVIEW_STACK_DEPTH = 0x0BA3;
    public const uint PROJECTION_STACK_DEPTH = 0x0BA4;
    public const uint TEXTURE_STACK_DEPTH = 0x0BA5;
    public const uint MODELVIEW_MATRIX = 0x0BA6;
    public const uint PROJECTION_MATRIX = 0x0BA7;
    public const uint TEXTURE_MATRIX = 0x0BA8;
    public const uint ATTRIB_STACK_DEPTH = 0x0BB0;
    public const uint CLIENT_ATTRIB_STACK_DEPTH = 0x0BB1;
    public const uint ALPHA_TEST = 0x0BC0;
    public const uint ALPHA_TEST_FUNC = 0x0BC1;
    public const uint ALPHA_TEST_REF = 0x0BC2;
    public const uint DITHER = 0x0BD0;
    public const uint BLEND_DST = 0x0BE0;
    public const uint BLEND_SRC = 0x0BE1;
    public const uint BLEND = 0x0BE2;
    public const uint LOGIC_OP_MODE = 0x0BF0;
    public const uint INDEX_LOGIC_OP = 0x0BF1;
    public const uint COLOR_LOGIC_OP = 0x0BF2;
    public const uint AUX_BUFFERS = 0x0C00;
    public const uint DRAW_BUFFER = 0x0C01;
    public const uint READ_BUFFER = 0x0C02;
    public const uint SCISSOR_BOX = 0x0C10;
    public const uint SCISSOR_TEST = 0x0C11;
    public const uint INDEX_CLEAR_VALUE = 0x0C20;
    public const uint INDEX_WRITEMASK = 0x0C21;
    public const uint COLOR_CLEAR_VALUE = 0x0C22;
    public const uint COLOR_WRITEMASK = 0x0C23;
    public const uint INDEX_MODE = 0x0C30;
    public const uint RGBA_MODE = 0x0C31;
    public const uint DOUBLEBUFFER = 0x0C32;
    public const uint STEREO = 0x0C33;
    public const uint RENDER_MODE = 0x0C40;
    public const uint PERSPECTIVE_CORRECTION_HINT = 0x0C50;
    public const uint POINT_SMOOTH_HINT = 0x0C51;
    public const uint LINE_SMOOTH_HINT = 0x0C52;
    public const uint POLYGON_SMOOTH_HINT = 0x0C53;
    public const uint FOG_HINT = 0x0C54;
    public const uint TEXTURE_GEN_S = 0x0C60;
    public const uint TEXTURE_GEN_T = 0x0C61;
    public const uint TEXTURE_GEN_R = 0x0C62;
    public const uint TEXTURE_GEN_Q = 0x0C63;
    public const uint PIXEL_MAP_I_TO_I = 0x0C70;
    public const uint PIXEL_MAP_S_TO_S = 0x0C71;
    public const uint PIXEL_MAP_I_TO_R = 0x0C72;
    public const uint PIXEL_MAP_I_TO_G = 0x0C73;
    public const uint PIXEL_MAP_I_TO_B = 0x0C74;
    public const uint PIXEL_MAP_I_TO_A = 0x0C75;
    public const uint PIXEL_MAP_R_TO_R = 0x0C76;
    public const uint PIXEL_MAP_G_TO_G = 0x0C77;
    public const uint PIXEL_MAP_B_TO_B = 0x0C78;
    public const uint PIXEL_MAP_A_TO_A = 0x0C79;
    public const uint PIXEL_MAP_I_TO_I_SIZE = 0x0CB0;
    public const uint PIXEL_MAP_S_TO_S_SIZE = 0x0CB1;
    public const uint PIXEL_MAP_I_TO_R_SIZE = 0x0CB2;
    public const uint PIXEL_MAP_I_TO_G_SIZE = 0x0CB3;
    public const uint PIXEL_MAP_I_TO_B_SIZE = 0x0CB4;
    public const uint PIXEL_MAP_I_TO_A_SIZE = 0x0CB5;
    public const uint PIXEL_MAP_R_TO_R_SIZE = 0x0CB6;
    public const uint PIXEL_MAP_G_TO_G_SIZE = 0x0CB7;
    public const uint PIXEL_MAP_B_TO_B_SIZE = 0x0CB8;
    public const uint PIXEL_MAP_A_TO_A_SIZE = 0x0CB9;
    public const uint UNPACK_SWAP_BYTES = 0x0CF0;
    public const uint UNPACK_LSB_FIRST = 0x0CF1;
    public const uint UNPACK_ROW_LENGTH = 0x0CF2;
    public const uint UNPACK_SKIP_ROWS = 0x0CF3;
    public const uint UNPACK_SKIP_PIXELS = 0x0CF4;
    public const uint UNPACK_ALIGNMENT = 0x0CF5;
    public const uint PACK_SWAP_BYTES = 0x0D00;
    public const uint PACK_LSB_FIRST = 0x0D01;
    public const uint PACK_ROW_LENGTH = 0x0D02;
    public const uint PACK_SKIP_ROWS = 0x0D03;
    public const uint PACK_SKIP_PIXELS = 0x0D04;
    public const uint PACK_ALIGNMENT = 0x0D05;
    public const uint MAP_COLOR = 0x0D10;
    public const uint MAP_STENCIL = 0x0D11;
    public const uint INDEX_SHIFT = 0x0D12;
    public const uint INDEX_OFFSET = 0x0D13;
    public const uint RED_SCALE = 0x0D14;
    public const uint RED_BIAS = 0x0D15;
    public const uint ZOOM_X = 0x0D16;
    public const uint ZOOM_Y = 0x0D17;
    public const uint GREEN_SCALE = 0x0D18;
    public const uint GREEN_BIAS = 0x0D19;
    public const uint BLUE_SCALE = 0x0D1A;
    public const uint BLUE_BIAS = 0x0D1B;
    public const uint ALPHA_SCALE = 0x0D1C;
    public const uint ALPHA_BIAS = 0x0D1D;
    public const uint DEPTH_SCALE = 0x0D1E;
    public const uint DEPTH_BIAS = 0x0D1F;
    public const uint MAX_EVAL_ORDER = 0x0D30;
    public const uint MAX_LIGHTS = 0x0D31;
    public const uint MAX_CLIP_PLANES = 0x0D32;
    public const uint MAX_TEXTURE_SIZE = 0x0D33;
    public const uint MAX_PIXEL_MAP_TABLE = 0x0D34;
    public const uint MAX_ATTRIB_STACK_DEPTH = 0x0D35;
    public const uint MAX_MODELVIEW_STACK_DEPTH = 0x0D36;
    public const uint MAX_NAME_STACK_DEPTH = 0x0D37;
    public const uint MAX_PROJECTION_STACK_DEPTH = 0x0D38;
    public const uint MAX_TEXTURE_STACK_DEPTH = 0x0D39;
    public const uint MAX_VIEWPORT_DIMS = 0x0D3A;
    public const uint MAX_CLIENT_ATTRIB_STACK_DEPTH = 0x0D3B;
    public const uint SUBPIXEL_BITS = 0x0D50;
    public const uint INDEX_BITS = 0x0D51;
    public const uint RED_BITS = 0x0D52;
    public const uint GREEN_BITS = 0x0D53;
    public const uint BLUE_BITS = 0x0D54;
    public const uint ALPHA_BITS = 0x0D55;
    public const uint DEPTH_BITS = 0x0D56;
    public const uint STENCIL_BITS = 0x0D57;
    public const uint ACCUM_RED_BITS = 0x0D58;
    public const uint ACCUM_GREEN_BITS = 0x0D59;
    public const uint ACCUM_BLUE_BITS = 0x0D5A;
    public const uint ACCUM_ALPHA_BITS = 0x0D5B;
    public const uint NAME_STACK_DEPTH = 0x0D70;
    public const uint AUTO_NORMAL = 0x0D80;
    public const uint MAP1_COLOR_4 = 0x0D90;
    public const uint MAP1_INDEX = 0x0D91;
    public const uint MAP1_NORMAL = 0x0D92;
    public const uint MAP1_TEXTURE_COORD_1 = 0x0D93;
    public const uint MAP1_TEXTURE_COORD_2 = 0x0D94;
    public const uint MAP1_TEXTURE_COORD_3 = 0x0D95;
    public const uint MAP1_TEXTURE_COORD_4 = 0x0D96;
    public const uint MAP1_VERTEX_3 = 0x0D97;
    public const uint MAP1_VERTEX_4 = 0x0D98;
    public const uint MAP2_COLOR_4 = 0x0DB0;
    public const uint MAP2_INDEX = 0x0DB1;
    public const uint MAP2_NORMAL = 0x0DB2;
    public const uint MAP2_TEXTURE_COORD_1 = 0x0DB3;
    public const uint MAP2_TEXTURE_COORD_2 = 0x0DB4;
    public const uint MAP2_TEXTURE_COORD_3 = 0x0DB5;
    public const uint MAP2_TEXTURE_COORD_4 = 0x0DB6;
    public const uint MAP2_VERTEX_3 = 0x0DB7;
    public const uint MAP2_VERTEX_4 = 0x0DB8;
    public const uint MAP1_GRID_DOMAIN = 0x0DD0;
    public const uint MAP1_GRID_SEGMENTS = 0x0DD1;
    public const uint MAP2_GRID_DOMAIN = 0x0DD2;
    public const uint MAP2_GRID_SEGMENTS = 0x0DD3;
    public const uint TEXTURE_1D = 0x0DE0;
    public const uint TEXTURE_2D = 0x0DE1;
    public const uint FEEDBACK_BUFFER_POINTER = 0x0DF0;
    public const uint FEEDBACK_BUFFER_SIZE = 0x0DF1;
    public const uint FEEDBACK_BUFFER_TYPE = 0x0DF2;
    public const uint SELECTION_BUFFER_POINTER = 0x0DF3;
    public const uint SELECTION_BUFFER_SIZE = 0x0DF4;
    #endregion

    #region AlphaFunction
    public const uint NEVER = 0x0200;
    public const uint LESS = 0x0201;
    public const uint EQUAL = 0x0202;
    public const uint LEQUAL = 0x0203;
    public const uint GREATER = 0x0204;
    public const uint NOTEQUAL = 0x0205;
    public const uint GEQUAL = 0x0206;
    public const uint ALWAYS = 0x0207;
    #endregion

    #region AttribMask
    public const uint CURRENT_BIT = 0x00000001;
    public const uint POINT_BIT = 0x00000002;
    public const uint LINE_BIT = 0x00000004;
    public const uint POLYGON_BIT = 0x00000008;
    public const uint POLYGON_STIPPLE_BIT = 0x00000010;
    public const uint PIXEL_MODE_BIT = 0x00000020;
    public const uint LIGHTING_BIT = 0x00000040;
    public const uint FOG_BIT = 0x00000080;
    public const uint DEPTH_BUFFER_BIT = 0x00000100;
    public const uint ACCUM_BUFFER_BIT = 0x00000200;
    public const uint STENCIL_BUFFER_BIT = 0x00000400;
    public const uint VIEWPORT_BIT = 0x00000800;
    public const uint TRANSFORM_BIT = 0x00001000;
    public const uint ENABLE_BIT = 0x00002000;
    public const uint COLOR_BUFFER_BIT = 0x00004000;
    public const uint HINT_BIT = 0x00008000;
    public const uint EVAL_BIT = 0x00010000;
    public const uint LIST_BIT = 0x00020000;
    public const uint TEXTURE_BIT = 0x00040000;
    public const uint SCISSOR_BIT = 0x00080000;
    public const uint ALL_ATTRIB_BITS = 0x000fffff;
    #endregion

    #region BeginMode
    public const uint POINTS = 0x0000;
    public const uint LINES = 0x0001;
    public const uint LINE_LOOP = 0x0002;
    public const uint LINE_STRIP = 0x0003;
    public const uint TRIANGLES = 0x0004;
    public const uint TRIANGLE_STRIP = 0x0005;
    public const uint TRIANGLE_FAN = 0x0006;
    public const uint QUADS = 0x0007;
    public const uint QUAD_STRIP = 0x0008;
    public const uint POLYGON = 0x0009;
    #endregion

    #region MatrixMode
    public const uint MODELVIEW = 0x1700;
    public const uint PROJECTION = 0x1701;
    public const uint TEXTURE = 0x1702;
    #endregion

    #region ShadingModel
    public const uint FLAT = 0x1D00;
    public const uint SMOOTH = 0x1D01;
    #endregion

    #region HintMode
    public const uint DONT_CARE = 0x1100;
    public const uint FASTEST = 0x1101;
    public const uint NICEST = 0x1102;
    #endregion

    #region DataType
    public const uint BYTE = 0x1400;
    public const uint UNSIGNED_BYTE = 0x1401;
    public const uint SHORT = 0x1402;
    public const uint UNSIGNED_SHORT = 0x1403;
    public const uint INT = 0x1404;
    public const uint UNSIGNED_INT = 0x1405;
    public const uint FLOAT = 0x1406;
    public const uint GL_2_BYTES = 0x1407;
    public const uint GL_3_BYTES = 0x1408;
    public const uint GL_4_BYTES = 0x1409;
    public const uint DOUBLE = 0x140A;
    #endregion

    #region Blend
    public const uint SRC_ALPHA = 0x302;
    public const uint ONE_MINUS_SRC_ALPHA = 0x303;
    #endregion
    #endregion
}
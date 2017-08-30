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

public static partial class OpenGL
{
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
    public class BITMAPINFO {
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
        public int abcA;
        public uint abcB;
        public int abcC;
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
}
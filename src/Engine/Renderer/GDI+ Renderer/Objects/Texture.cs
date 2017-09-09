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

/*just a higher deriving font object.*/
public class GDIPTexture : ITexture {
    private Bitmap p_Bitmap;
    public GDIPTexture(Bitmap bmp) {
        p_Bitmap = bmp;
    }
    public Bitmap Bitmap { get { return p_Bitmap; } }

    public void Dispose() {
        try {
            p_Bitmap.Dispose();
        }
        catch { }
        p_Bitmap = null;
    }
}
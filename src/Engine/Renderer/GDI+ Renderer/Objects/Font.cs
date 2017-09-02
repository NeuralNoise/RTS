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
public class GDIPFont : IFont {

    private Font p_Font;
    public GDIPFont(Font font) {
        p_Font = font;
    }

    public Font Font { get { return p_Font; } }

    public void Dispose() {
        p_Font.Dispose();
        p_Font = null;
    }
}
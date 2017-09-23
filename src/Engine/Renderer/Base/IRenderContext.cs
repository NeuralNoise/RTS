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

public interface IRenderContext : IDeviceContext, IDisposable {
    int Width { get; }
    int Height { get; }

    /// <summary>
    /// Returns true if the context has to be recreated.
    /// </summary>
    bool Resize(int w, int h);

    ITexture AllocateTexture(Bitmap bmp, string alias);
    IFont AllocateFont(Font font);

    ITexture GetTexture(string alias);

    float DPIX { get; }
    float DPIY { get; }

    bool IsDisposed { get; }
}
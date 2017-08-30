/*
 *  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 *  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 *  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 *  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
 *  REMAINS UNCHANGED.
 *
 *  REPO: http://www.github.com/tomwilsoncoder/RTS
*/


public static partial class OpenGL {
    public class OpenGLTexture : ITexture {
        public int INDEX;
        public int HASH;

        public int Width;
        public int Height;

        public void Dispose() {
            glDeleteTextures(1, INDEX);
            INDEX = 0;
            HASH = 0;
            Width = 0;
            Height = 0;
        }
    }
}
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
using System.Windows.Forms;

public interface IUI {
    void Update();

    void Draw(IRenderContext context, IRenderer renderer);

    void OnKeyDown(Game game, KeyEventArgs e);
    void OnKeyUp(Game game, KeyEventArgs e);

    void OnMouseClick(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseMove(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseDown(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseUp(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseScroll(Game game, Point mousePosition, MouseEventArgs e);

    void Enable();
    void Disable();

    bool Visible { get; set; }
    bool Enabled { get; }
}
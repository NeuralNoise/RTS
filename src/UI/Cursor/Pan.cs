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

public class UIPan : IUI {
    private bool p_Enabled;
    private UICursor p_Cursor;
    private Game p_Game;

    private Point p_CameraStart;
    private Point p_MouseStart;

    public UIPan(Game game, UICursor cursor) {
        p_Cursor = cursor;
        p_Game = game;
    }

    public void OnMouseDown(Game game, Point mousePosition, MouseEventArgs e) {
        //mouse middle down?
        MouseButtons button = e.Button;
        if (button != MouseButtons.Middle) {
            if (p_Enabled) { return; }
            Disable();
            return;
        }

        if (p_Enabled) { return; }

        //setup
        Camera camera = p_Game.Camera;
        Enable();
        p_MouseStart = mousePosition;
        p_CameraStart = new Point(
            camera.X,
            camera.Y);
    }
    public void OnMouseUp(Game game, Point mousePosition, MouseEventArgs e) {
        Disable();
    }

    public void OnMouseMove(Game game, Point mousePosition, MouseEventArgs e) {
        if (!p_Enabled) { return; }

        //get the difference between the current mouse
        //position and the start positions so we
        //can increment the camera position based off it.
        int dX = p_MouseStart.X - mousePosition.X;
        int dY = p_MouseStart.Y - mousePosition.Y;

        //we will later have a pan speed so we 
        //use that as a delta multiplier.
        dX = (int)(dX * 1.5f);
        dY = (int)(dY * 1.5f);

        //move camera
        Camera camera = p_Game.Camera;
        camera.MoveAbs(
            p_CameraStart.X + dX,
            p_CameraStart.Y + dY);

    }

    public bool Enabled { get { return p_Enabled; } }
    public bool Visible { 
        get { return true; }
        set {
            throw new NotSupportedException();
        }
    }

    public void Enable() {
        p_Enabled = true;

        p_Cursor.SetArrow(Direction.ALL);

    }
    public void Disable() {
        if (!p_Enabled) { return; }

        p_Cursor.SetArrow(Direction.NONE);
        p_Enabled = false;
    }

    /*Useless events*/
    public void OnMouseScroll(Game game, Point mousePosition, MouseEventArgs e) { }
    public void OnMouseClick(Game game, Point mousePosition, MouseEventArgs e) { }
    public void OnKeyDown(Game game, KeyEventArgs e) { }
    public void OnKeyUp(Game game, KeyEventArgs e) { }
    public void Update() { }
    public void Draw(IRenderContext context, IRenderer renderer) { }

}
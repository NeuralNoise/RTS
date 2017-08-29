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

public sealed class GameWindow : Form {
    private IRenderContext p_Context;
    private IRenderer p_Renderer;
    private bool p_Focused;
    private bool p_Closed;

    private int p_MouseOffsetX;
    private int p_MouseOffsetY;

    public GameWindow() {
        Text = "Game";

        ShowIcon = false;
        //ShowInTaskbar = false;

        MaximizeBox = false;
        
        StartPosition = FormStartPosition.CenterScreen;
        //FormBorderStyle = FormBorderStyle.FixedSingle;

        Size = new Size(740, 480);
        ClientSize = new Size(800, 600);

        /*create a wrapper where the rendering will 
          actually take place. We do this since any device
          context we create from the window will have the 
          raster 0,0 set to above the actual client region.*/

        /*setup renderer*/
        p_Renderer = RenderFactory.CreateRenderer();
        RecreateContext();
    }

    private void handleClosing(object sender, FormClosingEventArgs e) {
        if (e.CloseReason == CloseReason.TaskManagerClosing ||
             e.CloseReason == CloseReason.WindowsShutDown) {
                 p_Closed = true;
                 return;
        }

        //if it's full screen, take topmost off so the message box would actually show.
        if (IsFullScreen) { TopMost = false; }

        DialogResult res =
            MessageBox.Show("Do you really want to quit the game? Any unsaved progress will be lost.",
                            "Are you sure?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation);

        //switch back to topmost if we are in fullscreen
        if (IsFullScreen) { TopMost = true; }

        e.Cancel = (res == DialogResult.No);
        if (!e.Cancel) {
            p_Closed = true;
        }
    }
    private void handleKeyDown(object sender, KeyEventArgs e) { 
        /*ALT+Enter?*/
        if (e.Control && e.KeyCode == Keys.Return) {
            ToggleFullscreen();
        }
    }
    private void handleFocusChanged(object sender, EventArgs e) {
        p_Focused = base.Focused;    
    }
    private void handleMove(object sender, EventArgs e) {

        //get the screen position of the top left corner of the window
        Point location = PointToScreen(Point.Empty);
        p_MouseOffsetX = location.X;
        p_MouseOffsetY = location.Y;
    }

    public override bool Focused {
        get { return p_Focused; }
    }

    public void RecreateContext() {
        if (p_Context != null) {
            p_Context.Dispose();
        }

        p_Renderer = RenderFactory.CreateRenderer();
        p_Context = RenderFactory.CreateContext(Handle);
    }

    public void UnhookCoreEvents() {
        FormClosing -= handleClosing;
        KeyDown -= handleKeyDown;
        GotFocus -= handleFocusChanged;
        LostFocus -= handleFocusChanged;
        Move -= handleMove;
        Shown -= handleMove;
    }
    public void HookCoreEvents() {
        KeyDown += handleKeyDown;
        FormClosing += handleClosing;
        GotFocus += handleFocusChanged;
        LostFocus += handleFocusChanged;


        Move += handleMove;
        Shown += handleMove;
    }

    public bool Closed { get { return p_Closed; } }
    public bool IsFullScreen { get { return WindowState == FormWindowState.Maximized; } }
    public void ToggleFullscreen() { 
        //fullscreen?
        if (IsFullScreen) {
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            TopMost = false;
        }
        else {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
        }
    }

    public int MouseOffsetX { get { return p_MouseOffsetX; } }
    public int MouseOffsetY { get { return p_MouseOffsetY; } }
    public Point MouseOffset {
        get {
            return new Point(
                p_MouseOffsetX,
                p_MouseOffsetY);
        }
    }

    public IRenderContext Context { get { return p_Context; } }
    public IRenderer Renderer { get { return p_Renderer; } }
}
using System;
using System.Drawing;
using System.Windows.Forms;

public sealed class GameWindow : Form {
    private IRenderContext p_Context;
    private IRenderer p_Renderer;
    private bool p_Focused;
    private bool p_Closed;

    public GameWindow() {
        Text = "Game";

        ShowIcon = false;
        //ShowInTaskbar = false;

        MaximizeBox = false;
        
        StartPosition = FormStartPosition.CenterScreen;
        //FormBorderStyle = FormBorderStyle.FixedSingle;

        Size = new Size(740, 480);
        ClientSize = new Size(800, 600);

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
        DialogResult res =
            MessageBox.Show("Do you really want to quit the game? Any unsaved progress will be lost.",
                            "Are you sure?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation);
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

    public override bool Focused {
        get { return p_Focused; }
    }

    public void RecreateContext() {
        if (p_Context != null) {
            p_Context.Dispose();
        }
        p_Context = RenderFactory.CreateContext(Handle);
    }

    public void UnhookCoreEvents() {
        FormClosing -= handleClosing;
        KeyDown -= handleKeyDown;
        GotFocus -= handleFocusChanged;
        LostFocus -= handleFocusChanged;
    }
    public void HookCoreEvents() {
        KeyDown += handleKeyDown;
        FormClosing += handleClosing;
        GotFocus += handleFocusChanged;
        LostFocus += handleFocusChanged;
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

    public IRenderContext Context { get { return p_Context; } }
    public IRenderer Renderer { get { return p_Renderer; } }
}
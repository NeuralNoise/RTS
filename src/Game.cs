using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Game {
    private GameWindow p_Window;
    private Camera p_Camera;
    private Map p_Map;

    public Game(GameWindow wnd) {
        p_Window = wnd;
       
        /*hook crash events at the first opportunity.*/
        Application.ThreadException += handleCrash;

        /*fullscreen*/
        p_Window.Shown += delegate(object s, EventArgs e) {
            //p_Window.ToggleFullscreen();
            p_Window.Focus();
            p_Window.BringToFront();
        };

        /*initialize camera*/
        p_Camera = new Camera(this);
        p_Camera.ZoomAbs(0, 0);
        p_Camera.MoveCenter(500, 500);

        /*initialize map*/
        p_Map = new Map(this, 1000, 1000);

        /*init sub-systems*/
        initPlayers();
        initLogic();
        initUI();
        initDebug();
        initDraw();

        /*hook events*/
        wnd.MouseLeave += handleMouseLeave;
        wnd.Click += handleMouseClick;
        wnd.MouseWheel += handleMouseScroll;
        wnd.MouseMove += handleMouseMove;
        wnd.MouseUp += handleMouseUp;
        wnd.KeyDown += handleKeyDown;
        wnd.KeyUp += handleKeyUp;
        wnd.Resize += handleResize;
        wnd.GotFocus += handleFocusChanged;
        wnd.LostFocus += handleFocusChanged;
        
        wnd.HookCoreEvents();

        testCode();
    }

    public Camera Camera { get { return p_Camera; } }
    public Map Map { get { return p_Map; } }
    public GameWindow Window { get { return p_Window; } }

    private IUI p_EventHijacker;
    public void HijackEvents(IUI ui) {
        if (p_EventHijacker != null) {
            throw new Exception("A UI element already has control over events.");
        }

        p_EventHijacker = ui;
    }
    public void DetatchFromEventsHijack(IUI ui) {
        if (ui != p_EventHijacker) {
            throw new Exception("UI element did not hijack events");
        }
        p_EventHijacker = null;
    }

    private void testCode() {
        Fog f = CurrentPlayer.Fog;

        sight = new LineOfSight(500, 500, 5);
        p_CurrentPlayer.Fog.AddLOS(sight);
        p_CurrentPlayer.Fog.UpdateLOS();
    }

    #region Rendering
    private Heartbeat p_RenderHeartbeat;
    private void initDraw() {
        p_RenderHeartbeat = new Heartbeat("renderer");
        p_RenderHeartbeat.Speed(-1);
        p_RenderHeartbeat.Start(
            this,
            draw);

    }

    private void draw(object state) {
        //is the window minimized?
        if (p_Window.WindowState == FormWindowState.Minimized) {
            return;
        }

        Game game = state as Game;
        IRenderContext context = game.p_Window.Context;
        IRenderer renderer = game.p_Window.Renderer;
        renderer.BeginFrame(game.p_Window.Context);
        renderer.SetColor(Color.Black);
        renderer.Clear();

        p_Map.Draw(context, renderer);

        drawUI(context, renderer);

        renderer.EndFrame();
    }
    
    private string getPointString(Point pt) {
        return pt.X + "x" + pt.Y;
    }
    private string getSizeString(Size sz) {
        return sz.Width + "x" + sz.Height;
    }
    private string getHeartbeatString(Heartbeat h) {
        return
            "[r]" + h.Rate + "bps " +
            "[c]" + h.TotalFrames + " " +
            "[avg]" + h.AverageLatency.ToString("0.00") + "ms " +
            "[lat]" + h.LastLatency.ToString("0.00") + "ms";

    }

    
    #endregion

    #region Debug
    private UILabel p_DebugLabel;
    private bool p_DebugPrompt = false;
    private bool p_DebugFull = true;

    private void initDebug() {
        p_DebugLabel = (UILabel)addUI(new UILabel(this));
        p_DebugLabel.TextAlignment = TextAlign.Right;
        p_DebugLabel.ForeBrush = Brushes.White;
        p_DebugLabel.Font = new Font("Arial", 12, FontStyle.Bold);
        p_DebugLabel.Disable();
    }

    private void debugPrompt() {
        if (p_DebugPrompt) { return; }

        p_DebugPrompt = true;
        TextBox txt = new TextBox();
        txt.Font = new Font("Arial", 20);
        txt.Width = 400;
        txt.Location = new Point(
            (p_Window.ClientSize.Width / 2) - (txt.Width / 2),
            (p_Window.ClientSize.Height / 2) - (txt.Height / 2));
        p_Window.Controls.Add(txt);

        txt.Focus();

        txt.KeyDown += delegate(object sender, KeyEventArgs e) {
            if (e.KeyCode != Keys.Enter) { return; }
            TextBox t = sender as TextBox;
            t.Parent.Controls.Remove(t);
            p_DebugPrompt = false;
            try { onDebugPrompt(t.Text); }
            catch (Exception ex) {
                if (ex.Message == "Debug crash") {
                    throw ex;
                }
            }

        };
    
    }
    private void onDebugPrompt(string str) {
        str = str.ToLower();
        string[] txt = str.Split(' ');
        txt = removeBlankStr(txt);

        switch (txt[0]) { 
            case "crash":
                throw new Exception("Debug crash");

            case "logic":
                p_LogicDisabled = txt[1] == "disable";
                break;

            case "warp":
                p_Camera.MoveCenter(
                    Convert.ToInt32(txt[1]),
                    Convert.ToInt32(txt[2]));
                break;

            case "toggle":

                if (txt[1] == "debug") {
                    p_DebugLabel.Visible = !p_DebugLabel.Visible;
                    if (p_DebugLabel.Visible) {
                        p_DebugLabel.Enable();
                    }
                    else {
                        p_DebugLabel.Disable();
                    }
                    if (txt.Length == 3 && txt[2] == "full") {
                        p_DebugFull = !p_DebugFull;
                    }
                }
                if (txt[1] == "fog") {
                    EnableFog = !EnableFog;
                }
                if (txt[1] == "los") {
                    EnableLOS = !EnableLOS;
                }

                break;
        }
    }
    private string[] removeBlankStr(string[] array) {
        string[] rebuild = new string[0];
        int l = array.Length;
        for (int c = 0; c < l; c++) {
            string s = array[c];
            if (s.Replace(" ", "").Length == 0) { continue; }

            Array.Resize(ref rebuild, rebuild.Length + 1);
            rebuild[rebuild.Length - 1] = s;
        }
        return rebuild;

    }

    public delegate T Func<T>();
    private void updateDebug() {
        #region get debug string
        string dbgString = p_RenderHeartbeat.Rate + "fps";

        Fog fog = p_CurrentPlayer.Fog;

        if (p_DebugFull) {
            Point mousePosition = Point.Empty;
            try {
                mousePosition = (Point)p_Window.Invoke((Func<Point>)delegate {
                    return p_Window.PointToClient(Cursor.Position);
                });
            }
            catch { }

            VisibleBlock blockAtCursor = p_Map.TryGetBlockAtPoint(p_Window.Context, mousePosition);
            dbgString =
                "Keys: keys[" + p_CurrentKeys + "] arw[" + p_ArrowKeyDown + "]\n" +
                "Camera position: " + p_Camera.X + "x" + p_Camera.Y + "\n" +
                "Blocks rendered: " + p_Map.VisibleBlocks.Count + "\n" +
                "Blocks revealed: " + fog.BlocksRevealed + "/" + (p_Map.Width * p_Map.Height) +
                    " [" + (fog.BlocksRevealed * 1.0f / (p_Map.Width * p_Map.Height) * 100).ToString("0.00") + "%]\n" +
                "Block size: " + p_Camera.BlockWidth + "x" + p_Camera.BlockHeight + "\n" +
                "Cursor position: [L]" +
                    getPointString(mousePosition) + " [S]" +
                    getPointString(Cursor.Position) + " [B]" +
                    getPointString(new Point(blockAtCursor.BlockX, blockAtCursor.BlockY)) + "\n" +
                "Window size: " + getSizeString(new Size(p_Window.Context.Width, p_Window.Context.Height)) + "\n" +
                "Render: " + getHeartbeatString(p_RenderHeartbeat) + "\n" +
                "Logic: " + getHeartbeatString(p_LogicHeartbeat);
        }
        #endregion

        p_DebugLabel.Text = dbgString;
        p_DebugLabel.Location = new Point(
            p_Window.Context.Width - p_DebugLabel.Width - 10,
            p_Window.Context.Height - p_DebugLabel.Height);
    }
    #endregion

    #region Logic
    private bool p_EnableFog = true;
    private bool p_EnableLOS = true;

    private bool p_LogicDisabled = false;
    private Heartbeat p_LogicHeartbeat;

    private void initLogic() {
        ResourceStockPile.RegisterResource("Wood");
        ResourceStockPile.RegisterResource("Food");
        ResourceStockPile.RegisterResource("Gold");
        ResourceStockPile.RegisterResource("Stone");

        p_LogicHeartbeat = new Heartbeat("logic");
        p_LogicHeartbeat.Speed(10);
        p_LogicHeartbeat.Start(this, updateLogic);
    }
    
    private void updateLogic(object state) {
        if (p_LogicDisabled) { return; }
        if (p_RenderHeartbeat == null) { return; }

        Game game = state as Game;
        game.updateMouse();
        game.updateCamera();
        updateDebug();
        updateUI();      

        /*has the window closed?*/
        if (p_Window.Closed) {
            //stop all heartbeats
            p_RenderHeartbeat.Stop();

            //force stop logic heartbeat 
            //since normal Stop() would cause a deadlock
            //as we are stopping a thread we are on right now.
            //we while(true) to prevent the thread from executing 
            //any further during abort (to prevent corruption)
            p_LogicHeartbeat.ForceStop();
            while (true) ;
        }
    }
        
    private void updateCamera() {
        /*
         *  calculate camera steps based off the current frame rate.
            the faster the fps, the less of a step
         */
        int stepX, stepY;
        stepX = stepY = 15;

        stepX = 25 - (int)(p_RenderHeartbeat.Rate / 60 * 10);
        stepY = stepX;


        /*based off the arrow key flags, adjust the camera
            accordingly.*/
        if ((p_ArrowKeyDown & ArrowKey.LEFT) == ArrowKey.LEFT) {
            p_Camera.Move(-stepX, 0);
        }
        if ((p_ArrowKeyDown & ArrowKey.RIGHT) == ArrowKey.RIGHT) {
            p_Camera.Move(stepX, 0);
        }
        if ((p_ArrowKeyDown & ArrowKey.UP) == ArrowKey.UP) {
            p_Camera.Move(0, -stepY);
        }
        if ((p_ArrowKeyDown & ArrowKey.DOWN) == ArrowKey.DOWN) {
            p_Camera.Move(0, stepY);
        }

    }
    
    public Player CurrentPlayer { get { return p_CurrentPlayer; } }
    public bool EnableFog {
        get { return p_EnableFog; }
        set {
            if (value == p_EnableFog) { return; }
            p_EnableFog = value;
            p_Map.Invalidate();
            
        }
    }
    public bool EnableLOS {
        get { return p_EnableLOS; }
        set { p_EnableLOS = value; }
    }
    #endregion

    #region Players
    private Player[] p_Players;
    private Player p_CurrentPlayer;

    private void initPlayers() {

        p_Players = new Player[] {             
            new Player(this, 0, Color.Red),
            new Player(this, 1, Color.Blue),
            new Player(this, 2, Color.Green)
        
        };

        p_CurrentPlayer = p_Players[0];

    }

    #endregion

    #region Mouse
    private UIClickDrag p_ClickDrag;
    private bool p_MouseDown = false;
    private Point p_MousePosition;

    private void updateMouse() {
        //control hijacked?
        if (p_EventHijacker != null) { return; }

        //logic disabled?
        if (p_LogicDisabled) { return; }

        //game has focus?
        if (!p_Window.Focused) { return; }

        //clickdrag active?
        if (p_ClickDrag.Enabled) { return; }

        //get the cursor position relative to the window content
        Point mousePosition = p_MousePosition;

        //is the mouse at the side of the screen so we move the camera?
        int width = p_Window.Context.Width;
        int height = p_Window.Context.Height;
        int margin = 40;
        int x = mousePosition.X;
        int y = mousePosition.Y;
        int step = 10;

        //if the cursor exceeds the bounds of the window at all, we ignore
        //the processing of moving the camera.
        if (x < 0 || y < 0 ||
           x > width || y > height) { return; }


        if (x < margin) { p_Camera.Move(-step, 0); }
        if (y < margin) { p_Camera.Move(0, -step); }

        if (x > width - margin) {
            p_Camera.Move(step, 0);
        }
        if (y > height - margin) {
            p_Camera.Move(0, step);
        }
    }
    private void handleMouseMove(object sender, MouseEventArgs e) {
        if (p_LogicDisabled) { return; }

        //resolve mouse position relative to the window position
        //(we get a 0,0 for mouse in top, left corner of window
        p_MousePosition = p_Window.PointToClient(Cursor.Position);

        if (p_EventHijacker != null) {
            p_EventHijacker.OnMouseMove(this, p_MousePosition, e);
        }

        /*handle mouse down IF the mouse is clicked, so whenever the 
          mouse moves and the mouse is down, the event is called.
         Native MouseDown event does not have this behavior*/
        if (e.Button != MouseButtons.None) {
            handleMouseDown(sender, e);
            return;
        }

        //fire mousemove for all ui elements
        int uiL = p_UIElements.Length;
        for (int c = 0; c < uiL; c++) {
            p_UIElements[c].OnMouseMove(this, p_MousePosition, e);
        }
    }
    private void handleMouseDown(object sender, MouseEventArgs e) {
        if (p_LogicDisabled) { return; }
        p_MouseDown = true;

        if (p_EventHijacker != null) {
            p_EventHijacker.OnMouseDown(this, p_MousePosition, e);
        }

        /*fire for all ui elements BUT the click drag and test if
          the mouse collids with any, if so, we don't call click+drag*/
        int uiL = p_UIElements.Length;
        bool callClickDrag = true;
        for (int c = 0; c < uiL; c++) {
            IUI ui = p_UIElements[c];
            ui.OnMouseDown(this, p_MousePosition, e);
        }

    }
    private void handleMouseUp(object sender, MouseEventArgs e) {
        if (p_LogicDisabled) { return; }
        p_MouseDown = false;

        //resolve mouse position relative to the window position
        //(we get a 0,0 for mouse in top, left corner of window
        if (p_EventHijacker != null) {
            p_EventHijacker.OnMouseUp(this, p_MousePosition, e);
        }

        /*fire for all ui elements mouse up*/
        int uiL = p_UIElements.Length;
        for (int c = 0; c < uiL; c++) {
            p_UIElements[c].OnMouseUp(this, p_MousePosition, e);
        }
    }
    private void handleMouseScroll(object sender, MouseEventArgs e) {
        if (p_LogicDisabled) { return; }

        if (p_EventHijacker != null) {
            p_EventHijacker.OnMouseScroll(this, p_MousePosition, e);
        }

        //get the block where the mouse is currently at
        bool hasBlock;
        VisibleBlock block = p_Map.TryGetBlockAtPoint(p_Window.Context, p_MousePosition, out hasBlock);
       
        //adjust zoom
        int v = (e.Delta < 0 ? -1 : 1);
        p_Camera.Zoom(v * 10, v * 10);

        //move camera to the block where the mouse is.
        if (hasBlock) {
            p_Camera.MoveCenter(block.BlockX, block.BlockY);
        }

    }
    private void handleMouseClick(object sender, EventArgs e) {
        if (p_MouseDown || p_LogicDisabled) { return; }


        pathTest(p_MousePosition);

        //clear selected
        uiBlocksSelected(null);
       
        MouseEventArgs args = e as MouseEventArgs;
        if (p_EventHijacker != null) {
            p_EventHijacker.OnMouseClick(this, p_MousePosition, args);
        }

        int l = p_UIElements.Length;
        for (int c = 0; c < l; c++) {
            p_UIElements[c].OnMouseClick(this, p_MousePosition, args);
        }
    }
    private void handleMouseLeave(object sender, EventArgs e) {
        p_MousePosition = p_Window.PointToClient(Cursor.Position);
    }

    int pathState = 0;
    Point pathStart;
    Point pathEnd;

    private unsafe void pathTest(Point mousePosition) {
        Block* matrix = Map.GetBlockMatrix();

        if (pathState == 2) {
            pathState = 0;

            Block* ptr = matrix;
            Block* ptrEnd = ptr + (Map.Width * Map.Height);
            while (ptr != ptrEnd) {
                (*(ptr++)).Selected = false;
            }
            
        }

        VisibleBlock vBlock = default(VisibleBlock);
        try {
            vBlock = Map.GetBlockAtPoint(p_Window.Context, mousePosition);
        }
        catch { return; }
        Point blockLocation = new Point(
            vBlock.BlockX,
            vBlock.BlockY);
        Block* block = vBlock.Block;
        if (block == (Block*)0) { return; }
        if ((*block).StateID != -1) { return; }

        Console.WriteLine(pathState);

        switch (pathState) { 
            case 0:
                pathStart = blockLocation;
                break;
            case 1:
                pathEnd = blockLocation;
                List<Point> path = Pathfinder.ASSearch(
                    pathStart,
                    pathEnd,
                    Map.GetConcreteMatrix(),
                    Map.Width,
                    Map.Height);

                if (path.Count == 0) {
                    MessageBox.Show("Path not found!");
                }

                foreach (Point p in path) {

                    Block* b = matrix + (p.Y * Map.Width) + p.X;

                    (*b).Selected = true;

                }

                break;
        }


        pathState++;
    }
    #endregion

    #region Keyboard
    private ArrowKey p_ArrowKeyDown;
    private Keys p_CurrentKeys;

    public bool IsArrowKeyPressed {
        get { return p_ArrowKeyDown != ArrowKey.NONE; }
    }
    public ArrowKey ArrowKeys {
        get { return p_ArrowKeyDown; }
    }
    public Keys CurrentKeys {
        get { return p_CurrentKeys; }
    }

    private void handleKeyDown(object sender, KeyEventArgs e) {
        /*update key modifier*/
        p_CurrentKeys = e.KeyCode;
        
        if (p_EventHijacker != null) {
            p_EventHijacker.OnKeyDown(this, e);
            return;
        }

        if (e.Modifiers != Keys.None) { return; }

        fogFuck(e.KeyCode);

        /*by doing it this way, a player can press 2 keys to make the
         player go diagonal.*/
        switch (e.KeyCode) { 
            /*debug*/
            case Keys.Enter:
                debugPrompt();
                break;
            case Keys.F3:
                p_DebugLabel.Visible = true;
                p_DebugLabel.Enable();
                break;

            /*arrow key pressed?*/
            case Keys.Left:
            case Keys.A:
                p_ArrowKeyDown |= ArrowKey.LEFT;
                break;

            case Keys.Right:
            case Keys.D:
                p_ArrowKeyDown |= ArrowKey.RIGHT;
                break;

            case Keys.Up:
            case Keys.W:
                p_ArrowKeyDown |= ArrowKey.UP;
                break;

            case Keys.Down:
            case Keys.S:
                p_ArrowKeyDown |= ArrowKey.DOWN;
                break;
        }

        //if logic is disabled, do not allow arrow keys but
        //the user might have entered debug mode
        if (p_LogicDisabled) {
            p_ArrowKeyDown = 0;
            return;
        }

        /*send to all UI elements*/
        int l = p_UIElements.Length;
        for (int c = 0; c < l; c++) {
            p_UIElements[c].OnKeyDown(this, e);
        }
    }
    private void handleKeyUp(object sender, KeyEventArgs e) {
        p_CurrentKeys = Keys.None;
        if (p_LogicDisabled) { return; }
        
        if (p_EventHijacker != null) {
            p_EventHijacker.OnKeyUp(this, e);
            return;
        }

        /*adjust arrow key flags if we have an arrow key released*/
        switch (e.KeyCode) { 
            case Keys.Left:
            case Keys.A:
                p_ArrowKeyDown -= ArrowKey.LEFT;
                break;

            case Keys.Right:
            case Keys.D:
                p_ArrowKeyDown -= ArrowKey.RIGHT;
                break;

            case Keys.Up:
            case Keys.W:
                p_ArrowKeyDown -= ArrowKey.UP;
                break;

            case Keys.Down:
            case Keys.S:
                p_ArrowKeyDown -= ArrowKey.DOWN;
                break;
        }

        /*send to all UI elements*/
        int l = p_UIElements.Length;
        for (int c = 0; c < l; c++) {
            p_UIElements[c].OnKeyUp(this, e);
        }
    }

    LineOfSight sight;
    private void fogFuck(Keys key) {
        

        int xv = 0;
        int yv = 0;
        int s = 2;
        int r = 5;
        switch (key) { 
            case Keys.NumPad8:
                yv = -s; break;
            case Keys.NumPad2:
                yv = s; break;
            case Keys.NumPad4:
                xv = -s; break;
            case Keys.NumPad6:
                xv = s; break;
            case Keys.NumPad5:
                r = 10; break;

            case Keys.NumPad7:
                xv = -s; yv = -s; break;
            case Keys.NumPad9:
                xv = s; yv = -s; break;
            case Keys.NumPad1:
                xv = -s; yv = s; break;
            case Keys.NumPad3:
                xv = s; yv = s; break;
            default:
                return;
        }



        sight.CenterX += xv;
        sight.CenterY += yv;
        sight.Radius = r;
        p_CurrentPlayer.Fog.UpdateLOS();
        p_Map.Invalidate();
    }
    #endregion

    #region UI
    private IUI[] p_UIElements;
    private LinkedList<VisibleBlock> p_SelectedBlocks;

    private void initUI() {
        p_UIElements = new IUI[0];

        /*add click and drag*/
        p_ClickDrag = (UIClickDrag)addUI(
            new UIClickDrag());
        p_ClickDrag.BlocksSelected += uiBlocksSelected;

        addUI(new HUDMenu(this));
    }

    private void updateUI() { 
        int l = p_UIElements.Length;
        for (int c = 0; c < l; c++) {
            if (!p_UIElements[c].Enabled) { continue; }
            p_UIElements[c].Update();
        }
    }
    private void drawUI(IRenderContext context, IRenderer renderer) {
        //draw all ui elements
        int l = p_UIElements.Length;
        for (int c = 0; c < l; c++) {
            if (!p_UIElements[c].Visible) { continue; }
            p_UIElements[c].Draw(context, renderer);
        }
    }


    private IUI addUI(IUI ui) {
        Array.Resize(ref p_UIElements, p_UIElements.Length + 1);
        p_UIElements[p_UIElements.Length - 1] = ui;
        return ui;
    }

    private void handleFocusChanged(object sender, EventArgs e) {
        GameWindow wnd = sender as GameWindow;

        //reset arrow keys in case the user was scrolling
        //then defocus. This otherwise would scroll indefinately
        //even if the user is not pressing the key.
        p_ArrowKeyDown = 0;
    }
    private void handleResize(object sender, EventArgs e) {
        //get the center point on screen
        IRenderContext ctx = p_Window.Context;
        int cWidth = ctx.Width / 2;
        int cHeight = ctx.Height / 2;

        //get what the center block currently is on screen
        Camera cam = p_Camera;

        int blockX, blockY;
        blockX = (int)Math.Ceiling((cam.X + cWidth) * 1.0f / cam.BlockWidth);
        blockY = (int)Math.Ceiling((cam.Y + cHeight) * 1.0f / cam.BlockHeight);

        //handle the actual resize by resizing the context
        p_Window.RecreateContext();

        //move the camera to the center block
        cam.MoveCenter(
            blockX,
            blockY);
    }
       
    private unsafe void uiBlocksSelected(LinkedList<VisibleBlock> blocks) {
        if (p_SelectedBlocks != null) {
            foreach (VisibleBlock b in p_SelectedBlocks) {
                (*(b.Block)).Selected = false;
            }
            p_SelectedBlocks.Clear();
            p_SelectedBlocks = null;
        }

        if (blocks == null) { return; }
        foreach (VisibleBlock b in blocks) {
            if ((*b.Block).StateID == -1) { continue; }

            ResourceState r = Map.Resources.Resolve((*b.Block).StateID);
            ResourceStockPile.CollectFromBlock(
                p_Map,
                *b.Block,
                r.Amount);

            (*b.Block).StateID = -1;
        }
        p_SelectedBlocks = blocks;
    
    }
    #endregion

    #region Crash handling
    [DllImport("kernel32.dll")]
    static extern bool AllocConsole();
    private void handleCrash(object sender, System.Threading.ThreadExceptionEventArgs e) {
        try { crash("A fatal error occured that could not be recovered from", e.Exception); }
        catch (Exception cex) {
            AllocConsole();
            Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput()) {
                AutoFlush = true
            });

            string filename;
            string contents = crashToFile(e.Exception, cex, out filename);
            Console.WriteLine(contents);
            Console.WriteLine("Information about the crash has been saved to \"" +
                new FileInfo(filename).FullName
                + "\"");
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
    private void crash(string reason, Exception ex) {
        //disable everything
        try {
            p_RenderHeartbeat.ForceStop();
            p_LogicHeartbeat.ForceStop();
        }
        catch { }

        //completely renew the window
        p_Window.Controls.Clear();
        p_Window.UnhookCoreEvents();
        p_Window.Click -= handleMouseClick;
        p_Window.MouseMove -= handleMouseMove;
        p_Window.MouseUp -= handleMouseUp;
        p_Window.MouseDown -= handleMouseDown;
        p_Window.MouseWheel -= handleMouseScroll;
        p_Window.KeyDown -= handleKeyDown;
        p_Window.KeyUp -= handleKeyUp;
        p_Window.Resize -= handleResize;
        p_Window.GotFocus -= handleFocusChanged;
        p_Window.LostFocus -= handleFocusChanged;

        //save
        string filename;
        string repString = crashToFile(ex, null, out filename);
        
        //any key to exit
        p_Window.FormClosing += delegate(object sender, FormClosingEventArgs e) {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        };
        p_Window.KeyDown += delegate(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.F3) {
                p_RenderHeartbeat.Stop();

                RichTextBox rtb = new RichTextBox() { 
                    ReadOnly = true,
                    Dock = DockStyle.Fill
                };
                rtb.Text = repString;
                p_Window.Controls.Add(rtb);
                if (p_Window.IsFullScreen) {
                    p_Window.ToggleFullscreen();
                }
                return;
            }
            p_Window.Close();
        };

        //hijack rendering
        p_RenderHeartbeat = new Heartbeat("render");
        p_RenderHeartbeat.Speed(50);
        p_RenderHeartbeat.Start(this, delegate(object st) {
            Game gm = st as Game;

            IRenderContext ctx = gm.p_Window.Context;
            IRenderer renderer = gm.p_Window.Renderer;

            renderer.BeginFrame(ctx);

            renderer.SetColor(Color.Black);
            renderer.Clear();            

            renderer.SetFont(new Font("Arial", 70, FontStyle.Bold));
            Size crashTxtSize = renderer.MeasureString(":(");
            
            /*title*/
            Rectangle crashBounds = new Rectangle(
                (ctx.Width / 2) - (crashTxtSize.Width / 2),
                (ctx.Height / 2) - (crashTxtSize.Height / 2),
                crashTxtSize.Width,
                crashTxtSize.Height);
            renderer.SetBrush(Brushes.White);
            renderer.DrawString(":(", crashBounds.X, crashBounds.Y);

            /*reason*/
            renderer.SetFont(new Font("Arial", 12, FontStyle.Regular));
            Size reasonTxtSize = renderer.MeasureString(reason);
            renderer.DrawString(
                reason,
                (ctx.Width / 2) - (reasonTxtSize.Width / 2),
                crashBounds.Y + crashBounds.Height + 20);

            /*advice*/
            string advice = "Press any key to exit or press F3 to view report";
            Size adviceTxtSize = renderer.MeasureString(advice);
            renderer.DrawString(
                advice,
                (ctx.Width / 2) - (adviceTxtSize.Width / 2),
                crashBounds.Y + crashBounds.Height + 20 + reasonTxtSize.Height + 5);

            renderer.EndFrame();
        });


    }
    private string crashToFile(Exception ex, Exception crashEx, out string filename) {
        StringWriter print = new StringWriter();
        print.WriteLine("=================================================");
        print.WriteLine("+                  Fatal error                  +");
        print.WriteLine("=================================================");
        print.WriteLine(printException(ex));
        print.WriteLine("\n\n\n");

        if (crashEx != null) {
            print.WriteLine("=================================================");
            print.WriteLine("+                 crash() error                 +");
            print.WriteLine("=================================================");
            print.WriteLine(printException(crashEx));
        }

        DateTime now = DateTime.Now;
        string str = print.ToString();
        filename =
            "CRASH_" + 
            now.Day.ToString("00") +
            now.Month.ToString("00") +
            now.Year.ToString("00") +
            now.Hour.ToString("00") +
            now.Minute.ToString("00") +
            now.Second.ToString("00");
        filename += ".txt";
        
        File.WriteAllText(filename, str);
        return str;
        
    }
    private string printException(Exception ex) {
        string buffer = "";
        buffer +=    "Msg: \"" + ex.Message + "\"\r\n" +
                     "Source: \"" + ex.Source + "\"\r\n" +
                     "Target site: \"" + ex.TargetSite + "\"\r\n" +
                     "Stack:\r\n" + ex.StackTrace;
        return buffer;
    }
    #endregion
}
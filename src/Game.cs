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
    private MapRenderer p_MapRenderer;
    private Hotloader p_Hotloader;

    public Game(GameWindow wnd) {
        p_Window = wnd;
       
        /*hook crash events at the first opportunity.*/
        Application.ThreadException += handleCrash;

        /*fullscreen*/
        p_Window.Shown += delegate(object s, EventArgs e) {
            //p_Window.ToggleFullscreen();
            p_Window.Focus();
            p_Window.BringToFront();

            cmd("warp 0 0");
            cmd("zoom 0 0");

            cmd("toggle debug");
            cmd("toggle debug full");

            cmd("toggle fog");
            cmd("toggle los");

            testCode();
        };

        /*initialize hotloader*/
        p_Hotloader = new Hotloader();

        /*initialize the camera*/
        p_Camera = new Camera(this);

        /*initialize map*/
        p_Map = new Map(this, 1000, 1000);
        p_MapRenderer = new MapRenderer(this, p_Map, p_Camera);

        /*setup camera position*/
        p_Camera.ZoomAbs(32);
        p_Camera.MoveCenter(250, 250);

        /*init sub-systems*/
        initUI();
        initMouse();
        initPlayers();
        initLogic();
        initDebug();
        initDraw();

        /*hook events*/
        p_Cursor.HookEvents();
        wnd.Click += handleMouseClick;
        wnd.MouseWheel += handleMouseScroll;
        wnd.MouseMove += handleMouseMove;
        wnd.MouseUp += handleMouseUp;
        wnd.KeyDown += handleKeyDown;
        wnd.KeyUp += handleKeyUp;
        wnd.Resize += handleResize;
        wnd.GotFocus += handleFocusChanged;
        wnd.LostFocus += handleFocusChanged;

        unsafe {
            IPathfinderContext ctx = Pathfinder.ASCreateContext(p_Map.Width, p_Map.Height);
            while (true) {
                break;
                int time = Environment.TickCount;
                //break;
                List<Point> lol = Pathfinder.ASSearch(
                    ctx,
                    Point.Empty,
                    new Point(p_Map.Width - 1, p_Map.Height - 1),
                    p_Map.GetConcreteMatrix(true));

                Console.WriteLine((Environment.TickCount - time) + "ms for " + lol.Count + " items");
            }
        }
        wnd.HookCoreEvents();

        p_Camera.EnableMargin = true;
        p_Camera.SetMargin(10, 10);
    }

    public Camera Camera { get { return p_Camera; } }
    public Map Map { get { return p_Map; } }
    public MapRenderer MapRenderer { get { return p_MapRenderer; } }
    public GameWindow Window { get { return p_Window; } }

    private IUI p_EventHijacker;
    public void HijackEvents(IUI ui) {
        if (p_EventHijacker != null) {
            
            throw new Exception("A UI element already has control over events.");
        }

        Console.WriteLine("HIJACK");
        p_EventHijacker = ui;
    }
    public void DetatchFromEventsHijack(IUI ui) {
        Console.WriteLine("HIJACK");
        if (ui != p_EventHijacker) {
            throw new Exception("UI element did not hijack events");
        }
        p_EventHijacker = null;
    }

    private void testCode() {
        Fog f = CurrentPlayer.Fog;

        sight = new LineOfSight(250, 250, 5);
        p_CurrentPlayer.Fog.AddLOS(sight);
        p_CurrentPlayer.Fog.UpdateLOS();
       

        p_Window.MouseDown += delegate(object sender, MouseEventArgs e) {
            Point mousePosition = PointToClient(Cursor.Position);
            pathTest(mousePosition, e.Button);
        };
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

        p_MapRenderer.Draw(context, renderer);

        drawUI(context, renderer);

        p_Cursor.Draw(context, renderer);


        renderer.SetFont(new Font("Arial", 12, FontStyle.Bold));
        renderer.DrawString(
            p_Hotloader["print"].ToString(),
            10, 100);


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
    private UITextBox p_DebugTextBox;

    private void initDebug() {
        p_DebugLabel = (UILabel)addUI(new UILabel(this));
        p_DebugLabel.TextAlignment = TextAlign.Right;
        p_DebugLabel.ForeBrush = Brushes.White;
        p_DebugLabel.Font = new Font("Arial", 12, FontStyle.Bold);
        p_DebugLabel.Disable();

        p_DebugTextBox = (UITextBox)addUI(new UITextBox(this));
        p_DebugTextBox.Visible = false;
        p_DebugTextBox.Text = "";
        p_DebugTextBox.Font = new Font("Arial", 20, FontStyle.Regular);
        p_DebugTextBox.Width = 400;

        p_DebugTextBox.KeyDown += delegate(Game game, KeyEventArgs e) {            
            if (e.KeyCode != Keys.Enter) { return; }
            
            p_DebugPrompt = false;
            p_DebugTextBox.Visible = false;
            p_DebugTextBox.RemoveFocus();
            try { cmd(p_DebugTextBox.Text); }
            catch (Exception ex) {
                if (ex.Message == "Debug crash") {
                    throw ex;
                }
            }
            return;
        };
    }

    private void debugPrompt() {
        if (p_DebugPrompt) { return; }

        p_DebugPrompt = true;
        p_DebugTextBox.Location = new Point(
            (p_Window.ClientSize.Width / 2) - (p_DebugTextBox.Width / 2),
            (p_Window.ClientSize.Height / 2) - (p_DebugTextBox.Height / 2));
        p_DebugTextBox.Text = "";
        p_DebugTextBox.Visible = true;
        p_DebugTextBox.Focus();    
    }
    private void cmd(string str) {
        string[] txt = str.ToLower().Split(' ');
        txt = removeBlankStr(txt);

        if (txt.Length == 0) { return; }
        
        switch (txt[0]) { 
            case "crash":
                throw new Exception("Debug crash");

            case "clear":
                p_Messages.Clear();
                break;

            case "speed":
                if (txt[1] == "logic") {
                    p_LogicHeartbeat.Speed(Convert.ToInt32(txt[2]));
                }
                else if (txt[1] == "render") {
                    p_RenderHeartbeat.Speed(Convert.ToInt32(txt[2]));
                }
                break;

            case "eval":
                str = str.Substring(5);
                string result = "";
                try {
                    result = p_Hotloader.EvaluateExpression(str).ToString();
                }
                catch (HotloaderParserException ex) {
                    result = ex.Message;
                }

                p_Messages.AddMessage(result, Color.White);
                break;
            case "logic":
                p_LogicDisabled = txt[1] == "disable";
                break;

            case "warp":
                p_Camera.MoveCenter(
                    Convert.ToInt32(txt[1]),
                    Convert.ToInt32(txt[2]));
                break;
            case "zoom":
                if (txt[1] == "force") {
                    p_Camera.ForceZoomAbs(Convert.ToInt32(txt[2]));
                }
                else { 
                    p_Camera.ZoomAbs(Convert.ToInt32(txt[1])); 
                }
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
                
                else if (txt[1] == "fog") { EnableFog = !EnableFog; }
                else if (txt[1] == "los") { EnableLOS = !EnableLOS; }
                else if (txt[1] == "grid") { p_MapRenderer.ShowGrid = !p_MapRenderer.ShowGrid; }

                break;
            default:
                p_Messages.AddMessage(
                    "Player: " + str,
                    Color.White); 
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

        try {
            p_Window.Invoke(new MethodInvoker(delegate {
                try { p_Window.Text = dbgString + " (" + p_Window.Renderer + ") Build: " + Globals.BUILD; }
                catch { }
            }));
        }
        catch { }

        Fog fog = p_CurrentPlayer.Fog;

        if (p_DebugFull) {
            Point mousePosition = Point.Empty;
            try {
                mousePosition = (Point)p_Window.Invoke((Func<Point>)delegate {
                    return p_Window.PointToClient(Cursor.Position);
                });
            }
            catch { }

            VisibleBlock blockAtCursor = p_MapRenderer.TryGetBlockAtPoint(p_Window.Context, mousePosition);
            dbgString =
                "Keys: keys[" + p_CurrentKeys + "] arw[" + p_ArrowKeyDown + "]\n" +
                "Camera position: " + p_Camera.X + "x" + p_Camera.Y + "\n" +
                "Vertices: " + p_Window.Renderer.Vertices + "\n" + 
                "Blocks rendered: " + p_MapRenderer.VisibleBlocks.Count + "\n" +
                "Blocks revealed: " + fog.BlocksRevealed + "/" + (p_Map.Width * p_Map.Height) +
                    " [" + (fog.BlocksRevealed * 1.0f / (p_Map.Width * p_Map.Height) * 100).ToString("0.00") + "%]\n" +
                "Block size: " + p_Camera.BlockSize + " (" + (p_Camera.BlockSizeScalar*100).ToString("0.00")+"%)\n" + 
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
            p_Window.Context.Height - p_DebugLabel.Height - 10);
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
            p_Hotloader.Dispose();

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
        if (p_ArrowKeyDown == ArrowKey.NONE) { return; }

        /*
         *  calculate camera steps based off the current frame rate.
            the faster the fps, the less of a step
         */
        int stepX, stepY;
        stepX = stepY = 20;

        //stepX = 25 - (int)(p_RenderHeartbeat.Rate / 60 * 10);
        //stepY = stepX;

        /*update cursor*/
        Direction arrowDirection = translateArrowDirection(p_ArrowKeyDown);
        if (arrowDirection != Direction.ALL) {
            p_Cursor.SetArrow(arrowDirection);
        }

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
            p_MapRenderer.Invalidate();
            
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
    private UICursor p_Cursor;
    private UIPan p_Pan;

    private bool p_MouseDown = false;

    private void initMouse() {
        p_Cursor = new UICursor(this);
        p_Cursor.Enable();

        p_Pan = new UIPan(this, p_Cursor);
        addUI(p_Pan);
    }
    private void updateMouse() {
        //pan active?
        if (p_Pan.Enabled) { return; }

        if (
            //control hijacked?
            p_EventHijacker != null ||

            //logic disabled?
            p_LogicDisabled ||

            //game has focus?
            !p_Window.Focused ||

            //arrow key?
            p_ArrowKeyDown != ArrowKey.NONE ||

            //clickdrag active?
            p_ClickDrag.Enabled) {
                p_Cursor.SetArrow(Direction.NONE);
                return;
        }

        //get the cursor position relative to the window content
        Point mousePosition = PointToClient(Cursor.Position);

        //is the mouse at the side of the screen so we move the camera?
        int width = p_Window.Context.Width;
        int height = p_Window.Context.Height;
        int margin = 40;
        int x = mousePosition.X;
        int y = mousePosition.Y;
        int step = 10;
        int dX = 0, dY = 0;

        //if the cursor exceeds the bounds of the window at all, we ignore
        //the processing of moving the camera.
        if (x < 0 || y < 0 || x > width || y > height) { return; }

        if (x < margin) { dX = -step; }
        if (y < margin) { dY = -step; }

        if (x > width - margin) { dX = step; }
        if (y > height - margin) { dY = step; }

        if (dX == 0 && dY == 0) {
            p_Cursor.SetArrow(Direction.NONE);
            return;
        }

        //adjust cursor arrow direction according to change in camera
        Direction direction = Direction.NONE;
        if (dX < 0) { direction |= Direction.WEST; }
        if (dX > 0) { direction |= Direction.EAST; }

        if (dY < 0) { direction |= Direction.NORTH; }
        if (dY > 0) { direction |= Direction.SOUTH; }
        p_Cursor.SetArrow(direction);


        //move camera
        p_Camera.Move(dX, dY);
    }
    private void handleMouseMove(object sender, MouseEventArgs e) {
        if (p_LogicDisabled) { return; }

        //resolve mouse position relative to the window position
        //(we get a 0,0 for mouse in top, left corner of window
        Point mousePosition = PointToClient(Cursor.Position);

        if (p_EventHijacker != null) {
            p_EventHijacker.OnMouseMove(this, mousePosition, e);
        }

        /*handle mouse down IF the mouse is clicked, so whenever the 
          mouse moves and the mouse is down, the event is called.
         Native MouseDown event does not have this behavior*/
        if (e.Button != MouseButtons.None) {
            handleMouseDown(sender, e);
            //return;
        }

        //fire mousemove for all ui elements
        int uiL = p_UIElements.Length;
        for (int c = 0; c < uiL; c++) {
            p_UIElements[c].OnMouseMove(this, mousePosition, e);
        }
    }
    private void handleMouseDown(object sender, MouseEventArgs e) {
        if (p_LogicDisabled) { return; }
        p_MouseDown = true;

        Point mousePosition = PointToClient(Cursor.Position);

        if (p_EventHijacker != null) {
            p_EventHijacker.OnMouseDown(this, mousePosition, e);
        }

        /*fire for all ui elements BUT the click drag and test if
          the mouse collids with any, if so, we don't call click+drag*/
        int uiL = p_UIElements.Length;
        for (int c = 0; c < uiL; c++) {
            IUI ui = p_UIElements[c];
            ui.OnMouseDown(this, mousePosition, e);
        }

    }
    private void handleMouseUp(object sender, MouseEventArgs e) {
        if (p_LogicDisabled) { return; }
        p_MouseDown = false;

        Point mousePosition = PointToClient(Cursor.Position);

        //resolve mouse position relative to the window position
        //(we get a 0,0 for mouse in top, left corner of window
        if (p_EventHijacker != null) {
            p_EventHijacker.OnMouseUp(this, mousePosition, e);
        }

        /*fire for all ui elements mouse up*/
        int uiL = p_UIElements.Length;
        for (int c = 0; c < uiL; c++) {
            p_UIElements[c].OnMouseUp(this, mousePosition, e);
        }
    }
    private void handleMouseScroll(object sender, MouseEventArgs e) {
        if (p_LogicDisabled) { return; }

        Point mousePosition = PointToClient(Cursor.Position);

        if (p_EventHijacker != null) {
            p_EventHijacker.OnMouseScroll(this, mousePosition, e);
        }

        //only scroll if no button is currently down
        if (p_Cursor.MouseButton != MouseButtons.None) {
            return;
        }
        
        //get the block where the mouse is currently at
        bool hasBlock;
        VisibleBlock block = p_MapRenderer.TryGetBlockAtPoint(p_Window.Context, mousePosition, out hasBlock);
      
        //adjust zoom
        int v = (e.Delta < 0 ? -1 : 1);
        p_Camera.Scale(v * 10);

        //move camera to the block where the mouse is.
        if (hasBlock) {
            p_Camera.MoveCenter(block.BlockX, block.BlockY);
        }

    }
    private void handleMouseClick(object sender, EventArgs e) {
        if (p_MouseDown || p_LogicDisabled) { return; }

        Point mousePosition = PointToClient(Cursor.Position);

        //clear selected
        uiBlocksSelected(null);
       
        MouseEventArgs args = e as MouseEventArgs;
        if (p_EventHijacker != null) {
            p_EventHijacker.OnMouseClick(this, mousePosition, args);
        }

        int l = p_UIElements.Length;
        for (int c = 0; c < l; c++) {
            p_UIElements[c].OnMouseClick(this, mousePosition, args);
        }
    }

    Point pathStart;
    private unsafe void pathTest(Point mousePosition, MouseButtons button) {
       
        Block* matrix = Map.GetBlockMatrix();
        VisibleBlock vBlock = default(VisibleBlock);
        try {
            vBlock = p_MapRenderer.GetBlockAtPoint(p_Window.Context, mousePosition);
        }
        catch { return; }
        Point blockLocation = new Point(
           vBlock.BlockX,
           vBlock.BlockY);
        Block* block = vBlock.Block;
        if (block == (Block*)0) { return; }

        //
        if (button == MouseButtons.None) { return; }

        Block* ptr = matrix;
        Block* ptrEnd = ptr + (Map.Width * Map.Height);
        while (ptr != ptrEnd) {
            (*(ptr++)).Selected = false;
        }

        if (button == MouseButtons.Left) {
            pathStart = blockLocation;
            return;
        }
        if (button == MouseButtons.Right) {
            List<Point> path = Pathfinder.ASSearch(
                pathStart,
                blockLocation,
                Map.GetConcreteMatrix(true),
                Map.Width,
                Map.Height);

            if (path.Count == 0) {
                MessageBox.Show("Path not found!");
            }

            foreach (Point p in path) {
                Block* b = matrix + (p.Y * Map.Width) + p.X;

                if ((*b).TypeID == Globals.TERRAIN_WATER) { break; }

                (*b).Selected = true;

            }
        }
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
        p_CurrentKeys = e.KeyData;

        if (p_EventHijacker != null) {
            p_EventHijacker.OnKeyDown(this, e);
            return;
        }

        if (e.Modifiers != Keys.None) { return; }

        fogFuck(e.KeyCode);

        switch (e.KeyCode) { 
            /*debug*/
            case Keys.Enter:
                debugPrompt();
                return;
            case Keys.F3:
                p_DebugLabel.Visible = true;
                p_DebugLabel.Enable();
                break;

            /* arrow key pressed?
               note: by doing it this way, a player can press 2 keys to make the
               player go diagonal.
            */
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

    private Direction translateArrowDirection(ArrowKey key) {
        Direction buffer = Direction.NONE;

        if ((key & ArrowKey.LEFT) == ArrowKey.LEFT) {
            buffer |= Direction.WEST;
        }
        if ((key & ArrowKey.RIGHT) == ArrowKey.RIGHT) {
            buffer |= Direction.EAST;
        }
        if ((key & ArrowKey.UP) == ArrowKey.UP) {
            buffer |= Direction.NORTH;
        }
        if ((key & ArrowKey.DOWN) == ArrowKey.DOWN) {
            buffer |= Direction.SOUTH;
        }
        return buffer;

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


                p_CurrentPlayer.Fog.AddLOS(new LineOfSight(
                    sight.CenterX,
                    sight.CenterY,
                    10));
                break;


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
        p_MapRenderer.Invalidate();
    }
    #endregion

    #region UI
    private IUI[] p_UIElements;
    private List<VisibleBlock> p_SelectedBlocks;
    private UIMessages p_Messages;

    private void initUI() {
        p_UIElements = new IUI[0];

        /*add click and drag*/
        p_ClickDrag = (UIClickDrag)addUI(
            new UIClickDrag());
        p_ClickDrag.BlocksSelected += uiBlocksSelected;

        /*add messages*/
        p_Messages = (UIMessages)addUI(
            new UIMessages(this));
        p_Messages.Location = new Point(10, 40);


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
        blockX = (int)Math.Ceiling((cam.X + cWidth) * 1.0f / cam.BlockSize);
        blockY = (int)Math.Ceiling((cam.Y + cHeight) * 1.0f / cam.BlockSize);

        //handle the actual resize by resizing the context
        p_Window.Context.Resize(
            p_Window.ClientSize.Width,
            p_Window.ClientSize.Height);
        
        //move the camera to the center block
        cam.MoveCenter(
            blockX,
            blockY);
    }
       
    private unsafe void uiBlocksSelected(List<VisibleBlock> blocks) {
        if (p_SelectedBlocks != null) {
            foreach (VisibleBlock b in p_SelectedBlocks) {
                (*(b.Block)).Selected = false;
            }
            p_SelectedBlocks.Clear();
            p_SelectedBlocks = null;
        }

        if (blocks == null) { return; }
        foreach (VisibleBlock b in blocks) {
            (*b.Block).Selected = true;

        }
        p_SelectedBlocks = blocks;
    
    }

    public Point PointToClient(Point p) {
        return new Point(
            p.X - p_Window.MouseOffsetX,
            p.Y - p_Window.MouseOffsetY);
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
            p_Hotloader.ForceDispose();
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
        p_Cursor.UnhookEvents();

        //save
        string filename;
        string repString = crashToFile(ex, null, out filename);
        
        //re-enable mouse
        Cursor.Show();

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

        crashAnimateInit();

        //fallback on GDI+ renderer
        //RenderFactory.FailSafe();

        //re-initialize renderer since we have left the previous
        //one in an unknown state.
        p_Window.RecreateContext();

        //hijack rendering
        p_RenderHeartbeat = new Heartbeat("render");
        p_RenderHeartbeat.Speed(4);
        p_RenderHeartbeat.Start(this, delegate(object st) {
            Game gm = st as Game;

            IRenderContext ctx = gm.p_Window.Context;
            IRenderer renderer = gm.p_Window.Renderer;

            renderer.BeginFrame(ctx);

            renderer.SetColor(Color.Black);
            renderer.Clear();

            crashAnimateDraw(ctx, renderer);

            Font crashFont = new Font("Arial", 70, FontStyle.Bold);
            IFont font = renderer.SetFont(crashFont);
            Size crashTxtSize = renderer.MeasureString(":(", font);
            
            /*title*/
            Rectangle crashBounds = new Rectangle(
                (ctx.Width / 2) - (crashTxtSize.Width / 2),
                (ctx.Height / 2) - (crashTxtSize.Height / 2),
                crashTxtSize.Width,
                crashTxtSize.Height);
            renderer.SetBrush(Brushes.White);
            renderer.DrawString(":(", crashBounds.X, crashBounds.Y);

            /*reason*/
            Font regFont = new Font("Arial", 12, FontStyle.Regular);
            font = renderer.SetFont(regFont);
            Size reasonTxtSize = renderer.MeasureString(reason, font);
            renderer.DrawString(
                reason,
                (ctx.Width / 2) - (reasonTxtSize.Width / 2),
                crashBounds.Y + crashBounds.Height + 20);

            /*advice*/
            string advice = "Press any key to exit or press F3 to view report";
            Size adviceTxtSize = renderer.MeasureString(advice, font);
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


    private class crashAnimatedBlock {
        public int vX, vY;
        public int x, y;
        public int width, height;
        public Color color;

        public crashAnimatedBlock(int px, int py, int vx, int vy, int w, int h, Color c) {
            x = px;
            y = py;
            vX = vx;
            vY = vy;
            color = c;
            width = w;
            height = h;
        }

    }
    private crashAnimatedBlock[] p_CrashAnimatedBlocks;
    private IFont p_CrashAnimatedBlockFont;

    private void crashAnimateInit() {
        Size windowSize = p_Window.ClientSize;
        int width=windowSize.Width;
        int height=windowSize.Height;

        int blockWidth=100;
        int blockHeight=100;

        int v = 4;

        p_CrashAnimatedBlocks = new crashAnimatedBlock[] { 
            
            new crashAnimatedBlock(0, 0, v, v, blockWidth, blockHeight, Color.Red),
            new crashAnimatedBlock(width-blockWidth, 0, -v, v, blockWidth, blockHeight, Color.Lime),
            new crashAnimatedBlock(0, height-blockHeight, v, -v, blockWidth, blockHeight, Color.Blue),
            new crashAnimatedBlock(width-blockWidth,height-blockHeight,-v,-v,blockWidth, blockHeight, Color.Yellow),

            new crashAnimatedBlock((width/2)-(blockWidth/2), (height/2)-(blockHeight/2), v,v, 100,100, Color.Purple)

        };
    }
    private void crashAnimateDraw(IRenderContext ctx, IRenderer renderer) {
        Font blockFont = new Font("Arial", 40, FontStyle.Bold);
        IFont font = renderer.SetFont(blockFont);
        string blockStr = ":(";
        Size strSize = renderer.MeasureString(blockStr, font);

        List<crashAnimatedBlock> swapped = new List<crashAnimatedBlock>();

        Random r = new Random();

        foreach (crashAnimatedBlock b in p_CrashAnimatedBlocks) { 
            //update
            if (r.Next(0, 500) == 100) {
                b.vX = 0;
                b.vY = 0;
            }
            if (r.Next(0, 100) == 30) {
                b.vX += 1;
            }
            if (r.Next(0, 100) == 50) {
                b.vY += 1;
            }
            int max = 4;
            if (b.vX < -max) { b.vX = -max; }
            if (b.vX > max) { b.vX = max; }
            if (b.vY < -max) { b.vY = -max; }
            if (b.vY > max) { b.vY = max; }
            

            b.x += b.vX;
            b.y += b.vY;
            if (b.x < 0 || b.x + b.width > ctx.Width) {
                b.vX = -b.vX;
                b.x += b.vX;
            }
            if (b.y < 0 || b.y + b.height > ctx.Height) {
                b.vY = -b.vY;
                b.y += b.vY;
            }

            //get bounds
            Rectangle bounds = new Rectangle(b.x, b.y, b.width, b.height);

            //collided?
            bool collide = false;
            foreach (crashAnimatedBlock c in p_CrashAnimatedBlocks) {
                if (c == b) { continue; }
                
                Rectangle cB = new Rectangle(c.x, c.y, c.width, c.height);
                if (cB.IntersectsWith(bounds)) { 
                    //swap colors
                    if (!swapped.Contains(c) && !swapped.Contains(b)) {
                        Color t = b.color;
                        b.color = c.color;
                        c.color = t;
                        swapped.Add(c);
                    }

                    collide = true;
                }
            }

            renderer.SetBrush(new SolidBrush(b.color));
            renderer.FillQuad(
                b.x, b.y,
                b.width, b.height);


            renderer.SetBrush(Brushes.Black);
            renderer.DrawString(
                blockStr,
                b.x + (b.width / 2) - (strSize.Width / 2),
                b.y + (b.height / 2) - (strSize.Height / 2));
            if (collide) {
                b.vX = -b.vX;
                b.vY = -b.vY;
            }

        }

        renderer.SetBrush(new SolidBrush(Color.FromArgb(140, 0, 0, 0)));
        renderer.FillQuad(
            0, 0,
            ctx.Width, ctx.Height);


    }
    #endregion
}
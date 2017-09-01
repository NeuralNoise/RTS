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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;

public sealed class UICursor {
    private bool p_Enabled = false;
    private bool p_PanEnabled = true;
    private Game p_Game;
    private Size p_Size;
    private Direction p_CurrentArrow;
    private MouseButtons p_CurrentButton;
    private PointF[][] p_Polygon;
    
    private object p_Mutex = new object();


    private ITexture p_CursorTexture;

    public UICursor(Game game) {
        p_Game = game;
        p_Size = new Size(20, 20);
    }
    
    private Bitmap getCursorBitmap() {
        Bitmap bmp = (Bitmap)Bitmap.FromFile("cursor.png");

        Bitmap buffer = new Bitmap(bmp.Width, bmp.Height);

        /*THIS IS TEMPORARY, I KNOW THIS IS SLOWWWWW BUT 
          ONCE WE HAVE A SPRITESHEET SYSTEM SETUP, WE WILL
          USE THAT.*/
        int w = bmp.Width, h = bmp.Height;
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                Color pxl = bmp.GetPixel(x, y);

                int data = pxl.ToArgb();
                data &= 0x00FFFFFF;

                if (data != 0xFF00FF) {
                    buffer.SetPixel(x, y, pxl);
                }

            }
        }
        bmp.Dispose();
        return buffer;
    }

    public void Draw(IRenderContext context, IRenderer renderer) {
        if (!p_Enabled) { return; }
        Monitor.Enter(p_Mutex);

        //allocate the cursor
        if (p_CursorTexture == null) {
            p_CursorTexture = context.AllocateTexture(getCursorBitmap(), "cursor");
        }
        
        //get the current mouse position relative to the window
        Point mousePosition = p_Game.PointToClient(Cursor.Position);

        /*draw arrow?*/
        if (p_CurrentArrow != Direction.NONE) {
            int mouseX = mousePosition.X;
            int mouseY = mousePosition.Y;
            int width = p_Size.Width;
            int height = p_Size.Height;

            //white polygon
            renderer.SetBrush(Brushes.White);

            /*draw all polygons for the arrow on screen and translate
              position and scale*/
            int polyLength = p_Polygon.Length;
            for (int y = 0; y < polyLength; y++) {
                PointF[] scale = p_Polygon[y];
                int pointLength = scale.Length;
                Point[] points = new Point[pointLength];

                for (int x = 0; x < pointLength; x++) {
                    PointF p = scale[x];
                    points[x] = new Point(
                        (int)(p.X * width) + mouseX,
                        (int)(p.Y * height) + mouseY);
                }

                //draw poly
                renderer.FillPoly(points);
            }

            Monitor.Exit(p_Mutex);
            return;
        }

        renderer.SetTexture(p_CursorTexture);
        renderer.DrawTextureUnscaled(
            mousePosition.X,
            mousePosition.Y);
        Monitor.Exit(p_Mutex);
    }

    public void SetArrow(Direction direction) {
        if (p_CurrentArrow == direction) { return; }
        Monitor.Enter(p_Mutex);

        p_CurrentArrow = direction;

        const Direction NS = Direction.NORTH | Direction.SOUTH;
        const Direction WE = Direction.WEST | Direction.EAST;

        #region all directions
        if (direction == Direction.ALL) {
            const float radius = 1.2f;
            const float radiusHalf = radius / 2;
            const float size = radiusHalf;

            /*4 arrows, 3 points per arrow*/
            p_Polygon = new PointF[4][];
            PointF[] top = p_Polygon[0] = new PointF[3];
            PointF[] bottom = p_Polygon[1] = new PointF[3];
            PointF[] left = p_Polygon[2] = new PointF[3];
            PointF[] right = p_Polygon[3] = new PointF[3];

            #region top
            top[0] = new PointF(-radiusHalf, -radius);
            top[1] = new PointF(radiusHalf, -radius);
            top[2] = new PointF(0, -radius - size);
            #endregion

            #region bottom
            bottom[0] = new PointF(-radiusHalf, radius);
            bottom[1] = new PointF(radiusHalf, radius);
            bottom[2] = new PointF(0, radius + size);
            #endregion

            #region left
            left[0] = new PointF(-radius, -radiusHalf);
            left[1] = new PointF(-radius, radiusHalf);
            left[2] = new PointF(-radius - size, 0);
            #endregion

            #region right
            right[0] = new PointF(radius, -radiusHalf);
            right[1] = new PointF(radius, radiusHalf);
            right[2] = new PointF(radius + size, 0);
            #endregion

            Monitor.Exit(p_Mutex);
            return;
        }
        #endregion

        /*if north/south or west/east are together, cancel them out.*/
        if ((direction & NS) == NS) {
            direction -= NS;
        }
        if ((direction & WE) == WE) {
            direction -= WE;
        }

        /*we are initializing just a normal arrow, so create 
          just one poly to be rendered*/
        p_Polygon = new PointF[1][];
        p_Polygon[0] = new PointF[3];
        PointF[] poly = p_Polygon[0];
        switch (direction) {
            #region North
            case Direction.NORTH:
                poly[1] = new PointF(-0.7f, 0.7f);
                poly[2] = new PointF(0.7f, 0.7f);
                break;
            case Direction.NORTH_WEST:
                poly[1] = new PointF(1, 0);
                poly[2] = new PointF(0, 1);
                break;
            case Direction.NORTH_EAST:
                poly[1] = new PointF(-1, 0);
                poly[2] = new PointF(0, 1);
                break;
            #endregion

            #region South
            case Direction.SOUTH:
                poly[1] = new PointF(-0.7f, -0.7f);
                poly[2] = new PointF(0.7f, -0.7f);
                break;
            case Direction.SOUTH_WEST:
                poly[1] = new PointF(0, -1);
                poly[2] = new PointF(1, 0);
                break;
            case Direction.SOUTH_EAST:
                poly[1] = new PointF(-1, 0);
                poly[2] = new PointF(0, -1);
                break;
            #endregion

            case Direction.WEST:
                poly[1] = new PointF(0.70f, -0.70f);
                poly[2] = new PointF(0.70f, 0.70f);
                break;
            case Direction.EAST:
                poly[1] = new PointF(-0.7f, -0.7f);
                poly[2] = new PointF(-0.7f, 0.7f);
                break;
        }


        //clean up
        Monitor.Exit(p_Mutex);
    }

    private void handleMouseDown(object sender, MouseEventArgs e) {
        p_CurrentButton |= e.Button;
    }
    private void handleMouseUp(object sender, MouseEventArgs e) {
        p_CurrentButton -= e.Button;
    }

    public Size Size {
        get { return p_Size; }
        set { p_Size = value; }
    }
    public MouseButtons MouseButton { get { return p_CurrentButton; } }

    public void Enable() {
        if (p_Enabled) { return; }

        //disable native cursor
        p_Game.Window.Invoke(new MethodInvoker(delegate {
            Cursor.Hide();
        }));

        p_Enabled = true;
    }
    public void Disable() {
        if (!p_Enabled) { return; }

        //enable native cursor
        p_Game.Window.Invoke(new MethodInvoker(delegate {
            Cursor.Show();
        }));

        p_Enabled = false;
    }


    public void HookEvents() {
        /*hook into cursor down events so we know what button is being pressed*/
        Game game = p_Game;
        GameWindow window = game.Window;
        window.MouseDown += handleMouseDown;
        window.MouseUp += handleMouseUp;
    }
    public void UnhookEvents() {
        Game game = p_Game;
        GameWindow window = game.Window;
        window.MouseDown -= handleMouseDown;
        window.MouseUp -= handleMouseUp;
    }
}
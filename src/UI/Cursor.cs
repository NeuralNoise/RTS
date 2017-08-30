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

public sealed class UICursor {
    private Game p_Game;
    private PointF[] p_Polygon;
    private Size p_Size;
    private Direction p_CurrentArrow;
    private bool p_Enabled;
    private MouseButtons p_CurrentButton;

    private ITexture p_CursorTexture;

    public UICursor(Game game) {
        p_Game = game;
        p_Size = new Size(20, 20);

        /*hook into cursor down events so we know what button is being pressed*/
        game.Window.MouseDown += handleMouseDown;
        game.Window.MouseUp += handleMouseUp;
    }
    
    private Bitmap cursorTestBmp;
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

        //allocate the cursor
        if (p_CursorTexture == null) {
            p_CursorTexture = context.AllocateTexture(getCursorBitmap(), "cursor");
        }
        
        //get the current mouse position relative to the window
        Point mousePosition = p_Game.PointToClient(Cursor.Position);

        /*draw arrow?*/
        if (p_CurrentArrow != Direction.NONE) {
            Matrix transform = new Matrix();
            transform.Translate(
                (float)mousePosition.X,
                (float)mousePosition.Y);
            transform.Scale((float)p_Size.Width, (float)p_Size.Height);

            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(p_Polygon);
            path.Transform(transform);

            renderer.SetBrush(Brushes.White);
            //renderer.FillPath(path);

            PointF[] p = path.PathPoints;
            Point[] pt = new Point[p.Length];
            for (int c = 0; c < p.Length; c++) {
                pt[c] = new Point(
                    (int)p[c].X,
                    (int)p[c].Y);
            }
            renderer.FillPoly(pt);
            return;
        }

        renderer.SetTexture(p_CursorTexture);
        renderer.DrawTextureUnscaled(
            mousePosition.X,
            mousePosition.Y);
        return;

        /*draw cursor natively*/
        p_Game.Window.Cursor.Draw(
            (renderer as GDIPRenderer).FrameBuffer,
            new Rectangle(
                mousePosition,
                Cursor.Current.Size));
    }

    public void SetArrow(Direction direction) {
        if (p_CurrentArrow == direction) { return; }

        p_CurrentArrow = direction;

        const Direction ALL = Direction.NORTH_WEST | Direction.SOUTH_EAST;
        const Direction NS = Direction.NORTH | Direction.SOUTH;
        const Direction WE = Direction.WEST | Direction.EAST;

        /*all directions*/
        if (direction == ALL) {
            p_CurrentArrow = Direction.NONE;
            return;
        }

        /*if north/south or west/east are together, cancel them out.*/
        if ((direction & NS) == NS) {
            direction -= NS;
        }
        if ((direction & WE) == WE) {
            direction -= WE;
        }

        /*initialize polygon (it will always be 3)*/
        p_Polygon = new PointF[3];

        switch (direction) {
            #region North
            case Direction.NORTH:
                p_Polygon[1] = new PointF(-0.7f, 0.7f);
                p_Polygon[2] = new PointF(0.7f, 0.7f);
                break;
            case Direction.NORTH_WEST:
                p_Polygon[1] = new PointF(1, 0);
                p_Polygon[2] = new PointF(0, 1);
                break;
            case Direction.NORTH_EAST:
                p_Polygon[1] = new PointF(-1, 0);
                p_Polygon[2] = new PointF(0, 1);
                break;
            #endregion

            #region South
            case Direction.SOUTH:
                p_Polygon[1] = new PointF(-0.7f, -0.7f);
                p_Polygon[2] = new PointF(0.7f, -0.7f);
                break;
            case Direction.SOUTH_WEST:
                p_Polygon[1] = new PointF(0, -1);
                p_Polygon[2] = new PointF(1, 0);
                break;
            case Direction.SOUTH_EAST:
                p_Polygon[1] = new PointF(-1, 0);
                p_Polygon[2] = new PointF(0, -1);
                break;
            #endregion

            case Direction.WEST:
                p_Polygon[1] = new PointF(0.70f, -0.70f);
                p_Polygon[2] = new PointF(0.70f, 0.70f);
                break;
            case Direction.EAST:
                p_Polygon[1] = new PointF(-0.7f, -0.7f);
                p_Polygon[2] = new PointF(-0.7f, 0.7f);
                break;
        }
    }

    private void handleMouseDown(object sender, MouseEventArgs e) {
        p_CurrentButton |= e.Button;
    }
    private void handleMouseUp(object sender, MouseEventArgs e) {
        p_CurrentButton = MouseButtons.None;
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
}
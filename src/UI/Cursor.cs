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
    private ArrowDirection p_CurrentArrow;
    private bool p_Enabled;

    public UICursor(Game game) {
        p_Game = game;
        p_Size = new Size(20, 20);

        cursorTest();
    }


    private Bitmap cursorTestBmp;
    private void cursorTest() {
        /*REMOVE*/
        cursorTestBmp = (Bitmap)Bitmap.FromFile("cursor.png");

        Bitmap clone = new Bitmap(cursorTestBmp.Width, cursorTestBmp.Height);

        int w = cursorTestBmp.Width, h = cursorTestBmp.Height;
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {
                Color pxl = cursorTestBmp.GetPixel(x, y);

                int data = pxl.ToArgb();
                data &= 0x00FFFFFF;

                if (data != 0xFF00FF) {
                    clone.SetPixel(x, y, pxl);
                }

            }
        }
        cursorTestBmp = clone;
    }

    public void Draw(IRenderContext context, IRenderer renderer) {
        if (!p_Enabled) { return; }
        
        //get the current mouse position relative to the window
        Point mousePosition = p_Game.PointToClient(Cursor.Position);

        /*draw arrow?*/
        if (p_CurrentArrow != ArrowDirection.NONE) {
            Matrix transform = new Matrix();
            transform.Translate(
                (float)mousePosition.X,
                (float)mousePosition.Y);
            transform.Scale((float)p_Size.Width, (float)p_Size.Height);

            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(p_Polygon);
            path.Transform(transform);

            renderer.SetBrush(Brushes.White);
            renderer.FillPath(path);
            return;
        }

        renderer.DrawImageUnscaled(
            cursorTestBmp,
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

    public void SetArrow(ArrowDirection direction) {
        if (p_CurrentArrow == direction) { return; }

        p_CurrentArrow = direction;
        /*initialize polygon (it will always be 3)*/
        p_Polygon = new PointF[3];

        switch (direction) {
            #region North
            case ArrowDirection.NORTH:
                p_Polygon[1] = new PointF(-0.7f, 0.7f);
                p_Polygon[2] = new PointF(0.7f, 0.7f);
                break;
            case ArrowDirection.NORTH_WEST:
                p_Polygon[1] = new PointF(1, 0);
                p_Polygon[2] = new PointF(0, 1);
                break;
            case ArrowDirection.NORTH_EAST:
                p_Polygon[1] = new PointF(-1, 0);
                p_Polygon[2] = new PointF(0, 1);
                break;
            #endregion

            #region South
            case ArrowDirection.SOUTH:
                p_Polygon[1] = new PointF(-0.7f, -0.7f);
                p_Polygon[2] = new PointF(0.7f, -0.7f);
                break;
            case ArrowDirection.SOUTH_WEST:
                p_Polygon[1] = new PointF(0, -1);
                p_Polygon[2] = new PointF(1, 0);
                break;
            case ArrowDirection.SOUTH_EAST:
                p_Polygon[1] = new PointF(-1, 0);
                p_Polygon[2] = new PointF(0, -1);
                break;
            #endregion

            case ArrowDirection.WEST:
                p_Polygon[1] = new PointF(0.70f, -0.70f);
                p_Polygon[2] = new PointF(0.70f, 0.70f);
                break;
            case ArrowDirection.EAST:
                p_Polygon[1] = new PointF(-0.7f, -0.7f);
                p_Polygon[2] = new PointF(-0.7f, 0.7f);
                break;
        }
    }

    public Size Size {
        get { return p_Size; }
        set { p_Size = value; }
    }

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

    [Flags]
    public enum ArrowDirection { 
        NONE = 0,
     
        NORTH = 0x01,
        SOUTH = 0x02,

        WEST =  0x04,
        EAST =  0X08,

        NORTH_WEST = NORTH | WEST,
        NORTH_EAST = NORTH | EAST,

        SOUTH_WEST = SOUTH | WEST,
        SOUTH_EAST = SOUTH | EAST
    }
}
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
using System.Threading;


public class UILabel : UIControl {
    private string p_Text;
    private Font p_Font;
    private IRenderer p_SizeRenderer;
    private Brush p_ForeBrush;
    private TextAlign p_Align;
    private Brush p_ShadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black));
    private object p_Mutex = new object();

    private string[] p_Lines;

    public UILabel(Game game) : this("", new Font("Arial", 12, FontStyle.Regular), game) { }
    public UILabel(string text, Font font, Game game) : base(game) {
        p_SizeRenderer = RenderFactory.CreateRenderer();
        p_ForeBrush = Brushes.Black;
        p_Text = text;
        Font = font;
    }

    public Font Font {
        get { return p_Font; }
        set {
            p_Font = value;
            p_SizeRenderer.SetFont(value);
            invalidate();
        }
    }
    public string Text {
        get { return p_Text; }
        set {
            p_Text = value;
            invalidate();
        }
    }
    public Brush ForeBrush {
        get { return p_ForeBrush; }
        set { p_ForeBrush = value; }
    }
    public TextAlign TextAlignment {
        get { return p_Align; }
        set {
            p_Align = value;
        }
    }

    private void invalidate() {
        Size = p_SizeRenderer.MeasureString(p_Text);

        //update text information
        lock (p_Mutex) {
            p_Lines = p_Text.Split('\n');
        }
    }

    public override void Update() { }
    public override void Draw(IRenderContext context, IRenderer renderer) {
        Point rLocation = RenderLocation;
        int rX = rLocation.X;
        int rY = rLocation.Y;

        int shadowSize = 3;

        renderer.SetFont(p_Font);
        renderer.SetBrush(p_ForeBrush);

        /*left align is just render as normal*/
        if (p_Align == TextAlign.Left) {
            drawShadow(renderer, p_Text, rX, rY, shadowSize);
            renderer.DrawString(
                p_Text,
                rX, rY);
            return;
        }

        //clone the current lines so any concurrent thread
        //to change it won't be blocked until we render.
        string[] lines = null;
        lock (p_Mutex) {
            lines = (string[])p_Lines.Clone();
        }

        /*grab each line of text*/
        int lineLength = lines.Length;
        for (int c = 0; c < lineLength; c++) {
            string line = lines[c];

            //get the render size of the text
            Size lineSize = p_SizeRenderer.MeasureString(line);

            /*calculate x offset based off alignment type*/
            int xOffset = 0;
            switch (p_Align) { 
                case TextAlign.Center:
                    xOffset = (Width / 2) - (lineSize.Width / 2);
                    break;
                case TextAlign.Right:
                    xOffset = Width - lineSize.Width;
                    break;
            }
            
            //draw shadow
            drawShadow(
                renderer,
                line,
                rX + xOffset,
                rY,
                shadowSize);

            //draw
            renderer.SetBrush(p_ForeBrush);
            renderer.DrawString(
                line,
                rX + xOffset,
                rY);
            rY += lineSize.Height;
        }
    }

    private void drawShadow(IRenderer renderer, string text, int x, int y, int shadowSize) {
        //draw shadow
        int shadowWidth = 2;
        renderer.SetBrush(p_ShadowBrush);
        for (int c = shadowWidth; c != -1; c--) {
            renderer.DrawString(
                text,
                x + c,
                y + c);
        }
    }
}
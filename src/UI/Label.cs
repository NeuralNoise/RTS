using System;
using System.Drawing;


public class UILabel : UIControl {
    private string p_Text;
    private Font p_Font;
    private IRenderer p_SizeRenderer;
    private Brush p_ForeBrush;
    private TextAlign p_Align;

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
        p_Lines = p_Text.Split('\n');
    }

    public override void Update() { }
    public override void Draw(IRenderContext context, IRenderer renderer) {
        Point rLocation = RenderLocation;
        int rX = rLocation.X;
        int rY = rLocation.Y;

        renderer.SetFont(p_Font);
        renderer.SetBrush(p_ForeBrush);

        /*left align is just render as normal*/
        if (p_Align == TextAlign.Left) {
            renderer.DrawString(
                p_Text,
                rX, rY);
            return;
        }
        

        /*grab each line of text*/
        int lineLength = p_Lines.Length;
        for (int c = 0; c < lineLength; c++) {
            string line = p_Lines[c];

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

            //draw
            renderer.DrawString(
                line,
                rX + xOffset,
                rY);
            rY += lineSize.Height;
        }
        


    }
}
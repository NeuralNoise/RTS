using System;
using System.Drawing;


public class UILabel : UIControl {
    private string p_Text;
    private Font p_Font;
    private IRenderer p_SizeRenderer;
    private Brush p_ForeBrush;

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

    private void invalidate() {
        Size = p_SizeRenderer.MeasureString(p_Text);
    }

    public override void Update() { }
    public override void Draw(IRenderContext context, IRenderer renderer) {
        Point rLocation = RenderLocation;


        renderer.SetFont(p_Font);
        renderer.SetBrush(p_ForeBrush);
        renderer.DrawString(
            p_Text,
            rLocation.X,
            rLocation.Y);
    }
}
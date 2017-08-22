using System;
using System.Drawing;
using System.Windows.Forms;

public interface IUI {
    void Update();

    void Draw(IRenderContext context, IRenderer renderer);

    void OnKeyDown(Game game, KeyEventArgs e);
    void OnKeyUp(Game game, KeyEventArgs e);

    void OnMouseClick(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseMove(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseDown(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseUp(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseScroll(Game game, Point mousePosition, MouseEventArgs e);

    void Enable();
    void Disable();

    bool Enabled { get; }
}
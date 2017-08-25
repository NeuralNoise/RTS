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
using System.Windows.Forms;
using System.Collections.Generic;

public sealed class UIClickDrag : IUI {
    private bool p_Enabled;
    private Point p_StartPosition;

    private int p_Width, p_Height;

    private static Pen p_Border = new Pen(Brushes.White);
    private static Brush p_Fill = new SolidBrush(
        Color.FromArgb(130, Color.White));
    
    /*mouse events does this.*/
    public void Update() { }
    public void OnMouseClick(Game game, Point mousePosition, MouseEventArgs e) { }
    public void OnMouseMove(Game game, Point mousePosition, MouseEventArgs e) { }
    public void OnMouseDown(Game game, Point mousePosition, MouseEventArgs e) { 
        //arrow keys pressed?
        if (game.IsArrowKeyPressed) {
            return;
        }

        //must be a mouse left
        if (e.Button != MouseButtons.Left) {
            if (p_Enabled) {
                Disable();
                if (BlocksSelected != null) {
                    BlocksSelected(null);
                }
            }
            return;
        }

        //is this the first mouse down since mouseup/start?
        if (!p_Enabled) {
            p_StartPosition = mousePosition;            
            p_Enabled = true;
            p_Width = p_Height = 0;
            
            //fire selected event for no blocks selected
            if (BlocksSelected != null) {
                BlocksSelected(null);
            }
            
            return;
        }

        //set size relative to the current 
        //mouse position vs start position
        p_Width = mousePosition.X - p_StartPosition.X;
        p_Height = mousePosition.Y - p_StartPosition.Y;

    }
    public unsafe void OnMouseUp(Game game, Point mousePosition, MouseEventArgs e) {
        if (!p_Enabled) { return; }
        Disable();

        //fire selected event for all selected blocks
        if (BlocksSelected == null) { return; }
        Rectangle region = new Rectangle(
            p_StartPosition.X,
            p_StartPosition.Y,
            p_Width,
            p_Height);

        LinkedList<VisibleBlock> blocks = game.Map.GetBlocksInRegion(
            game.Window.Context,
            region);
        BlocksSelected(blocks);        
    }
    public void OnMouseScroll(Game game, Point mousePosition, MouseEventArgs e) { }

    public void OnKeyDown(Game game, KeyEventArgs e) { }
    public void OnKeyUp(Game game, KeyEventArgs e) { }

    public void Draw(IRenderContext context, IRenderer renderer) {
        //anything to draw?
        if (!p_Enabled || 
            p_Width == 0 || p_Height == 0) { return; }


        int drawX = p_StartPosition.X;
        int drawY = p_StartPosition.Y;
        int drawW = p_Width;
        int drawH = p_Height;

        //negative size? if so we swap so we can render behind the start
        if (drawW < 0) {
            drawW = -drawW;
            drawX -= drawW;
        }
        if (drawH < 0) {
            drawH = -drawH;
            drawY -= drawH;
        }

        //draw
        renderer.SetPen(p_Border);
        renderer.DrawQuad(
            drawX, drawY,
            drawW, drawH);

        renderer.SetBrush(p_Fill);
        renderer.FillQuad(
            drawX, drawY,
            drawW, drawH);
    }

    public void Enable() {
        p_Enabled = true;
        p_StartPosition = Point.Empty;
        p_Width = 0;
        p_Height = 0;
    }
    public void Disable() {
        p_Enabled = false;
    }

    public bool Enabled { get { return p_Enabled; } }
    public bool Visible {
        get { return true; }
        set {
            throw new NotSupportedException();
        }
    }

    public event OnBlocksSelectedEventHandler BlocksSelected;
    
    public delegate void OnBlocksSelectedEventHandler(LinkedList<VisibleBlock> blocks);
}
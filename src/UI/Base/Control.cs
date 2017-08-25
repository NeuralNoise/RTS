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

public abstract class UIControl : IUI {
    private bool p_Enabled;
    private bool p_Focus;
    private int p_X, p_Y;
    private int p_W, p_H;

    private Game p_Game;

    private UIControl p_Parent;
    private List<UIControl> p_Children = new List<UIControl>();

    public UIControl(Game game) {
        p_Game = game;
        p_Enabled = true;

        //default visibility to true
        Visible = true;
    }

    public void Enable() { p_Enabled = true; }
    public void Disable() { p_Enabled = false; }
    public bool Visible { get; set; }
    public bool Enabled { get { return p_Enabled; } }
    public Game Game { get { return p_Game; } }

    public Size Size {
        get { return new Size(p_W, p_H); }
        set {
            p_W = value.Width;
            p_H = value.Height;
        }
    }
    public Point Location {
        get { return new Point(p_X, p_Y); }
        set {
            p_X = value.X;
            p_Y = value.Y;
        }
    }
    public Point RenderLocation {
        get {
            if (p_Parent == null) {
                return Location;
            }
            
            //use recursion to get this answer
            return new Point(
                p_X + p_Parent.RenderLocation.X,
                p_Y + p_Parent.RenderLocation.Y);
        }
    }

    public int X { 
        get { return p_X; }
        set { p_X = value; }
    }
    public int Y {
        get { return p_Y; }
        set { p_Y = value; }
    }
    public int Width {
        get { return p_W; }
        set { p_W = value; }
    }
    public int Height {
        get { return p_H; }
        set { p_H = value; }
    }

    public bool Focused { get { return p_Focus; } }

    public void Focus() { 
        //unfocus all siblings first
        UIControl[] sibs = Siblings;
        int l = sibs.Length;
        for (int c = 0; c < l; c++) {
            sibs[c].RemoveFocus();
        }

        //toggle focus
        p_Focus = true;

        //hijack events
        if (p_Parent == null) {
            Game.HijackEvents(this);
        }

    }
    public void RemoveFocus()  {
        p_Focus = false;
        if (p_Parent == null) {
            Game.DetatchFromEventsHijack(this);
        }
    }

    public UIControl AddControl(UIControl control) {
        //adding the parent?
        if (control == p_Parent) {
            throw new Exception("Cannot add the controls parent as a child.");
        }

        //already a member?
        if (p_Children.Contains(control)) { return control; }

        //add and swap parents
        p_Children.Add(control);
        if (control.p_Parent != null) {
            control.p_Parent.p_Children.Remove(control);
        }
        control.p_Parent = this;   
     
        //control have focus?
        if (control.p_Focus) {
            control.Focus();
        }
        return control;
    }
    public bool RemoveControl(UIControl control) { 
        //exist?
        if (!p_Children.Contains(control)) { return false; }
        
        //remove
        bool result = p_Children.Remove(control);
        if (result) { control.p_Parent = null; }
        return result;
    }
    public bool Remove() {
        if (p_Parent == null) { return false; }
        return p_Parent.RemoveControl(this);
    }

    public UIControl[] Siblings {
        get { 
            //do we have a parent?
            if (p_Parent == null) {
                return new UIControl[0];
            }

            //copy the parents children but remove this entity
            //thus returning all siblings.
            List<UIControl> children = p_Parent.p_Children;
            int l = children.Count;
            int ai = 0;
            UIControl[] buffer = new UIControl[l - 1];
            for (int c = 0; c < children.Count; c++) {
                UIControl current = children[c];
                if (current == this) { continue; }
                buffer[ai++] = current;
            }

            return buffer;
        }
    }
    public List<UIControl> Children { get { return p_Children; } }
    public UIControl Parent { get { return p_Parent; } }

    public bool DoesCollide(Game game, Point mousePosition) {
        Point renderLocation = RenderLocation;
        return 
            mousePosition.X >= renderLocation.X &&
            mousePosition.Y >= renderLocation.Y &&
            mousePosition.X <= renderLocation.X + p_W &&
            mousePosition.Y <= renderLocation.Y + p_H;
    }

    #region Event handling

    public void OnMouseClick(Game g, Point mousePosition, MouseEventArgs e) {
        sendMessage(g, msgType.MOUSE_CLICK, new mouseArgs { 
            e = e,
            mousePosition = mousePosition
        });
    }
    public void OnMouseMove(Game g, Point mousePosition, MouseEventArgs e) {
        sendMessage(g, msgType.MOUSE_MOVE, new mouseArgs { 
            e = e,
            mousePosition = mousePosition
        });
    }
    public void OnMouseDown(Game g, Point mousePosition, MouseEventArgs e) {
        sendMessage(g, msgType.MOUSE_DOWN, new mouseArgs {
            e = e,
            mousePosition = mousePosition
        });
    }
    public void OnMouseUp(Game g, Point mousePosition, MouseEventArgs e) {
        sendMessage(g, msgType.MOUSE_UP, new mouseArgs {
            e = e,
            mousePosition = mousePosition
        });
    }
    public void OnMouseScroll(Game g, Point mousePosition, MouseEventArgs e) {
        sendMessage(g, msgType.MOUSE_SCROLL, new mouseArgs {
            e = e,
            mousePosition = mousePosition
        });
    }
    public void OnKeyDown(Game g, KeyEventArgs e) {
        sendMessage(g, msgType.KEY_DOWN, e);
    }
    public void OnKeyUp(Game g, KeyEventArgs e) {
        sendMessage(g, msgType.KEY_UP, e);
    }
    
    private void sendMessage(Game g, msgType type, object args) {
        if (!p_Enabled) { return; }

        //no event?
        if (type == msgType.NONE) { return; }

        //key event?
        bool isKeyEvent = type >= msgType.KEY_DOWN;

        //does this event need focus?
        bool requireFocus = isKeyEvent || type >= msgType.MOUSE_MOVE;
        if (requireFocus && !p_Focus) { return; }

        //handle key
        if (isKeyEvent) {
            fireToFocused(this, g, type, args);
            return;
        }

        //is the mouse within the region of this control?
        Point mousePosition = g.Window.PointToClient(Cursor.Position);
        bool mouseHit = DoesCollide(g, mousePosition);

        //mouse click/down
        if (type == msgType.MOUSE_DOWN || type == msgType.MOUSE_CLICK) {
            if (mouseHit) {
                if (!p_Focus) { Focus(); }

                fireEvent(g, type, args);

                //send message to all children
                int l = p_Children.Count;
                for (int c = 0; c < l; c++) {
                    p_Children[c].fireEvent(
                        g,
                        type,
                        args
                    );
                }
            }
            else if(p_Focus){
                RemoveFocus();
            }
        }
    }

    private void fireToFocused(UIControl control, Game g, msgType type, object args) {
        control.fireEvent(g, type, args);
         
        //fire to every focused child in the control and recursively fire.
        int l = control.p_Children.Count;
        for (int c = 0; c < l; c++) {
            UIControl child = control.p_Children[c];
            if (child.p_Focus) {
                fireToFocused(
                    child,
                    g,
                    type,
                    args);
            }
        }

    }
    private void fireEvent(Game g, msgType type, object args) {
        mouseArgs mArgs = (args is mouseArgs ? args as mouseArgs : null);

        Console.WriteLine("FIRE " + type);

        switch (type) {
            #region KeyUp
            case msgType.KEY_UP:
                if (KeyUp != null) {
                    KeyUp(g, args as KeyEventArgs);
                }
                break;
            #endregion
            #region KeyDown
            case msgType.KEY_DOWN:
                if (KeyDown != null) {
                    KeyDown(g, args as KeyEventArgs);
                }
                break;
            #endregion
            #region MouseMove
            case msgType.MOUSE_MOVE:
                if (MouseMove != null) {
                    MouseMove(g, mArgs.mousePosition, mArgs.e);
                }
                break;
            #endregion
            #region MouseDown
            case msgType.MOUSE_DOWN:
                if (MouseDown != null) {
                    MouseDown(g, mArgs.mousePosition, mArgs.e);
                }
                break;
            #endregion
            #region MouseUp
            case msgType.MOUSE_UP:
                if (MouseUp != null) {
                    MouseUp(g, mArgs.mousePosition, mArgs.e);
                }
                break;
            #endregion
            #region MouseMove
            case msgType.MOUSE_SCROLL:
                if (MouseScroll != null) {
                    MouseScroll(g, mArgs.mousePosition, mArgs.e);
                }
                break;
            #endregion
        }


    }

    public event OnMouseEventHandler MouseMove;
    public event OnMouseEventHandler MouseUp;
    public event OnMouseEventHandler MouseDown;
    public event OnMouseEventHandler MouseScroll;
    public event OnKeyEventHandler KeyUp;
    public event OnKeyEventHandler KeyDown;
    
    public delegate void OnMouseEventHandler(Game game, Point mousePosition, MouseEventArgs args);
    public delegate void OnKeyEventHandler(Game game, KeyEventArgs e);

    private enum msgType { 
        NONE =          0,
        MOUSE_CLICK =   1,
        MOUSE_DOWN =    2,
        MOUSE_MOVE =    3,
        MOUSE_UP =      4,
        MOUSE_SCROLL =  5,
        KEY_DOWN =      6,
        KEY_UP =        7
    }
    private class mouseArgs {
        public Point mousePosition;
        public MouseEventArgs e;
    }
    #endregion

    /*abstract functions for derivers*/
    public abstract void Update();
    public abstract void Draw(IRenderContext context, IRenderer renderer);
}
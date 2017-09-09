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

public sealed class Camera : IAnimatePosition {
    public const int BLOCKSIZE_MIN = 16;
    public const int BLOCKSIZE_MAX = 48;

    private int p_BlockSize;
    
    private int p_X, p_Y;

    private bool p_AllowMargin;
    private int p_MarginWidth, p_MarginHeight;

    /*1 pixel in total render size*/
    private float p_WidthScalar, p_HeightScalar;

    private Game p_Game;

    public Camera(Game game) {
        p_Game = game;
    }

    public bool Move(int dX, int dY) {
        //apply delta
        int oldX = p_X;
        int oldY = p_Y;
        p_X += dX;
        p_Y += dY;

        //
        int success = 2;

        //check margin
        if (p_AllowMargin) {
            Map map = p_Game.Map;

            //translate margin to render x/y
            int marginW = p_MarginWidth * p_BlockSize;
            int marginH = p_MarginHeight * p_BlockSize;

            //check mins
            if (p_X < -marginW) { p_X = -marginW; }
            if (p_Y < -marginH) { p_Y = -marginH; }

            //check max
            Size blocksPerFrame = BlocksInFrame;

            int maxX = marginW + ((map.Width - blocksPerFrame.Width) * p_BlockSize);
            int maxY = marginH + ((map.Height - blocksPerFrame.Height) * p_BlockSize);
            if (p_X > maxX) {
                success--;
                p_X = maxX; 
            }
            if (p_Y > maxY) {
                success--;
                p_Y = maxY; 
            }
        }

        //fire changed
        if (CameraChanged != null) {
            CameraChanged(this);
        }

        //only return true if X or Y were changed.
        bool ret = success != 0;
        if (!ret) {
            p_X = oldX;
            p_Y = oldY;
        }
        return ret;
    }
    public void Scale(int d) {
        //deturmine the block the camera is currently at.
        //so we can later move the camera back to this position.
        //note: the position WILL change since the resize will
        //essentially push all blocks forwards or backwards.
        int x = (int)(p_X * 1.0f / p_BlockSize);
        int y = (int)(p_Y * 1.0f / p_BlockSize);

        p_BlockSize += d;

        //check
        if (p_BlockSize <= BLOCKSIZE_MIN) {
            p_BlockSize = BLOCKSIZE_MIN;
        }
        if (p_BlockSize >= BLOCKSIZE_MAX) {
            p_BlockSize = BLOCKSIZE_MAX;
        }

        /*update width/height scalars*/
        Map map = p_Game.Map;
        int totalRenderWidth = map.Width * p_BlockSize;
        int totalRenderHeight = map.Height * p_BlockSize;
        p_WidthScalar = 1.0f / totalRenderWidth;
        p_HeightScalar = 1.0f / totalRenderHeight;

        //move to the old position since the 
        //camera would of moved due to the resize.
        MoveAbs(
            x * p_BlockSize,
            y * p_BlockSize);

        //fire changed
        if (CameraChanged != null) {
            CameraChanged(this);
        }
    }

    public void SetMargin(int width, int height) {
        p_AllowMargin = true;
        p_MarginWidth = width;
        p_MarginHeight = height;
        Move(0, 0);
    }

    public bool MoveAbs(int x, int y) {
        return Move(
            -p_X + x,
            -p_Y + y);
    }
    public void ZoomAbs(int size) {
        Scale(-p_BlockSize + size);
    }
    public void ForceZoomAbs(int size) {
        /*as the name suggests, we don't bound check the size.*/
        p_BlockSize = size;

        if (CameraChanged != null) {
            CameraChanged(this);
        }
    }

    public void MoveCenter() {
        //move to the estimated centre block
        Map map = p_Game.Map;
        MoveCenter(
            map.Width / 2,
            map.Height / 2);
    }
    public void MoveCenter(int blockX, int blockY) {        
        //get how many blocks the camera can fit in one frame
        Size blocksInFrame = BlocksInFrame;
        int width = blocksInFrame.Width;
        int height = blocksInFrame.Height;
        width *= p_BlockSize;
        height *= p_BlockSize;

        //convert block location to render location
        blockX *= p_BlockSize;
        blockY *= p_BlockSize;

        //move the camera so it has the render x/y in the center
        //of the frame
        MoveAbs(
            blockX - (width / 2),
            blockY - (height / 2));
        

    }

    public int X {
        get { return p_X; }
        set {
            MoveAbs(value, p_Y);
        }
    }
    public int Y {
        get { return p_Y; }
        set {
            MoveAbs(p_X, value);
        }
    }

    public int BlockSize {
        get { return p_BlockSize; }
        set {
            ZoomAbs(value);
        }
    }

    public float BlockSizeScalar {
        get {
            return (p_BlockSize * 1.0f / BLOCKSIZE_MAX);
        }
    }

    public bool EnableMargin {
        get { return p_AllowMargin; }
        set { p_AllowMargin = value; }
    }

    public Size BlocksInFrame {
        get {
            IRenderContext ctx = p_Game.Window.Context;

            return new Size(
                (int)Math.Ceiling(ctx.Width * 1.0f / p_BlockSize),
                (int)Math.Ceiling(ctx.Height * 1.0f / p_BlockSize));
        }
    }
    
    public event OnCameraChangeEventHandler CameraChanged;

    public delegate void OnCameraChangeEventHandler(Camera camera);
}
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

public sealed class Camera {
    private const int BLOCKSIZE_W_MIN = 16;
    private const int BLOCKSIZE_H_MIN = 16;
    private const int BLOCKSIZE_W_MAX = 48;
    private const int BLOCKSIZE_H_MAX = 48;

    private int p_BlockSizeW = BLOCKSIZE_W_MAX, 
                p_BlockSizeH = BLOCKSIZE_H_MAX;
    private int p_X, p_Y;

    private bool p_AllowMargin;
    private int p_MarginWidth, p_MarginHeight;

    private Game p_Game;

    public Camera(Game game) {
        p_Game = game;
    }

    public void Move(int dX, int dY) {
        p_X += dX;
        p_Y += dY;

        //check margin
        if (p_AllowMargin) {
            Map map = p_Game.Map;

            //translate margin to render x/y
            int marginW = p_MarginWidth * p_BlockSizeW;
            int marginH = p_MarginHeight * p_BlockSizeH;

            //check mins
            if (p_X < -marginW) { p_X = -marginW; }
            if (p_Y < -marginH) { p_Y = -marginH; }

            //check max
            Size blocksPerFrame = BlocksInFrame;

            int maxX = marginW + ((map.Width - blocksPerFrame.Width) * p_BlockSizeW);
            int maxY = marginH + ((map.Height - blocksPerFrame.Height) * p_BlockSizeH);
            if (p_X > maxX) { p_X = maxX; }
            if (p_Y > maxY) { p_Y = maxY; }
        }

        //fire changed
        if (CameraChanged != null) {
            CameraChanged(this);
        }
    }
    public void Zoom(int dX, int dY) {
        //deturmine the block the camera is currently at.
        //so we can later move the camera back to this position.
        //note: the position WILL change since the resize will
        //essentially push all blocks forwards or backwards.
        int x = (int)(p_X * 1.0f / p_BlockSizeW);
        int y = (int)(p_Y * 1.0f / p_BlockSizeH);

        p_BlockSizeW += dX;
        p_BlockSizeH += dY;

        //check
        if (p_BlockSizeW <= BLOCKSIZE_W_MIN) {
            p_BlockSizeW = BLOCKSIZE_W_MIN;
        }
        if (p_BlockSizeW >= BLOCKSIZE_W_MAX) {
            p_BlockSizeW = BLOCKSIZE_W_MAX;
        }

        if (p_BlockSizeH <= BLOCKSIZE_H_MIN) {
            p_BlockSizeH = BLOCKSIZE_H_MIN;
        }
        if (p_BlockSizeH >= BLOCKSIZE_H_MAX) {
            p_BlockSizeH = BLOCKSIZE_H_MAX;
        }

        //move to the old position since the 
        //camera would of moved due to the resize.
        MoveAbs(
            x * p_BlockSizeW,
            y * p_BlockSizeH);

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

    public void MoveAbs(int x, int y) {
        Move(
            -p_X + x,
            -p_Y + y);
    }
    public void ZoomAbs(int x, int y) {
        Zoom(
            -p_BlockSizeW + x,
            -p_BlockSizeH + y);
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
        width *= p_BlockSizeW;
        height *= p_BlockSizeH;

        //convert block location to render location
        blockX *= p_BlockSizeW;
        blockY *= p_BlockSizeH;

        //move the camera so it has the render x/y in the center
        //of the frame
        MoveAbs(
            blockX - (width / 2),
            blockY - (height / 2));
        

    }

    public int X { get { return p_X; } }
    public int Y { get { return p_Y; } }
    public int BlockWidth { get { return p_BlockSizeW; } }
    public int BlockHeight { get { return p_BlockSizeH; } }

    public bool EnableMargin {
        get { return p_AllowMargin; }
        set { p_AllowMargin = value; }
    }

    public Size BlocksInFrame {
        get {
            IRenderContext ctx = p_Game.Window.Context;

            return new Size(
                (int)Math.Ceiling(ctx.Width * 1.0f / p_BlockSizeW),
                (int)Math.Ceiling(ctx.Height * 1.0f / p_BlockSizeH));
        }
    }
    
    public event OnCameraChangeEventHandler CameraChanged;

    public delegate void OnCameraChangeEventHandler(Camera camera);
}
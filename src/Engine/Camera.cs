/* 
 * This file is part of the RTS distribution (https://github.com/tomwilsoncoder/RTS)
 * 
 * This program is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
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
    private Game p_Game;

    public Camera(Game game) {
        p_Game = game;

        
    }

    public void Move(int dX, int dY) {
        p_X += dX;
        p_Y += dY;

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
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
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public unsafe class Fog {
    private bool* p_FogMatrix;
    private bool* p_LOSMatrix;
    private object p_Mutex = new object();

    private Map p_Map;
    private int p_Width;
    private int p_Height;
    private int p_BlocksRevealed;
    private bool p_RecalcRevealed;
    private LinkedList<LineOfSight> p_LOS;

    public Fog(Map map) {
        p_Map = map;

        //initialize the matrix
        p_Width = map.Width;
        p_Height = map.Height;
        Clear();
    }

    public LinkedList<VisibleBlock> Filter(LinkedList<VisibleBlock> blocks) {
        Lock();

        LinkedList<VisibleBlock> buffer = new LinkedList<VisibleBlock>();
        foreach (VisibleBlock b in blocks) {

            if (*(p_FogMatrix + (b.BlockY * p_Width) + b.BlockX)) {
                buffer.AddLast(b);
            }

        }

        Unlock();
        return buffer;

    }

    public void SetPixel(int x, int y, bool hasFog) {
        Lock();
        *(p_FogMatrix + (y * p_Width) + x) = !hasFog;
        Unlock();
    }

    public void FillCircle(int centerX, int centerY, int radius) {
        Lock();
        fillCircle(centerX, centerY, radius, p_FogMatrix);
        p_RecalcRevealed = true;
        Unlock();
    }
    private void fillCircle(int centerX, int centerY, int radius, bool* matrix) {
        /*
            Thanks to Zevan Rosser @ http://actionsnippet.com/?p=496 (17th August 2017)   
            Version of the Bresenham algorithm to fill an ellipse pixel by pixel.
            Code ported from ActionScript and modified
        */
        int xOffset, yOffset, balance;
        xOffset = 0;
        yOffset = radius;
        balance = -radius;
        
        while (xOffset <= yOffset) {
            int p0 = centerX - xOffset;
            int p1 = centerX - yOffset;

            int w0 = xOffset << 1;
            int w1 = yOffset << 1;

            for (int c = p0; c < p0 + w0; c++) {
                trySet(c, centerY + yOffset, true, matrix);
                trySet(c, centerY - yOffset, true, matrix);
            }
            for (int c = p1; c < p1 + w1; c++) {
                trySet(c, centerY + xOffset, true, matrix);
                trySet(c, centerY - xOffset, true, matrix);
            }

            xOffset++;
            balance += xOffset << 1;

            if (balance >= 0) {
                balance -= yOffset << 1;
                yOffset--;
            }
        }
    }

    public void Lock() {
        Monitor.Enter(p_Mutex);
    }
    public void Unlock() {
        Monitor.Exit(p_Mutex);
    }

    public void AddLOS(LineOfSight s) {
        lock (p_Mutex) {
            p_LOS.AddLast(s);
        }
    }
    public bool HasLOS(int x, int y) {
        lock (p_Mutex) {
            return *(p_LOSMatrix + (y * p_Map.Width) + x);
        }
    }
    public void UpdateLOS() {
        Lock();

        //clear current LOS matrix
        bool* ptr = p_LOSMatrix;
        bool* ptrEnd = ptr + (p_Width * p_Height);
        while (ptr != ptrEnd) {
            *(ptr++) = false;
        }

        //draw every line of site to the LOS matrix
        foreach (LineOfSight s in p_LOS) {
            fillCircle(
                s.CenterX,
                s.CenterY,
                s.Radius,
                p_LOSMatrix);
            fillCircle(
                s.CenterX,
                s.CenterY,
                s.Radius,
                p_FogMatrix);
        }

        Unlock();
        p_RecalcRevealed = true;
    }

    public int BlocksRevealed {
        get {
            if (!p_RecalcRevealed) { return p_BlocksRevealed; }

            Lock();
            bool* ptr = p_FogMatrix;
            bool* ptrEnd = ptr + (p_Width * p_Height);

            int buffer = 0;
            while (ptr != ptrEnd) {
                if (*(ptr++)) {
                    buffer++;
                }
            }

            p_BlocksRevealed = buffer;
            p_RecalcRevealed = false;
            Unlock();
            return buffer;
        }
    }

    public void CopyFrom(Fog source) {
        //verify
        if (source.p_Width != p_Width ||
           source.p_Height != p_Height) {
               throw new Exception("Source must have a matching size with the destination.");
        }


        //copy
        bool* ptr = p_FogMatrix;
        bool* sourcePtr = source.p_FogMatrix;
        bool* ptrEnd = p_FogMatrix + (p_Width * p_Height);
        while (ptr != ptrEnd) {
            *(ptr++) = *(sourcePtr++);
        }

    }

    public void Clear() {
        //recreate LOS
        p_LOS = new LinkedList<LineOfSight>();
        if (p_LOSMatrix != (bool*)0) {
            Marshal.FreeHGlobal((IntPtr)p_LOSMatrix);
        }
        p_LOSMatrix = (bool*)Marshal.AllocHGlobal(
            p_Width * p_Height);


        //reallocate the fog matrix
        if (p_FogMatrix != (bool*)0) {
            Marshal.FreeHGlobal((IntPtr)p_FogMatrix);
        }
        p_FogMatrix = (bool*)Marshal.AllocHGlobal(
            p_Width * p_Height);
    
        //force all bools to false
        bool* ptr = p_FogMatrix;
        bool* losPtr = p_LOSMatrix;
        bool* ptrEnd = p_FogMatrix + (p_Width * p_Height);
        while (ptr != ptrEnd) {
            *(ptr++) = false;
            *(losPtr++) = false;
        }
    }

    public bool* GetLOSMatrix() {
        //we don't bother cloning.
        return p_LOSMatrix;
    }

    private void trySet(int x, int y, bool value, bool* matrix) {
        if (x < 0 || y < 0) { return; }
        if (x >= p_Width || y >= p_Height) { return; }

        *(matrix + (y * p_Width) + x) = value;
    }
}
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
using System.Runtime.InteropServices;

/*
    To be tested for performance... 
*/
public unsafe sealed class PtrQueue : IDisposable {
    private void** p_Base;
    private void** p_BaseEnd;

    private void** p_CurrentStart;
    private void** p_CurrentEnd;

    public PtrQueue(int capacity) {
        int size = capacity * sizeof(void*);
        p_Base = (void**)Marshal.AllocHGlobal(size);
        p_BaseEnd = p_Base + capacity;

        p_CurrentStart = p_Base;
        p_CurrentEnd = p_Base;
    }

    public void Push(void* ptr) {
        *(p_CurrentEnd++) = ptr;

        if (p_CurrentEnd >= p_BaseEnd)
        {
            p_CurrentEnd = p_Base;
        }

    }
    public void* Pop() {
        void* node = *(p_CurrentStart++);

        if (p_CurrentStart >= p_BaseEnd) {
            p_CurrentStart = p_Base;
        }

        return node;
    }

    public void* Peak() {
        return *(p_CurrentStart);
    }

    public bool Empty {
        get {
            return p_CurrentStart == p_Base;
        }
    }

    public int Length {
        get {
            return (int)(p_CurrentStart - p_Base);
        }
    }

    public void Clear() {
        p_CurrentStart = p_CurrentEnd = p_Base;
    }

    public void Dispose() {
        if (p_Base == (void*)0) { return; }

        Marshal.FreeHGlobal((IntPtr)p_Base);
        p_Base = 
        p_BaseEnd = 
        p_CurrentStart = 
        p_CurrentEnd =
                    (void**)0;
    }
    ~PtrQueue() {
        Dispose();
    }
}
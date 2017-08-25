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

public static partial class Pathfinder {
    private unsafe class ASNodeQueue {
        private ASNode** p_Base;
        private ASNode** p_BaseEnd;

        private ASNode** p_CurrentStart;
        private ASNode** p_CurrentEnd;
        
        public ASNodeQueue(int s) {
            int size = s * sizeof(ASNode*);
            p_Base = (ASNode**)Marshal.AllocHGlobal(size);
            p_BaseEnd = p_Base + s;

            p_CurrentStart = p_Base;
            p_CurrentEnd = p_Base;
        }

        public void Push(ASNode* ptr) {
            *(p_CurrentEnd++) = ptr;

            if (p_CurrentEnd >= p_BaseEnd) {
                p_CurrentEnd = p_Base;
            }

        }
        public ASNode* Pop() {
            ASNode* node = *(p_CurrentStart++);

            if (p_CurrentStart >= p_BaseEnd) {
                p_CurrentStart = p_Base;
            }

            return node;
        }

        public void Clear() {
            p_CurrentStart = p_CurrentEnd = p_Base;
        }
    }

}
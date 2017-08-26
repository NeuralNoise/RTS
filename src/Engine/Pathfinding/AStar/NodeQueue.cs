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

        /*
            We trust this call and don't check that the list is not
            empty and does contain at least 2 entities (it's faster that way)
        */
        public void Swap() {
            ASNode* first = *(p_CurrentStart - 1);
            ASNode* current = *p_CurrentStart;

            //swap over
            *p_CurrentStart = first;
            *(p_CurrentStart - 1) = current;
        }
        public ASNode* Peak() {
            return *(p_CurrentStart);
        }

        public void Clear() {
            p_CurrentStart = p_CurrentEnd = p_Base;
        }
    }

}
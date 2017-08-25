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
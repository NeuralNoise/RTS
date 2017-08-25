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

    [StructLayout(LayoutKind.Explicit, Pack=1)]
    private unsafe struct ASNode {
        [FieldOffset(0)]
        public ASNodeState State;

        [FieldOffset(1)]
        public short ParentX;

        [FieldOffset(3)]
        public short ParentY;

        /// <summary>
        /// Node distance from start
        /// </summary>
        [FieldOffset(5)]
        public float G;
    }
}
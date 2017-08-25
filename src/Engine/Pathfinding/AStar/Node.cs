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
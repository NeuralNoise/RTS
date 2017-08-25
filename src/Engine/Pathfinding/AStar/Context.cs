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
using System.Runtime.InteropServices;
using System.Collections.Generic;


public static partial class Pathfinder {
    private unsafe class ASContext : IPathfinderContext {
        private int p_Width, p_Height;

        private int p_EndX, p_EndY;

        private bool* p_ConcreteMatrix;
        private int* p_AdjacentLocations;

        private ASNode* p_NodeMatrix;
        private ASNode** p_AdjacentNodes;

        private ASNodeQueue p_Queue;

        public ASContext(int width, int height, int stackSize) {
            p_Width = width;
            p_Height = height;

            /*allocate*/
            initAdjacent(out p_AdjacentNodes, out p_AdjacentLocations);
            p_NodeMatrix = initNodeMatrix(width, height);
            p_Queue = new ASNodeQueue(stackSize);

            int sz = sizeof(ASNode);

            return;
        }

        public bool search(ASNode* firstNode, ASNode* endNode) {
            //return searchRecursive(firstNode, 0);
            ASNodeQueue nodeStack = p_Queue;
            nodeStack.Clear();
            nodeStack.Push(firstNode);

            int count = 1;
            while (count != 0) {
                ASNode* current = nodeStack.Pop();
                (*current).State = ASNodeState.CLOSED;
                count--;

                //sort by total cost      
                /*
                adjacent.Sort(delegate(ASNode n1, ASNode n2) {
                    float n1F = n1.G + n1.H;
                    float n2F = n2.G + n2.H;
                    return n1F.CompareTo(n2F);
                });
                */

                getAdjacent(current);
                ASNode** adjacent = p_AdjacentNodes;
                ASNode** adjacentEnd = adjacent + 8;
                
                while(adjacent != adjacentEnd) {
                    if ((*adjacent) == (ASNode*)0) { break; }

                    //we hit the end?
                    if (*adjacent == endNode) {
                        return true;
                    }

                    //get the pointer for this node
                    nodeStack.Push(*adjacent);
                    adjacent++;
                    count++;
                }
            }

            /*no path was found*/
            return false;
        }
        public ASNode* reset(bool* concrete, int endX, int endY) {
            p_ConcreteMatrix = concrete;
            p_EndX = endX;
            p_EndY = endY;

            resetNodeMatrix(p_NodeMatrix, p_Width, p_Height);
            return p_NodeMatrix;
        }


        /*will always return 8*2 (16 locations) */
        public void getAdjacentLocations(int x, int y, int* output) {
            int* ptr = output;

            /*
                Note: We have to assign diagonals last so 
                we can verify if NESW blocks are collidable
                first. Also it is easy to deturmine if we 
                are looking at a diagonal by just doing some
                pointer arith.
            */

            /*top*/
            *(ptr++) = x; *(ptr++) = y - 1; //x,y-1

            /*bottom*/
            *(ptr++) = x; *(ptr++) = y + 1; //x,y+1

            /*left*/
            *(ptr++) = x - 1; *(ptr++) = y; //x-1,y

            /*right*/
            *(ptr++) = x + 1; *(ptr++) = y; //x+1,y

            /*diagonals*/
            *(ptr++) = x - 1; *(ptr++) = y - 1; //x-1,y-1
            *(ptr++) = x + 1; *(ptr++) = y - 1; //x+1,y-1

            *(ptr++) = x - 1; *(ptr++) = y + 1; //x-1,y+1
            *(ptr++) = x + 1; *(ptr++) = y + 1; //x+1,y+1

        }
        public void getAdjacent(ASNode* current) {

            //zero fill the current adjacent nodes
            ASNode** buffer = p_AdjacentNodes;
            zeroAdjacent(buffer);

            //populate adjacent
            short currentX = (short)((current - p_NodeMatrix) % p_Width);
            short currentY = (short)((current - p_NodeMatrix) / p_Width);

            float currentG = (*current).G;
            getAdjacentLocations(currentX, currentY, p_AdjacentLocations);

            //iterate through each adjacent location
            int* adjacentLocationPtr = p_AdjacentLocations;
            int* adjacentLocationEnd = p_AdjacentLocations + 16;
            int* adjacentLocationDiagonals = p_AdjacentLocations + 8;
            bool allowDiagonal = true;
            while (adjacentLocationPtr != adjacentLocationEnd) {
                //read x,y
                int x = *(adjacentLocationPtr++);
                int y = *(adjacentLocationPtr++);

                //bound check
                if (x < 0 || y < 0 ||
                   x >= p_Width || y >= p_Height) { continue; }

                //only allow diagonals when all adjacent NESW blocks 
                //are not collidable.
                bool isDiagonal = adjacentLocationPtr > adjacentLocationDiagonals;
                if (isDiagonal && !allowDiagonal) {
                    continue;
                }

                //get the node at this location
                int offset = (y * p_Width) + x;
                bool isConcrete = *(p_ConcreteMatrix + offset);
                ASNode* node = p_NodeMatrix + offset;

                //solid?
                if (isConcrete) {
                    allowDiagonal = false;
                    continue;
                }

                //already closed?
                if ((*node).State == ASNodeState.CLOSED) {
                    continue;
                }

                //open?
                if ((*node).State == ASNodeState.OPEN) {
                    //get the distance from current node to this node.
                    float distance = calcDistance(x, y, currentX, currentY);
                    float gNew = currentG + distance;
                    if (gNew < (*node).G) {
                        (*node).ParentX = currentX;
                        (*node).ParentY = currentY;
                        (*node).G = gNew;

                        (*(buffer++)) = node;
                    }
                    continue;
                }

                //not tested
                (*node).ParentX = currentX;
                (*node).ParentY = currentY;
                (*node).G = currentG + calcDistance(
                    x, y,
                    currentX, currentY);
                (*node).State = ASNodeState.OPEN;

                (*(buffer++)) = node;
            }

        }
        public void zeroAdjacent(ASNode** ptr) {
            ASNode** end = ptr + 8;
            while (ptr != end) {
                (*(ptr++)) = (ASNode*)0;
            }
        }



        private static float calcDistance(int x1, int y1, int x2, int y2) {
            /*use pythagoras c^2=a^2+b^2 to get distance.*/
            int dX = x2 - x1;
            int dY = y2 - y1;

            return (float)Math.Sqrt(
                (dX * dX) +
                (dY * dY));
        }

        [DllImport("kernel32.dll")]
        private static extern void ZeroMemory(byte* ptr, int size);
        private static void resetNodeMatrix(ASNode* matrix, int w, int h) {
            ZeroMemory((byte*)matrix, (w * h) * sizeof(ASNode));
        }


        private static ASNode* initNodeMatrix(int w, int h) {
            //allocate the matrix
            int blockSize = w * h * sizeof(ASNode);
            ASNode* matrix = (ASNode*)Marshal.AllocHGlobal(blockSize);
            ZeroMemory((byte*)matrix, blockSize);
            return matrix;
        }
        private static void initAdjacent(out ASNode** adjacent, out int* adjacentLocations) {
            adjacent = (ASNode**)Marshal.AllocHGlobal(8 * sizeof(ASNode*));
            adjacentLocations = (int*)Marshal.AllocHGlobal(16 * sizeof(int));
        }


        public int Width { get { return p_Width; } }
        public int Height { get { return p_Height; } }
        public Size Size {
            get {
                return new Size(p_Width, p_Height);
            }
        }
        public void Dispose() {
            Marshal.FreeHGlobal((IntPtr)p_AdjacentNodes);
            Marshal.FreeHGlobal((IntPtr)p_AdjacentLocations);
            Marshal.FreeHGlobal((IntPtr)p_NodeMatrix);
        }
    }
}
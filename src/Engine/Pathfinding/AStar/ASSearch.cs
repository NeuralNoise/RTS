using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static unsafe partial class Pathfinder {

    public static List<Point> ASSearch(Point start, Point end, bool[][] concreteMatrix, int width, int height) {
        IntPtr matrix = IntPtr.Zero;

        List<Point> result = ASSearch(
            start,
            end,
            concreteMatrix,
            ref matrix,
            width,
            height);

        Marshal.FreeHGlobal(matrix);
        return result;

    }
    public static List<Point> ASSearch(Point start, Point end, bool[][] concreteMatrix, ref IntPtr matrix, int width, int height) {
        bool* ptr = getConcreteMatrix(concreteMatrix, width, height);

        List<Point> result = ASSearch(
            start,
            end,
            ptr,
            ref matrix,
            width, 
            height);

        Marshal.FreeHGlobal((IntPtr)ptr);

        return result;


    }

    public static List<Point> ASSearch(Point start, Point end, bool* concreteMatrix, int width, int height) {
        IntPtr matrix = IntPtr.Zero;
        List<Point> result = ASSearch(
            start,
            end,
            concreteMatrix,
            ref matrix,
            width, 
            height
       );
       Marshal.FreeHGlobal(matrix);
       return result;

    }
    public static List<Point> ASSearch(Point start, Point end, bool* concreteMatrix, ref IntPtr matrix, int width, int height) {
        int startX = start.X, startY = start.Y;
        int endX = end.X, endY = end.Y;

        //already there?
        if (startX == endX && startY == endY) { 
            return new List<Point>();
        }

        //verify start/end is not collidable!
        bool startCollide = *(concreteMatrix + (startY * width) + startX);
        bool endCollide = *(concreteMatrix + (endY * width) + endX);
        if (startCollide || endCollide) {
            return new List<Point>();
        }

        //allocate the node matrix
        ASNode* nodeMatrix = (ASNode*)matrix;
        if (nodeMatrix == (ASNode*)0) {
            nodeMatrix = initNodeMatrix(width, height);
            matrix = (IntPtr)nodeMatrix;
        }
        else {
            resetNodeMatrix(nodeMatrix, width, height);
        }
        
        //allocate the adjacent location int array
        int* adjacentLocations = (int*)Marshal.AllocHGlobal(16 * sizeof(int));

        //get the node at the start and end
        ASNode* startNode = nodeMatrix + (startY * width) + startX;
        ASNode* endNode = nodeMatrix + (endY * width) + endX;

        //search
        asContext ctx = new asContext(endX, endY, width, height, adjacentLocations, nodeMatrix, concreteMatrix);
        bool found = ctx.search(startNode);//search(startNode, endX, endY, width, height, adjacentLocations, nodeMatrix, concreteMatrix);
        if (!found) {
            return new List<Point>();
        }

        //cycle back from the end point all the way back to the start
        //adding each node location on the way. 
        //note: since we have done this backwards, just reverse the list
        ASNode* current = endNode;

        List<Point> buffer = new List<Point>();
        buffer.Add(end);
        while ((*current).Parent != (ASNode*)0) {
            buffer.Add(new Point(
                (*current).X,
                (*current).Y));
            current = (*current).Parent;
        }
        buffer.Add(start);
        buffer.Reverse();

        //clean up
        Marshal.FreeHGlobal((IntPtr)adjacentLocations);
        return buffer;
    }
 
    private static ASNode* initNodeMatrix(int w, int h) {
        //allocate the matrix
        ASNode* matrix = (ASNode*)Marshal.AllocHGlobal(
            w * h * sizeof(ASNode));

        //populate
        int x = 0;
        int y = 0;
        ASNode* ptr = matrix;
        ASNode* end = matrix + (w * h);
        while (ptr != end) {
            ASNode* node = (ptr++);
            (*node).X = x;
            (*node).Y = y;
            
            (*node).Initialized = false;

            (*node).Parent = (ASNode*)0;
            (*node).State = ASNodeState.NONE;

            (*node).H = 0;
            (*node).G = 0;


            //update x/y
            x++;
            if (x == w) {
                x = 0;
                y++;
            }
        }

        return matrix;
    }
    private static void resetNodeMatrix(ASNode* matrix, int w, int h) {
        ASNode* ptr = matrix;
        ASNode* ptrEnd = ptr + (w * h);
        while (ptr != ptrEnd) {
            ASNode* current = ptr++;

            (*current).Initialized = false;
            (*current).Parent = (ASNode*)0;
            (*current).State = ASNodeState.NONE;
        }
    }

    
    /*encapsulate the search recursion to prevent stack overflow*/
    private class asContext {
        public int w;
        public int h;
        public int endX;
        public int endY;
        public int* adjacentLocations;
        public ASNode* nodeMatrix;
        public bool* concreteMatrix;

        public asContext(int eX, int eY, int wdth, int hght, int* adjLoations, ASNode* matrix, bool* concrete) {
            w = wdth;
            h = hght;
            endX = eX;
            endY = eY;
            adjacentLocations = adjLoations;
            nodeMatrix = matrix;
            concreteMatrix = concrete;
        }

        public bool search(ASNode* current) {
            (*current).State = ASNodeState.CLOSED;

            //get adjacent nodes
            List<ASNode> adjacent = getAdjacent(current);

            //sort by total cost
            adjacent.Sort(delegate(ASNode n1, ASNode n2) {
                float n1F = n1.G + n1.H;
                float n2F = n2.G + n2.H;
                return n1F.CompareTo(n2F);
            });

            foreach (ASNode n in adjacent) {
                if (n.X == endX && n.Y == endY) {
                    return true;
                }

                //get the pointer for this node
                ASNode* ptr = nodeMatrix + (n.Y * w) + n.X;

                //recursive call for this node.
                bool result = search(ptr);
                if (result) { return true; }
            }

            return false;
        }

        /*will always return 8*2 (16 locations) */
        private void getAdjacentLocations(int x, int y, int* output) {
            int* ptr = output;

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
        private List<ASNode> getAdjacent(ASNode* current) {
            List<ASNode> buffer = new List<ASNode>();

            //initialize current
            if (!(*current).Initialized) {
                (*current).H = calcDistance(
                    (*current).X,
                    (*current).Y,
                    endX, endY);
                (*current).Initialized = true;
            }

            //populate adjacent
            int currentX = (*current).X;
            int currentY = (*current).Y;
            float currentG = (*current).G;
            getAdjacentLocations(currentX, currentY, adjacentLocations);

            //iterate through each adjacent location
            int* adjacentLocationPtr = adjacentLocations;
            int* adjacentLocationEnd = adjacentLocations + 16;
            while (adjacentLocationPtr != adjacentLocationEnd) {
                //read x,y
                int x = *(adjacentLocationPtr++);
                int y = *(adjacentLocationPtr++);

                //bound check
                if (x < 0 || y < 0 ||
                   x >= w || y >= h) { continue; }

                //get the node at this location
                int offset = (y * w) + x;
                bool isConcrete = *(concreteMatrix + offset);
                ASNode* node = nodeMatrix + offset;

                //solid?
                if (isConcrete) { continue; }

                //already closed?
                if ((*node).State == ASNodeState.CLOSED) {
                    continue;
                }

                /*initialize?*/
                if (!(*node).Initialized) {
                    (*node).H = calcDistance(x, y, endX, endY);
                    (*node).Initialized = true;
                }

                //open?
                if ((*node).State == ASNodeState.OPEN) {
                    ASNode* parent = (*node).Parent;

                    //get the distance from current node to this node.
                    float distance = calcDistance(x, y, (*parent).X, (*parent).Y);
                    float gTemp = currentG + distance;
                    if (gTemp < (*node).G) {
                        (*node).Parent = current;
                        (*node).G = currentG + calcDistance(
                            x, y,
                            currentX, currentY);
                        buffer.Add(*node);
                    }
                    continue;
                }

                //not tested
                (*node).Parent = current;
                (*node).G = currentG + calcDistance(
                    x, y,
                    currentX, currentY);
                (*node).State = ASNodeState.OPEN;
                buffer.Add(*node);

            }

            return buffer;

        }

        private static float calcDistance(int x1, int y1, int x2, int y2) {
            /*use pithags c^2=a^2+b^2 to get distance.*/

            int dX = x2 - x1;
            int dY = y2 - y1;

            return (float)Math.Sqrt(
                (dX * dX) +
                (dY * dY));
        }
    }
}
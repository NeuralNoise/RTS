using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static unsafe partial class Pathfinder {

    public static List<Point> ASSearch(Point start, Point end, bool[][] concreteMatrix, int width, int height) {
        bool* ptr = getConcreteMatrix(concreteMatrix, width, height);
        List<Point> buffer = ASSearch(
            start,
            end,
            ptr,
            width,
            height);
        Marshal.FreeHGlobal((IntPtr)ptr);
        return buffer;
    }
    public static List<Point> ASSearch(Point start, Point end, bool* concreteMatrix, int width, int height) {
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
        PathfinderASNode* nodeMatrix = initNodeMatrix(width, height, endX, endY);
        
        //allocate the adjacent location int array
        int* adjacentLocations = (int*)Marshal.AllocHGlobal(16 * sizeof(int));

        //get the node at the start and end
        PathfinderASNode* startNode = nodeMatrix + (startY * width) + startX;
        PathfinderASNode* endNode = nodeMatrix + (endY * width) + endX;

        //search
        bool found = search(startNode, endX, endY, width, height, adjacentLocations, nodeMatrix, concreteMatrix);
        if (!found) {
            return new List<Point>();
        }

        //cycle back from the end point all the way back to the start
        //adding each node location on the way. 
        //note: since we have done this backwards, just reverse the list
        PathfinderASNode* current = endNode;

        List<Point> buffer = new List<Point>();
        buffer.Add(end);
        while ((*current).Parent != (PathfinderASNode*)0) {
            buffer.Add(new Point(
                (*current).X,
                (*current).Y));
            current = (*current).Parent;
        }
        buffer.Add(start);
        buffer.Reverse();

        //clean up
        Marshal.FreeHGlobal((IntPtr)nodeMatrix);
        Marshal.FreeHGlobal((IntPtr)adjacentLocations);
        return buffer;
    }
 
    private static bool search(PathfinderASNode* current, int endX, int endY, int w, int h, int* adjacentLocations, PathfinderASNode* nodeMatrix, bool* concreteMatrix) {
        (*current).State = PathfinderASNodeState.CLOSED;

        //get adjacent nodes
        List<PathfinderASNode> adjacent = getAdjacent(
            w, h, endX, endY, adjacentLocations, current, nodeMatrix, concreteMatrix);

        //sort by total cost
        adjacent.Sort(delegate(PathfinderASNode n1, PathfinderASNode n2) {
            float n1F = n1.G + n1.H;
            float n2F = n2.G + n2.H;
            return n1F.CompareTo(n2F);
        });

        foreach (PathfinderASNode n in adjacent) {
            if (n.X == endX && n.Y == endY) {
                return true;
            }

            //get the pointer for this node
            PathfinderASNode* ptr = nodeMatrix + (n.Y * w) + n.X;

            //recursive call for this node.
            bool result = search(
                ptr,
                endX, endY,
                w, h,
                adjacentLocations,
                nodeMatrix,
                concreteMatrix);
            if (result) { return true; }
        }

        return false;
    }

    /*will always return 8*2 (16 locations) */
    private static void getAdjacentLocations(int x, int y, int* output) {
        int* ptr = output;

        /*top*/
        *(ptr++) = x - 1; *(ptr++) = y - 1; //x-1,y-1
        *(ptr++) = x; *(ptr++) = y - 1; //x,y-1
        *(ptr++) = x + 1; *(ptr++) = y - 1; //x+1,y-1

        /*bottom*/
        *(ptr++) = x - 1; *(ptr++) = y + 1; //x-1,y+1
        *(ptr++) = x; *(ptr++) = y + 1; //x,y+1
        *(ptr++) = x + 1; *(ptr++) = y + 1; //x+1,y+1

        /*left*/
        *(ptr++) = x - 1; *(ptr++) = y; //x-1,y
       
        /*right*/
        *(ptr++) = x + 1; *(ptr++) = y; //x+1,y
    }

    private static List<PathfinderASNode> getAdjacent(int w, int h, int endX, int endY, int* adjacentLocations, PathfinderASNode* current, PathfinderASNode* nodeMatrix, bool* concreteMatrix) {
        List<PathfinderASNode> buffer = new List<PathfinderASNode>();
        
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
            PathfinderASNode* node = nodeMatrix + offset;

            //solid?
            if (isConcrete) { continue; }

            //already closed?
            if ((*node).State == PathfinderASNodeState.CLOSED) { 
                continue; 
            }

            /*initialize?*/
            if (!(*node).Initialized) {
                (*node).H = calcDistance(x, y, endX, endY);
                (*node).Initialized = true;
            }

            //open?
            if ((*node).State == PathfinderASNodeState.OPEN) {
                PathfinderASNode* parent = (*node).Parent;

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
            (*node).State = PathfinderASNodeState.OPEN;
            buffer.Add(*node);

        }

        return buffer;

    }

    private static PathfinderASNode* initNodeMatrix(int w, int h, int endX, int endY) {
        //allocate the matrix
        PathfinderASNode* matrix = (PathfinderASNode*)Marshal.AllocHGlobal(
            w * h * sizeof(PathfinderASNode));

        //populate
        int x = 0;
        int y = 0;
        PathfinderASNode* ptr = matrix;
        PathfinderASNode* end = matrix + (w * h);
        while (ptr != end) {
            PathfinderASNode* node = (ptr++);
            (*node).X = x;
            (*node).Y = y;
            
            (*node).Initialized = false;

            (*node).Parent = (PathfinderASNode*)0;
            (*node).State = PathfinderASNodeState.NONE;

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

    private static float calcDistance(int x1, int y1, int x2, int y2) {
        /*use pithags c^2=a^2+b^2 to get distance.*/

        int dX = x2 - x1;
        int dY = y2 - y1;

        return (float)Math.Sqrt(
            (dX * dX) +
            (dY * dY));
    }
}
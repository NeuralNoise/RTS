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
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static unsafe partial class Pathfinder {
    private const int DEFAULT_STACK_SIZE = 1000;

    public static List<Point> ASSearch(Point start, Point end, bool* concreteMatrix, int width, int height) {
        return ASSearch(
            start,
            end,
            concreteMatrix,
            width,
            height,
            DEFAULT_STACK_SIZE);
    }
    public static List<Point> ASSearch(Point start, Point end, bool* concreteMatrix, int width, int height, int stackSize) {
        IPathfinderContext ctx = ASCreateContext(width, height, stackSize);
        List<Point> buffer = ASSearch(
            ctx,
            start,
            end,
            concreteMatrix);
        ctx.Dispose();
        return buffer;        
    }
    
    public static List<Point> ASSearch(IPathfinderContext context, Point start, Point end, bool* concreteMatrix) {
        int startX = start.X, startY = start.Y;
        int endX = end.X, endY = end.Y;
        int width = context.Width;
        int height = context.Height;

        //validate context
        if (!(context is ASContext) || context == null) {
            throw new Exception("Invalid A* search context!");
        }
        ASContext ctx = context as ASContext;

        //verify start/end is not collidable!
        bool startCollide = *(concreteMatrix + (startY * width) + startX);
        bool endCollide = *(concreteMatrix + (endY * width) + endX);
        if (startCollide || endCollide) {
            bool resolved = false;

            /*attempt to resolve the collision*/
            if (startCollide) {
                resolved = ResolveCollision(ref start, end, width, height, concreteMatrix);
            }
            if (endCollide) {
                resolved = ResolveCollision(ref end, start, width, height, concreteMatrix);
            }

            //not resolved?
            if (!resolved) {
                return new List<Point>();
            }
            startX = start.X; startY = start.Y;
            endX = end.X; endY = end.Y;
        }

        //already there?
        if (startX == endX && startY == endY) { 
            return new List<Point>();
        }

        //reset the context
        ASNode* nodeMatrix = ctx.reset(
            concreteMatrix,
            endX,
            endY);

        //get the node at the start and end
        ASNode* startNode = nodeMatrix + (startY * width) + startX;
        ASNode* endNode = nodeMatrix + (endY * width) + endX;

        //search
        bool found = ctx.search(startNode, endNode);
        if (!found) {
            return new List<Point>();
        }

        //cycle back from the end point all the way back to the start
        //adding each node location on the way. 
        //note: since we have done this backwards, just reverse the list
        ASNode* current = endNode;
        List<Point> buffer = new List<Point>();
        buffer.Add(end);

        while (current != startNode) {
            //get the X/Y from the pointer.
            short x = (short)((current - nodeMatrix) % width);
            short y = (short)((current - nodeMatrix) / width);
            buffer.Add(new Point(x, y));

            //resolve the parent
            ASNode* parent = nodeMatrix + ((*current).ParentY * width) + (*current).ParentX;
            current=parent;
        }

        buffer.Add(start);
        buffer.Reverse();
        return buffer;
    }


    public static IPathfinderContext ASCreateContext(int width, int height) {
        return ASCreateContext(width, height, DEFAULT_STACK_SIZE);
    }
    public static IPathfinderContext ASCreateContext(int width, int height, int stackSize) {
        return new ASContext(width, height, stackSize);
    }
}

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public unsafe partial class Pathfinder {
    public static List<Point> CompressPath(List<Point> path) {
        int pathLength = path.Count;

        /*check*/
        if (path.Count <= 2) { return path; }

        /*define the return buffer but add the first node since we will skip it*/
        List<Point> buffer = new List<Point>();
        buffer.Add(path[0]);

        /*iterate through the list*/
        Point lastPoint = path[0];
        int velocityX = 0, velocityY = 0;
        for (int c = 1; c < pathLength - 1; c++) { 
            //deturmine the vector for this point from the last
            Point point = path[c];

            int vX = 0, vY = 0;
            if (point.X < lastPoint.X) { vX = -1; }
            if (point.Y < lastPoint.Y) { vY = -1; }
            if (point.X > lastPoint.X) { vX = 1; }
            if (point.Y > lastPoint.Y) { vY = 1; }

            //if the vector has changed from the last, we add this point
            if (vX != velocityX || vY != velocityY) {
                buffer.Add(lastPoint);
                velocityX = vX;
                velocityY = vY;
            }

            lastPoint = point;
        }

        //add the last point
        buffer.Add(path[pathLength - 1]);

        return buffer;
    }


    private static bool* getConcreteMatrix(bool[][] matrix, int width, int height) {
        //allocate
        bool* buffer = (bool*)Marshal.AllocHGlobal(
            width * height);

        
        //populate
        bool* ptr = buffer;
        bool* ptrEnd = buffer + (width * height);
        int x = 0, y = 0;
        while (ptr != ptrEnd) {

            *(ptr++) = matrix[y][x];

            x++;
            if (x == width) {
                x = 0;
                y++;
            }
        }

        return buffer;
    }
}
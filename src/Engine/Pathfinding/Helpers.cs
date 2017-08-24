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

    public static bool ResolveCollision(ref Point point, Point endPoint, int width, int height, bool* concreteMatrix) { 
        //deturmine the direction we would be coming
        //from
        int dX = endPoint.X - point.X;
        int dY = endPoint.Y - point.Y;
        if (dX < 0) { dX = -1; }
        else { dX = 1; }
        if (dY < 0) { dY = -1; }
        else { dY = 1; }

        //keep decreasing location until we hit a non-concrete block
        int newX = point.X;
        int newY = point.Y;
        while (newX != 0 && newY != 0 &&
              newX < width && newY < height) {

            //none collide?
            if (!(*(concreteMatrix + (newY * width) + newX))) {
                point = new Point(newX, newY);
                return true;
            }
                 
            newX += dX;
            newY += dY;
        }
        
        //not resolved
        return false;
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
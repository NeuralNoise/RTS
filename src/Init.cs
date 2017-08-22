using System;
using System.Drawing;
using System.Windows.Forms;


using System.Collections.Generic;

static class Init {
    
    static void printMatrix(bool[][] matrix, bool[][] path, int w, int h) {
        for (int y = 0; y < h; y++) {
            for (int x = 0; x < w; x++) {

                char ch = ' ';
                if (matrix[y][x]) { ch = 'X'; }
                if (path[y][x]) { ch = '.'; }
                Console.Write(ch);
            }
            Console.Write('\n');
        }
    }

    static bool[][] toMatrix(string str, int width, int height) {
        string[] lines = str.Split('\n');
        bool[][] matrix = new bool[height][];
        for (int y = 0; y < height; y++) {
            string line = lines[y].Replace("\r", "");
            matrix[y] = new bool[width];
            for (int x = 0; x < width; x++) {
                matrix[y][x] = line[x] == ' ';    
            }
        }
        return matrix;
    }

    static bool[][] getPath(bool[][] matrix, Point p1, Point p2, int w, int h) {
        List<Point> list = Pathfinder.ASSearch(
            p1,
            p2,
            matrix,
            w, h);

        list = Pathfinder.CompressPath(list);

        bool[][] buffer = new bool[h][];
        for (int y = 0; y < h; y++) {
            buffer[y] = new bool[w];
        }

        foreach (Point p in list) {
            buffer[p.Y][p.X] = true;
        }
        return buffer;
    }



    [STAThread]
    static void Main(string[] args) {
        string data =
            "XXXXXXXXXX\n" +
            "         X\n" +
            "XXXXXXXXXX\n" +
            "X         \n" +
            "XXXXXXXXXX\n" +
            "XXXXXXXXX \n";
        bool[][] matrix = toMatrix(data, 10, 6);
        bool[][] path = getPath(matrix, new Point(0, 5), 
                                        new Point(0, 0), 10, 6);
        printMatrix(matrix, path, 10, 6);

        

        Application.EnableVisualStyles();

        GameWindow wnd = new GameWindow();
        Game game = new Game(wnd);

        Application.Run(wnd);
    }
}
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
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

static class Init {

    static int hashFont(Font f) {
        return
            f.Name.GetHashCode() +
            f.Size.GetHashCode() +
            f.Style.GetHashCode();
    }

    [STAThread]
    static void Main(string[] args) {

        int hash = (2.0f).GetHashCode();
        int hash2 = (3.0f).GetHashCode();

        Console.WriteLine("Hello World".GetHashCode().ToString("X"));

        Application.EnableVisualStyles();

        GameWindow wnd = new GameWindow();
        Game game = new Game(wnd);
        Application.Run(wnd);

    }
}
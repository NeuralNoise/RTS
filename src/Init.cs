using System;
using System.Drawing;
using System.Windows.Forms;


using System.Collections.Generic;

static class Init {
    [STAThread]
    static void Main(string[] args) {
        Application.EnableVisualStyles();

        new System.Threading.Thread(new System.Threading.ThreadStart(delegate{

            GameWindow wnd = new GameWindow();
            Game game = new Game(wnd);
            Application.Run(wnd);
        }), 1024 * 1024 * 5).Start();

    }
}
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
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

public interface IUI {
    void Update();

    void Draw(IRenderContext context, IRenderer renderer);

    void OnKeyDown(Game game, KeyEventArgs e);
    void OnKeyUp(Game game, KeyEventArgs e);

    void OnMouseClick(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseMove(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseDown(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseUp(Game game, Point mousePosition, MouseEventArgs e);
    void OnMouseScroll(Game game, Point mousePosition, MouseEventArgs e);

    void Enable();
    void Disable();

    bool Visible { get; set; }
    bool Enabled { get; }
}
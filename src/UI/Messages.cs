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
using System.Windows.Forms;
using System.Collections.Generic;

/*very basic messaging system at the moment, we yet to impliment text-wrapping etc...
  we are only using this at the moment to see what debug commands has been used*/
public class UIMessages : UIControl {
    private Font p_Font;
    private LinkedList<messageEntry> p_Messages;

    private object p_Mutex = new object();

    public UIMessages(Game game) : base(game) {
        p_Font = new Font("Arial", 12, FontStyle.Bold);
        p_Messages = new LinkedList<messageEntry>();
    }

    public override void Update() { }
    public override void Draw(IRenderContext context, IRenderer renderer) {
        lock (p_Mutex) {
            IFont font = renderer.SetFont(p_Font);

            Point renderLocation=RenderLocation;
            int rX = renderLocation.X;
            int rY = renderLocation.Y;

            foreach (messageEntry e in p_Messages) {

                Size size = renderer.MeasureString(e.message[0], font);


                drawShadow(renderer, p_Font, e.message[0], 5, rX, rY);
                renderer.SetBrush(new SolidBrush(e.renderColor));
                renderer.DrawString(
                    e.message[0],
                    rX,
                    rY);


                rY += size.Height + 5;

            }



        }
    }

    private void drawShadow(IRenderer renderer, Font font, string str, int shadowSize, int x, int y) {
        Color color = Color.FromArgb(40, 0, 0, 0);
        renderer.SetBrush(new SolidBrush(color));
        for (int c = 0; c < shadowSize; c++) {
            renderer.DrawString(
                str,
                x + c,
                y + c);
        }
    }


    public void AddMessage(string message, Color color) {
        //don't allow blank strings
        if (message.Replace(" ", "").Length == 0) {
            throw new Exception("String cannot be empty");
        }

        lock (p_Mutex) { 
            //max message length?
            if (message.Length > 120) {
                throw new Exception("Message limit of 120 exceeded. String length: " + message.Length);
            }

            //shift the messages up?
            if (p_Messages.Count == 10) {
                p_Messages.RemoveFirst();
            }   

            //add the new message
            p_Messages.AddLast(new messageEntry { 
                message = message.Split('\n'),
                renderColor = color
            });


            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
            player.SoundLocation = "sounds/message.wav";
            player.Play();
        }
    }
    public void Clear() {
        lock (p_Mutex) {
            p_Messages.Clear();
        }
    }

    private class messageEntry {
        public string[] message;

        public Color renderColor;
    }
}
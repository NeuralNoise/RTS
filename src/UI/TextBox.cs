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
using System.Runtime.InteropServices;
using System.Text;

public class UITextBox : UIControl {
    private int p_TextXOffset = 5;
    private string p_Text;
    private Font p_Font;

    private long p_CaratNextFlipTicks = 0;
    private int p_CaratPosition;
    private int p_CaratHeight;
    private int p_CaratRenderX;

    private int p_Padding = 10;

    private bool p_Invalidate;

    private bool p_ShowCarrat = false;

    private Brush p_ForeBrush = Brushes.Black;

    public UITextBox(Game game) : base(game) {
        p_Text = "";

        /*size to default*/
        Width = 200;

        /*hook events*/
        KeyDown += handleKeyDown;
        Resize += handleResize;
    }

    public override void Update() {
        if (!Visible) { return; }

        //the Carat flips it's visibility every 500ms
        const long timeNextIncr = TimeSpan.TicksPerMillisecond * 500;
        long currentTime = DateTime.Now.Ticks;
        if (currentTime >= p_CaratNextFlipTicks) {
            p_ShowCarrat = !p_ShowCarrat;

            //set the time of the next flip
            p_CaratNextFlipTicks = currentTime + timeNextIncr;
        }

    }
    public override void Draw(IRenderContext context, IRenderer renderer) {       
        if (p_Invalidate) {
            invalidate(context, renderer);
        }

        /*get render x/y*/
        Point renderLocation = RenderLocation;
        int rX = renderLocation.X;
        int rY = renderLocation.Y;

        //draw background
        renderer.SetBrush(Brushes.White);
        renderer.FillQuad(
            rX, rY, Width, Height);

        //draw border
        renderer.SetPen(new Pen(Brushes.Black, 1));
        renderer.DrawQuad(
            rX, rY, Width, Height);
        renderer.SetBrush(p_ForeBrush);

        #region draw Carat
        if (Focused && p_ShowCarrat) {

            const int carratWidth = 2;

            renderer.FillQuad(
                rX + p_CaratRenderX,
                rY + p_Padding,
                carratWidth,
                p_CaratHeight);
        }

        #endregion

        /*draw the text*/
        renderer.SetFont(p_Font);
        renderer.DrawString(
            p_Text,
            rX + p_TextXOffset,
            rY + p_Padding);

    }

    private void invalidate(IRenderContext context, IRenderer renderer) {
        //font null?
        if (p_Font == null) { return; }
        
        //allocate the current font onto
        //the rendering context
        IFont font = null;
        try { font = context.AllocateFont(p_Font); }
        catch { return; }

        //keep reducing the text until it all fits in the text box
        int maxWidth = Width - (p_TextXOffset * 2);
        while (renderer.MeasureString(p_Text, font).Width >= maxWidth) {
            p_Text = p_Text.Substring(0, p_Text.Length - 1);
        }

        //set the height to the height of the font + padding
        p_CaratHeight = renderer.GetFontHeight(font);
        Height = p_CaratHeight + (p_Padding << 1);

        //calculate the render x position of the Carat
        int renderX = p_TextXOffset;
        int textLength = p_Text.Length;
        if (p_CaratPosition >= textLength) {
            p_CaratPosition = textLength - 1;
        }
        for (int c = 0; c < p_CaratPosition + 1; c++) {
            char character = p_Text[c];
            int width = renderer.GetCharWidth(character, font);
            renderX += width;
        }

        p_CaratRenderX = renderX;
        p_Invalidate = false;
    }

    private void handleKeyDown(object sender, KeyEventArgs e) {
        if (!Visible) { return; }

        uint key = (uint)e.KeyData;

        #region left/right?
        int caratDelta = 0;
        if (e.KeyCode == Keys.Left) {
            caratDelta = -1;
        }
        if (e.KeyCode == Keys.Right) {
            caratDelta = 1;
        }
        if (caratDelta != 0) { 
            
            //try and move the carat
            p_CaratPosition += caratDelta;
            if (p_CaratPosition < -1) { p_CaratPosition = -1; }
            if (p_CaratPosition >= p_Text.Length) {
                p_CaratPosition = p_Text.Length - 1;
            }

            p_ShowCarrat = true;
            p_CaratNextFlipTicks = DateTime.Now.AddMilliseconds(500).Ticks;
            p_Invalidate = true;
        }
        #endregion

        #region control keys
        //backspace?
        if (e.KeyCode == Keys.Back) {
            //no characters?
            if (p_Text.Length == 0 || p_CaratPosition == -1) {
                return;
            }

            //remove at the carrat index
            p_Text = p_Text.Remove(p_CaratPosition, 1);
            p_CaratPosition--;
            p_Invalidate = true;
            return;
        }

        //delete?
        if (e.KeyCode == Keys.Delete) { 
            //can we make a deletion?
            if (p_CaratPosition == p_Text.Length - 1) {
                return;
            }

            p_Text = p_Text.Remove(p_CaratPosition + 1, 1);
            p_Invalidate = true;
            return;

        }

        //paste
        if (e.Control && e.KeyCode == Keys.V) {
            if (Clipboard.ContainsText()) {
                insertTextToCarat(
                    Clipboard.GetText());
            }

            return;
        }

        #endregion

        /*get the current state of the keyboard*/
        byte[] keyboarsState = new byte[256];
        GetKeyboardState(keyboarsState);

        /*map the key to a virtual key*/
        uint scan = MapVirtualKey((uint)e.KeyData, 0x00);
        StringBuilder buffer = new StringBuilder(2);

        /*convert the vkey to a unicode character*/
        int result = ToUnicode(
            key,
            scan,
            keyboarsState,
            buffer,
            2,
            0);

        if (result < 1) { return; }
        char character = buffer[0];
        buffer = null;

        //control character?
        if (Char.IsControl(character)) { return; }

        if (p_CaratPosition == p_Text.Length - 1) {
            p_Text += character;
            p_CaratPosition++;
            p_Invalidate = true;
            return;
        }

        /*insert into wherever the carrat currently is*/
        insertTextToCarat(character.ToString());
    }
    private void handleResize(object sender, Size oldSize, Size newSize, out bool allowed) {
        allowed = true;
        p_Invalidate = true;
    }

    private void insertTextToCarat(string txt) {
        int textIndex = p_CaratPosition + 1;
        string left = p_Text.Substring(0, textIndex);
        string right = p_Text.Substring(textIndex);
        p_Text = left + txt + right;
        p_CaratPosition += txt.Length;
        p_Invalidate = true;
    }

    public string Text {
        get { return p_Text; }
        set {
            p_Text = value;

            /*set Carat location to the end of the string*/
            p_CaratPosition = value.Length - 1;

            p_Invalidate = true;
        }
    }
    public Font Font {
        get { return p_Font; }
        set {
            p_Font = value;
            p_Invalidate = true;
        }
    }
    public Brush ForeBrush {
        get { return p_ForeBrush; }
        set { p_ForeBrush = value; }
    }


    [DllImport("user32.dll")]
    static extern char MapVirtualKey(uint vkey, uint map);
    [DllImport("user32.dll")]
    public static extern int ToUnicode(
        uint wVirtKey,
        uint wScanCode,
        byte[] lpKeyState,
        [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] 
            StringBuilder pwszBuff,
        int cchBuff,
        uint wFlags);

    [DllImport("user32.dll")]
    public static extern bool GetKeyboardState(byte[] lpKeyState);
}
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
using System.Text;

public class HotloaderParserException : Exception {
    private int p_Line;
    private int p_Column;
    private HotloaderFile p_File;

    public HotloaderParserException(HotloaderFile file, int line, int column, string message) : base(message) {
        p_Line = line;
        p_Column = column;
        p_File = file;
    }

    public int Line { get { return p_Line; } }
    public int Column { get { return p_Column; } }

    public override string Message {
        get {
            return String.Format(
                "Error on line {0}, column {1}: {2}",
                p_Line,
                p_Column,
               base.Message);
        }
    }

    public void Print(TextWriter writer) {
        #region grab lines
        FileStream stream = p_File.Lock();
        long oldPosition = stream.Position;
        byte[] raw = new byte[stream.Length];
        stream.Position = 0;
        stream.Read(raw, 0, raw.Length);
        stream.Position = oldPosition;
        p_File.Unlock();

        string rawString = Encoding.ASCII.GetString(raw);
        string[] lines = rawString.Split('\n');
        #endregion

        //print the error
        writer.WriteLine(base.Message);

        //since we want to print out the
        //lines before and after the one
        //with the error, we need to 
        //do some bound checking!
        const int range = 2;
        int errorIndex = p_Line - 1;
        int linesStart = errorIndex - range + 1;
        int linesEnd = errorIndex + range;
        if (linesStart < 0) { linesStart = 0; }
        if (linesEnd >= lines.Length) { linesEnd = lines.Length - 1; }

        //loop through every line to print
        for (int c = linesStart; c <= linesEnd; c++) {
            string prefix = "   Line " + (c + 1) + ": ";
            string line = prefix + lines[c];

            //print the line
            processLine(ref line);   
            writer.WriteLine(line);  

            //is this the error line?
            if (c == errorIndex) {                
                //count how many tabs we have so we can offset accordingly.
                int tabCount = 0;
                string lineOriginal = lines[c];
                foreach (char x in lineOriginal) {
                    if (x == '\t') { tabCount++; }
                }

                int length = p_Column + tabCount + prefix.Length - 1;
                writer.WriteLine(new string('=', length) + '^');
            }
            else {
                writer.WriteLine(line);
            }
        }

    }

    private void processLine(ref string str) {
        str = str.Replace("\t", "  ")
                 .Replace("\r", "");
    }
}
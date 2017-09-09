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

public class HotloaderParserException : Exception {
    private int p_Line;
    private int p_Column;

    public HotloaderParserException(int line, int column, string message) : base(message) {
        p_Line = line;
        p_Column = column;
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
}
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

public enum HotloaderValueOperator { 
    
    NONE =      0,

    ADD =       1,
    SUBTRACT =  2,
    DIVIDE =    3,
    MULTIPLY =  4,


    MODULUS =   5,

    POWER =     6,

    /*bitwise*/
    AND =       7,
    OR =        8,
    XOR =       9,

    /*boolean*/
    NOT =       10,
}
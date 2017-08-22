using System;


[Flags]
public enum ArrowKey : byte {
    NONE = 0x00,
    UP = 0x01,
    DOWN = 0x02,
    LEFT = 0x04,
    RIGHT = 0x08
}
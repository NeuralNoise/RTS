using System;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct Block {
    [FieldOffset(0)]
    public int SpriteID;

    [FieldOffset(4)]
    public int StateID;

    [FieldOffset(8)]
    public bool Selected;
}

public unsafe struct VisibleBlock {
    public int BlockX;
    public int BlockY;
    
    public int RenderX;
    public int RenderY;

    public Block* Block;
}
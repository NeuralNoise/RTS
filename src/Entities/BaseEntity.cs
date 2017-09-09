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

public abstract class BaseEntity {
    private int p_ID;
    private string p_Name;
    private int p_MaxHP;

    private int p_AttackDamage;
    private int p_StabDefense;
    private int p_BallisticDefense;
    
    /*0-1 out of camera block size*/
    private float p_Width;
    private float p_Height;

    public int ID { get { return p_ID; } }
    public string Name { get { return p_Name; } }

    public int MaxHP { get { return p_MaxHP; } }

    public int AttackDamage { get { return p_AttackDamage; } }
    public int StabDefense { get { return p_StabDefense; } }
    public int BallisticDefense { get { return p_BallisticDefense; } }

    /*these functions are to reduce call overhead when rendering.*/
    public void GetSize(int blockSize, out int width, out int height) {
        width = (int)(p_Width * blockSize);
        height = (int)(p_Height * blockSize);
    }
    public int GetWidth(int blockSize) {
        return (int)(p_Width * blockSize);
    }
    public int GetHeight(int blockSize) {
        return (int)(p_Height * blockSize);
    }
}
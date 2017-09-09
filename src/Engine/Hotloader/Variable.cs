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

public class HotloaderVariable {
    private HotloaderClass p_Parent;
    private HotloaderAccessor p_Accessors;

    private int p_Hash;
    private string p_Name;
    private HotloaderExpression p_Value;
    
    public HotloaderVariable(string name, Hotloader hotloader) {
        p_Name = name;
        p_Hash = name.GetHashCode();
        p_Accessors = HotloaderAccessor.NONE;
        p_Value = new HotloaderExpression(hotloader, this);
    }
    public HotloaderVariable(string name, object value, Hotloader hotloader)
        : this(name, value, HotloaderAccessor.NONE, hotloader) { }
    public HotloaderVariable(string name, object value, HotloaderAccessor accessors, Hotloader hotloader)
        : this(name, hotloader) {
            p_Value.SetValue(value);
            p_Accessors = accessors;
    }

    public string Name { get { return p_Name; } }
    public HotloaderAccessor Accessors {
        get { return p_Accessors; }
        set { p_Accessors = value; }
    }
    public HotloaderClass Parent { get { return p_Parent; } }
    public HotloaderExpression Value { get { return p_Value; } }

    public string FullName {
        get {
            if (p_Parent != null) {
                return p_Parent.FullName + "." + p_Name;
            }
            return p_Name;
        }
    }

    internal void changeParent(HotloaderClass newParent) {
        p_Parent = newParent;
    }

    public override string ToString() {
        return FullName;
    }
    public override int GetHashCode() {
        return p_Hash;
    }
}
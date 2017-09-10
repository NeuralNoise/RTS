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
using System.Collections.Generic;

public class HotloaderClass {
    private List<HotloaderClass> p_ChildClasses;
    private List<HotloaderVariable> p_ChildVariables;

    private Hotloader p_Hotloader;
    private HotloaderClass p_Parent;

    private string p_Name;
    private int p_NameHash;
    private object p_Mutex = new object();

    public HotloaderClass(string name, Hotloader hotloader) {
        p_Name = name;
        p_NameHash = name.GetHashCode();
        p_ChildClasses = new List<HotloaderClass>();
        p_ChildVariables = new List<HotloaderVariable>();
        p_Hotloader = hotloader;
    }

    public bool AddVariable(string name, object value) {
        return AddVariable(
            name,
            value,
            HotloaderAccessor.NONE);
    }
    public bool AddVariable(string name, object value, HotloaderAccessor accessors) {
        return AddVariable(
            new HotloaderVariable(
                name,
                value,
                accessors,
                p_Hotloader));
    }
    public bool AddVariable(string name, HotloaderEvaluationCallback evaluationCallback) {
        return AddVariable(
            new HotloaderVariable(
                    name,
                    evaluationCallback,
                    p_Hotloader));
    }
    public bool AddVariable(HotloaderVariable variable) {
        lock (p_Mutex) {
            int hash = variable.GetHashCode();
            
            //the name cannot contains "."
            if (variable.Name.Contains(".")) {
                return false;
            }

            //does the name already exist?
            if (NameExists(variable.Name)) {
                return false;
            }

            //remove from variables parent
            if (variable.Parent != null) {
                variable.Parent.RemoveVariable(variable);
            }

            //add
            variable.changeParent(this);
            p_ChildVariables.Add(variable);
            return true;
        }
    }

    public bool AddClass(HotloaderClass cls) {
        lock (p_Mutex) {
            //does the name already exist?
            if (NameExists(cls.p_Name)) {
                return false;
            }

            //remove from existing parent
            if (cls.p_Parent != null) {
                cls.p_Parent.RemoveClass(cls);
            }

            cls.p_Parent = this;
            p_ChildClasses.Add(cls);
            return true;
        }
    }

    public bool RemoveClass(HotloaderClass cls) {
        bool result = p_ChildClasses.Remove(cls);
        if (result) {
            cls.p_Parent = null;
        }
        return result;
    }
    public bool RemoveVariable(HotloaderVariable variable) {
        lock (p_Mutex) {
            return p_ChildVariables.Remove(variable);
        }
    }

    public bool NameExists(string name) { 
        //check local
        if (GetClass(name) != null) { return true; }
        if (GetVariable(name) != null) { return true; }

        //check the parent
        if (p_Parent != null) {
            return p_Parent.NameExists(name);
        }

        //it doesn't exist
        return false;
    }

    public HotloaderClass GetClass(string name) {
        int nameHash = name.GetHashCode();
        lock (p_Mutex) {
            foreach (HotloaderClass cls in p_ChildClasses) {
                if (cls.p_NameHash == nameHash) {
                    return cls;
                }
            }
        }

        //not found
        return null;
            
    }
    public HotloaderVariable GetVariable(string name) {
        int nameHash = name.GetHashCode();

        lock (p_Mutex) {
            foreach (HotloaderVariable v in p_ChildVariables) {
                if (v.GetHashCode() == nameHash) {
                    return v;
                }
            }
        }

        //not found!
        return null;
    }

    public HotloaderClass ResolveClass(string fullName) { 
        //split up the fullname to get each class
        string[] split = fullName.Split('.');
        return resolveClass(split, 0, split.Length);
    }
    public HotloaderVariable ResolveVariable(string fullName) {
        string[] split = fullName.Split('.');

        //get the class 
        HotloaderClass cls = resolveClass(split, 0, split.Length - 1);
        if (cls == null) { return null; }

        //get the variable
        return cls.GetVariable(split[split.Length - 1]);
    }

    private HotloaderClass resolveClass(string[] split, int index, int length) {
        //index hits the end?
        if (index == length) { return this; }

        string name = split[index];
        int nameHash = name.GetHashCode();

        //resolve for this instance
        lock (p_Mutex) {
            foreach (HotloaderClass cls in p_ChildClasses) {
                if (cls.p_NameHash == nameHash) {
                    return cls.resolveClass(
                        split,
                        index + 1,
                        length);
                }
            }
        }

        //not found
        return null;

    }

    public HotloaderClass Parent { get { return p_Parent; } }

    public string FullName {
        get {
            if (p_Parent == null) { return p_Name; }
            return p_Parent.FullName + "." + p_Name;
        }
    }

    public override string ToString() {
        return FullName;
    }
}
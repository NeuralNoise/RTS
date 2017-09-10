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
using System.Collections.Generic;


public unsafe partial class Hotloader : IDisposable {
    private Heartbeat p_Heartbeat;
    private List<HotloaderFile> p_Files;
    private object p_Mutex = new object();
    private long p_LastHashCheck;
    private HotloaderClass p_GlobalClass;
    private bool p_ForceUpdate = false;

    public Hotloader() : this(true) { }
    public Hotloader(bool addDefaultClasses) {
        p_GlobalClass = new HotloaderClass("GLOBALS", this);

        /**/
        p_Files = new List<HotloaderFile>();
        p_Heartbeat = new Heartbeat("hotloader");
        p_Heartbeat.Speed(10);
        p_Heartbeat.Start(this, tick);

        if (addDefaultClasses) {
            initDefaults();
        }

        try { AddFile("test.txt"); }
        catch(HotloaderParserException ex) {
            ex.Print(Console.Out);
        }
    }

    public HotloaderFile AddFile(string filename) {
        filename = new FileInfo(filename).FullName;

        lock (p_Mutex) {
            //does the file already exist?
            HotloaderFile buffer = GetFile(filename);
            if (buffer != null) { return buffer; }
            buffer = new HotloaderFile(filename);
            p_Files.Add(buffer);

            //force update
            p_ForceUpdate = true;

            //clean up
            return buffer;
        }
    }

    public bool RemoveFile(string filename) {
        //get the instance to remove
        HotloaderFile file = GetFile(filename);
        if (file == null) { return false; }
        return RemoveFile(file);
    }
    public bool RemoveFile(HotloaderFile file) {
        lock (p_Mutex) { 
            //remove all variables we loaded from
            //this file.
            List<HotloaderVariable> vars = file.Variables;
            foreach (HotloaderVariable v in vars) {
                v.Remove();
            }

            //remove all included files
            List<HotloaderFile> includes = file.Includes;
            foreach (HotloaderFile i in includes) {
                RemoveFile(i);
            }

            //remove
            bool result = p_Files.Remove(file);
            if (!result) { return false; }

            //force update on all files since
            //we might of removed variables
            //that exist elsewhere.
            p_ForceUpdate = true;
            return true;
        }
    }

    public bool HasFile(string filename) {
        return GetFile(filename) != null;
    }
    public HotloaderFile GetFile(string filename) {
        lock (p_Mutex) {
            filename = new FileInfo(filename).FullName;
            filename = filename.ToLower();
            foreach (HotloaderFile f in p_Files) {
                if (f.Filename.ToLower() == filename) {
                    return f;
                }
            }
            return null;
        }

    }

    public object EvaluateExpression(string expression) {
        //escape control characters
        expression = expression.Replace(";", "\";\"");

        //create a variable name that will never occur in normal code.
        string variableName = generateString(new Random(), 20);
        expression = variableName + "=" + expression + ";";

        //convert the expression to just a variable being assigned and grab
        //that from globals.
        byte[] data = Encoding.ASCII.GetBytes(expression);
        fixed (byte* dataPtr = data) {
            byte* ptr = dataPtr;

            //load the expression in
            load(null, ptr, data.Length);
        }

        //get the variable
        HotloaderVariable variable = p_GlobalClass.GetVariable(variableName);
        p_GlobalClass.RemoveVariable(variable);

        //evaluate
        return variable.Value.Evaluate();
    }

    public HotloaderClass Globals { get { return p_GlobalClass; } }

    public void ForceDispose() {
        p_Heartbeat.ForceStop();
    }
    public void Dispose() {
        p_Heartbeat.Stop();
    }

    private void tick(object state) {
        Hotloader hot = (Hotloader)state;

        /*do we check the hash? we do this every second
          instead of every tick.*/
        long hashTime = hot.p_LastHashCheck + TimeSpan.TicksPerSecond;
        long now = DateTime.Now.Ticks;
        bool checkHash = now >= hashTime;
        if (checkHash) {
            hot.p_LastHashCheck = now;
        }

        lock (p_Mutex) {

            List<HotloaderFile> files = hot.p_Files;
            int fileLength = files.Count;

            //keep iterating until the files were not modified!
            bool modified;
            do {
                modified = false;
                for (int c = 0; c < fileLength; c++) {
                    HotloaderFile file = files[c];

                    //does the file exist?
                    if (!File.Exists(file.Filename)) { 
                        //remove it
                        RemoveFile(file);
                        modified = true;
                        fileLength = files.Count;
                        break;
                    }

                    //has the file changed?
                    FileStream fileStream;
                    bool changed = file.HasChanged(out fileStream);

                    //file modified/force update?
                    if (p_ForceUpdate || changed) {

                        try { hot.fileChanged(file, fileStream); }
                        catch(HotloaderParserException ex) {
                            ex.Print(Console.Out);
                        }

                        //a file been removed/added?
                        if (files.Count != fileLength) {
                            modified = true;
                            fileLength = files.Count;
                            fileStream.Close();
                            break;
                        }
                    }

                    //clean up
                    file.Close();
                }
            }
            while(modified);

            p_ForceUpdate = false;
        }

    }
    private void fileChanged(HotloaderFile file, FileStream stream) {
        //read the file into memory
        byte[] data = new byte[stream.Length];
        stream.Position = 0;
        stream.Read(data, 0, data.Length);
        stream.Position = 0;

        
        fixed (byte* ptr = data) {
            load(file, ptr, data.Length);
        }

        //clean up
        data = null;
    }
    private string generateString(Random r, int length) {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
        byte[] block = new byte[length];
        fixed (byte* blockPtr = block) {
            byte* ptr = blockPtr;
            byte* ptrEnd = blockPtr + length;

            while (ptr != ptrEnd) {
                *(ptr++) = (byte)alphabet[r.Next(0, 53)];
            }

            return new string((sbyte*)blockPtr, 0, length);
        }
    }

    public object this[string fullName] {
        get {
            HotloaderVariable var = p_GlobalClass.ResolveVariable(fullName);
            if (var == null) { return null; }
            return var.Value.Evaluate();
        }
    }
}
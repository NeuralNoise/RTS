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

    public Hotloader() {
        p_GlobalClass = new HotloaderClass("GLOBALS");

        /**/
        p_Files = new List<HotloaderFile>();
        p_Heartbeat = new Heartbeat("hotloader");
        p_Heartbeat.Speed(10);
        p_Heartbeat.Start(this, tick);
    }

    public HotloaderFile AddFile(string filename) {
        lock (p_Mutex) {
            HotloaderFile buffer = new HotloaderFile(filename);
            p_Files.Add(buffer);

            //trigger file changed to load the file
            FileStream stream = new FileStream(
                filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None);
            fileChanged(buffer, stream);

            //clean up
            stream.Close();
            return buffer;
        }
    }

    public object EvaluateExpression(string expression) {
        //escape control characters
        expression = expression.Replace(";", "\";\"");

        //create a variable name that will never occur in normal code.
        string variableName = generateString(new Random(), 10);
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
            foreach (HotloaderFile file in hot.p_Files) {
                FileStream stream;
                if (file.HasChanged(out stream)) {
                    hot.fileChanged(file, stream);
                }
                stream.Close();
            }
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

    public HotloaderClass Globals { get { return p_GlobalClass; } }

    public void ForceDispose() {
        p_Heartbeat.ForceStop();
    }
    public void Dispose() {
        p_Heartbeat.Stop();
    }
}
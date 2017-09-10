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
using System.Threading;
using System.Collections.Generic;

public class HotloaderFile {
    private string p_Filename;
    private int p_Hash;
    private long p_LastModified;
    private long p_HashLastChecked;
    private FileStream p_Stream;
    private object p_Mutex = new object();

    private List<HotloaderFile> p_Included = new List<HotloaderFile>();
    private List<HotloaderVariable> p_Variables = new List<HotloaderVariable>();

    public HotloaderFile(string filename) { 
        //does the file exist?
        if (!File.Exists(filename)) {
            throw new Exception("File \"" + filename + "\"does not exist!");
        }

        p_Filename = filename;
        p_HashLastChecked = DateTime.Now.Ticks;

        //get the file info
        FileStream fs = new FileStream(
            filename, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.None);
        FileInfo info = new FileInfo(filename);
        p_Hash = getHash(fs);
        p_LastModified = info.LastWriteTime.Ticks;

        //clean up
        fs.Close();
        
    }

    public bool HasChanged(out FileStream fileStream) { 
        //use a file stream to lock the file        
        FileStream fileLock = Open();

        //get when the file was last modified
        FileInfo info = new FileInfo(p_Filename);
        long lastModified = info.LastWriteTime.Ticks;

        //do we check the hash?
        long now = DateTime.Now.Ticks;
        bool checkHash = now >= p_HashLastChecked + TimeSpan.TicksPerSecond;
        
        //has it been modified?
        bool modified = false;
        int newHash = 0;
        if (lastModified != p_LastModified) {
            modified = true;
        }
        //compare hash?
        if (checkHash || modified) {
            newHash = getHash(fileLock);
            if (newHash != p_Hash) {
                modified = true;
            }
        }

        //update info if it has been changed!
        if (modified) {
            p_Hash = newHash;
            p_LastModified = lastModified;
            p_HashLastChecked = now;
        }

        fileStream = fileLock;
        return modified;
    }

    public string Filename { get { return p_Filename; } }

    public FileStream Open() {
        //wait until we can get a lock on the file!
        lock (p_Mutex) {
            if (p_Stream != null) { return p_Stream; }

            while (true) {
                try {
                    p_Stream = new FileStream(
                        p_Filename,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None);
                    return p_Stream;
                }
                catch { }
            }
        }
    }
    public void Close() {
        lock (p_Mutex) {
            p_Stream.Close();
            p_Stream.Dispose();
            p_Stream = null;
        }
    }

    public FileStream Lock() {
        Monitor.Enter(p_Mutex);
        return p_Stream;
    }
    public void Unlock() {
        Monitor.Exit(p_Mutex);
    }

    public List<HotloaderVariable> Variables { get { return p_Variables; } }
    public List<HotloaderFile> Includes { get { return p_Included; } }

    public override int GetHashCode() {
        return p_Hash;
    }

    private int getHash(FileStream stream) {
        /*
            just return the hashed string of the file data 
        */
        byte[] data = new byte[stream.Length];
        stream.Position = 0;
        stream.Read(data, 0, data.Length);
        stream.Position = 0;

        string text = Encoding.ASCII.GetString(data);
        int hash = text.GetHashCode();
        data = null;
        text = null;
        return hash;
    }

    internal void setIncludes(List<HotloaderFile> f) {
        p_Included = null;
        p_Included = f;
    }
    internal void setVariables(List<HotloaderVariable> v) {
        p_Variables = null;
        p_Variables = v;
    }

}
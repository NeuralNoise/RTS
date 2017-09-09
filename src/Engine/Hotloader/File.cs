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

public class HotloaderFile {
    private string p_Filename;
    private int p_Hash;
    private long p_LastModified;
    private long p_HashLastChecked;
    

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
        FileStream fileLock = null;
        while (fileLock == null) {
            //there might be an application
            //that is writing to the file
            //at the same time.
            try {
                fileLock = new FileStream(
                    p_Filename,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None);
            }
            catch { }
        }

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

    public string Filename { get { return p_Filename; } }

    public override int GetHashCode() {
        return p_Hash;
    }
}
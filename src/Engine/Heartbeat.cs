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
using System.Windows.Forms;
using System.Threading;


public class Heartbeat {
    private long p_LastTime;
    private ulong p_TotalFrames;
    private double p_LastLatency;
    private double p_TotalLatency;
    private double p_FrameRate;
    private long p_FrameSecond;
    private long p_Second;

    private bool p_StopVerify;
    private bool p_Stop;
    private int p_SleepInterval;
    private Thread p_Thread;

    private string p_Name;
    public Heartbeat(string name) {
        p_Name = "HEARTBEAT: " + name;
    }

    public void Start(object state, BeatCallback callback) {
        if (p_Thread != null) {
            Stop();
        }        
        p_Stop = false;
        
        p_Thread = new Thread(new ParameterizedThreadStart(beat)) { 
            Name = p_Name
        };
        p_Thread.Start(new stateStruct { 
            state = state,
            callback = callback
        });
    }
    public void Stop() {

        p_Stop = true;
        while (!p_StopVerify) ;
        p_Thread.Abort();
        p_Thread = null;


        Console.WriteLine("Thread: " + p_Name + " has stopped!");
    }
    public void ForceStop() {
        p_Thread.Abort();
        p_Thread = null;
    }

    public void Speed(int interval) {
        p_SleepInterval = interval;
    }
    
    private void beat(object s) {
        stateStruct st = (stateStruct)s;

        p_StopVerify = false;
        while (!p_Stop) {            
            long callbackStart = DateTime.Now.Ticks;
            st.callback(st.state);

            long nowTicks = DateTime.Now.Ticks;
            p_TotalFrames++;
            p_FrameSecond++;
            p_LastLatency = (nowTicks - callbackStart) * 1.0f / TimeSpan.TicksPerMillisecond;
            p_TotalLatency += p_LastLatency;
            p_LastTime = nowTicks;

            //has one second passed?
            if (nowTicks - p_Second >= (TimeSpan.TicksPerSecond)) {
                p_FrameRate = p_FrameSecond;
                p_FrameSecond = 0;
                p_Second = nowTicks;
            }

            if (p_SleepInterval != -1) {
                Thread.Sleep(p_SleepInterval);
            }
        }

        Console.WriteLine("Thread " + p_Name + " is stopping!");
        p_StopVerify = true;
    }

    public double Rate { get { return p_FrameRate; } }
    public ulong TotalFrames { get { return p_TotalFrames; } }
    public double LastLatency { get { return p_LastLatency; } }
    public double AverageLatency {
        get {
            return p_TotalLatency / p_TotalFrames;
        }
    }

    public delegate void BeatCallback(object state);

    private struct stateStruct {
        public BeatCallback callback;
        public object state;
    }
}
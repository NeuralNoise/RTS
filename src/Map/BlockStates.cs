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

public class BlockStates<T> {
    private object p_Mutex = new object();
    private Stack<int> p_Available = new Stack<int>();
    private List<T> p_States = new List<T>();
    
    public int RegisterState(T state) {
        lock (p_Mutex) {
            //indexes available?
            if (p_Available.Count != 0) {
                int available = p_Available.Pop();
                p_States[available] = state;
                return available;

            }
            else {
                p_States.Add(state);
                return p_States.Count - 1;
            }
        }
    }
    public void Remove(int stateIndex) { 
        //exist?
        if (stateIndex >= p_States.Count) {
            throw new Exception("State does not exist");
        }

        //remove
        lock (p_Mutex) {
            p_States[stateIndex] = default(T);
            p_Available.Push(stateIndex);
        }
    }

    public T Resolve(int stateIndex) {
        return p_States[stateIndex];
    }

    public bool Has(int stateIndex) {
        return 
            stateIndex >= 0 && 
            stateIndex < p_States.Count;
    }

    public T this[int stateIndex] {
        get {
            return p_States[stateIndex];
        }
        set {
            p_States[stateIndex] = value;
        }
    }
}
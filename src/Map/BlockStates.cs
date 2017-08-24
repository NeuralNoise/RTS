/* 
 * This file is part of the RTS distribution (https://github.com/tomwilsoncoder/RTS)
 * 
 * This program is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
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
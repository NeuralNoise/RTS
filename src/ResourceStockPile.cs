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
public static class ResourceStockPile {

    private static resource[] p_Resources = new resource[0];
    private static object p_Mutex = new object();

    public static int RegisterResource(string name) {
        lock (p_Mutex) {
            Array.Resize(ref p_Resources, p_Resources.Length + 1);
            p_Resources[p_Resources.Length - 1] = new resource { 
                name = name,
                amount = 0
            };
            return p_Resources.Length - 1;
        }
    }

    public static bool CollectFromBlock(Map map, Block block, double amount) {
        if (block.StateID == -1) { return false; }

        //get the block state
        ResourceState state = map.Resources.Resolve(block.StateID);
        
        //can we take this away?
        if (amount > state.Amount) { return false; }
        
        //collect
        lock (p_Mutex) {
            p_Resources[state.ResourceID].amount += state.Amount;
        }
        return true;
    }

    public static double GetAmount(int resourceID) {
        lock (p_Mutex) {
            return p_Resources[resourceID].amount;
        }
    }
    public static string GetName(int resourceID) {
        lock (p_Resources) {
            return p_Resources[resourceID].name;
        }
    }

    private class resource {
        public string name;
        public double amount;
    }

}
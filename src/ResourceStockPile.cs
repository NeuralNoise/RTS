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
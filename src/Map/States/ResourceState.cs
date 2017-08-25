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


public class ResourceState {
    private int p_ResourceID;
    private double p_Amount;
    
    public ResourceState(int resourceID, double amount) {
        p_Amount = amount;
        p_ResourceID = resourceID;
    }

    public double Amount { get { return p_Amount; } }
    public int ResourceID { get { return p_ResourceID; } }

    public bool Take(double amount) {
        //verify
        if (p_Amount - amount < 0 || amount < 0) { return false; }

        p_Amount -= amount;
        return true;
    }
    public void Give(double amount) {
        if (amount < 0) { return; }
        p_Amount += amount;
    }

    public void Change(int newResourceID, double newAmount) {
        p_Amount = newAmount;
        p_ResourceID = newResourceID;
    }
}
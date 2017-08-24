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
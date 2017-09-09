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

public class HotloaderValueOperand {
    private object p_Raw;
    private HotloaderValueType p_Type;
    private HotloaderClass p_Globals;
    private int p_Line;
    private int p_Column;
    private HotloaderVariable p_Variable;

    public HotloaderValueOperand(HotloaderClass globals, int line, int column, 
                                 HotloaderVariable variable,
                                 object raw, HotloaderValueType type) {
        p_Raw = raw;
        p_Type = type;
        p_Globals = globals;
        p_Line = line;
        p_Column = column;
        p_Variable = variable;
    }

    public object GetValue(out HotloaderValueType type) {
        type = p_Type;

        
        //evaluation?
        HotloaderExpression eval = p_Raw as HotloaderExpression;
        if (eval != null) {
            return eval.Evaluate(out type);
        }

        #region variable?
        if (p_Type == HotloaderValueType.VARIABLE) {             
            //resolve the variable
            HotloaderVariable variable = p_Variable.Parent.ResolveVariable((string)p_Raw);
            if (variable == null) {
                variable = p_Globals.ResolveVariable((string)p_Raw);
            }
            if (variable == null) {
                throw new HotloaderParserException(
                    p_Line,
                    p_Column,
                    String.Format(
                        "Variable \"{0}\" does not exist",
                        p_Raw));
            }

            //verify no cross-referencing
            if (variable == p_Variable) {
                throw new HotloaderParserException(
                    p_Line,
                    p_Column,
                    "Cycle-dependancy found! Cannot self-reference");
            }

            //evaluate
            object value = variable.Value.Evaluate(out type);
            return value;
        }
        #endregion

        //this should never happen.
        return p_Raw;
    }


    public int Line { get { return p_Line; } }
    public int Column { get { return p_Column; } }
}
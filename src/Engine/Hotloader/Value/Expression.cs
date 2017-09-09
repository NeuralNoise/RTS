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
using System.Threading;

public class HotloaderExpression {
    private Hotloader p_Hotloader;
    private HotloaderClass p_Globals;
    private HotloaderExpression p_Parent;
    
    private HotloaderValueOperator[] p_Operators;
    private HotloaderValueOperand[] p_Operands;

    private HotloaderVariable p_Variable;

    private GetValueCallback p_Callback;

    private object p_Mutex = new object();
   
    public HotloaderExpression(Hotloader hotloader, HotloaderVariable variable) {
        p_Hotloader = hotloader;
        p_Globals = hotloader.Globals;
        p_Variable = variable;

        p_Operators = new HotloaderValueOperator[0];
        p_Operands = new HotloaderValueOperand[0];
    }

    public void AddOperator(HotloaderValueOperator op) {
        lock (p_Mutex) {
            Array.Resize(ref p_Operators, p_Operators.Length + 1);
            p_Operators[p_Operators.Length - 1] = op;
        }
    }
    public HotloaderValueOperand AddOperand(object raw, HotloaderValueType type, int line, int column) {
        lock (p_Mutex) {
           HotloaderValueOperand buffer = new HotloaderValueOperand(
                p_Globals,
                line,
                column,
                p_Variable,
                raw,
                type);

           Array.Resize(ref p_Operands, p_Operands.Length + 1);
           p_Operands[p_Operands.Length - 1] = buffer;
           return buffer;
        }    
    }

    public object Evaluate() {
        HotloaderValueType t;
        return Evaluate(out t);
    }
    public object Evaluate(out HotloaderValueType type) {
        type = HotloaderValueType.NONE;

        if (p_Callback != null) {
            return p_Callback(this);
        }

        Monitor.Enter(p_Mutex);

        //check for valid evaluation
        if (!Valid) {
            Monitor.Exit(p_Mutex);
            throw new Exception("Invalid evaluation");
        }

        #region evaluate each operand
        int operandLength = p_Operands.Length;
        object[] operands = new object[operandLength];
        HotloaderValueType[] types = new HotloaderValueType[operandLength];
        for (int c = 0; c < operandLength; c++) {
            HotloaderValueOperand opand = p_Operands[c];
            HotloaderValueType t;
            operands[c] = opand.GetValue(out t);
            types[c] = t;

            //should this type be the return type?
            if ((int)t > (int)type) {
                type = t;
            }

            //boolean? if so, we do not
            //allow operators on bools
            if (t == HotloaderValueType.BOOLEAN &&
               operandLength != 1) {
                   Monitor.Exit(p_Mutex);
                   throw new HotloaderParserException(
                       opand.Line,
                       opand.Column,
                       "Booleans cannot be operands");
            }
        }
        #endregion

        //just 1 operand?
        if (operandLength == 1) {
            type = types[0];
            Monitor.Exit(p_Mutex);
            return operands[0];
        }


        object buffer = toType(operands[0], types[0], type);

        //perform each operator
        int opLength = p_Operators.Length;
        for (int c = 0; c < opLength; c++) {
            HotloaderValueOperator op = p_Operators[c];
            object operand = operands[c + 1];
            HotloaderValueType operandType = types[c +1];

            operand = toType(
                operand,
                operandType,
                type);

            #region just a string concat?
            if (type == HotloaderValueType.STRING) { 
                //must be add
                if (op != HotloaderValueOperator.ADD) {
                    Monitor.Exit(p_Mutex);
                    throw new HotloaderParserException(
                        p_Operands[c + 1].Line,
                        p_Operands[c + 1].Column,
                        "Cannot perform operation on a string");
                }

                //add
                buffer = (string)buffer + (string)operand;
                continue;
            }
            #endregion

            #region numeric operand

            #region bitwise
            //bitwise?
            bool isBitwise =
                op == HotloaderValueOperator.AND ||
                op == HotloaderValueOperator.OR ||
                op == HotloaderValueOperator.XOR;

            //if it's bitwise, convert both sides to an int then convert back once
            //the bitwise has been done
            if (isBitwise) {
                long a = (long)toType(buffer, type, HotloaderValueType.NUMERICAL);
                long b = (long)toType(operand, type, HotloaderValueType.NUMERICAL);
                long result = 0;
                switch (op) { 
                    case HotloaderValueOperator.AND:
                        result = a & b;
                        break;
                    case HotloaderValueOperator.OR:
                        result = a | b;
                        break;
                    case HotloaderValueOperator.XOR:
                        result = a ^ b;
                        break;
                }
                buffer = toType(result, HotloaderValueType.NUMERICAL, type);
                continue;
            }
            #endregion

            //just convert the operands to a double then do the maths on that.
            double d1 = (double)toType(buffer, type, HotloaderValueType.DECIMAL);
            double d2 = (double)toType(operand, type, HotloaderValueType.DECIMAL);
            double res = 0;
            switch (op) { 
                case HotloaderValueOperator.ADD:
                    res = d1 + d2;
                    break;
                case HotloaderValueOperator.SUBTRACT:
                    res = d1 - d2;
                    break;
                case HotloaderValueOperator.MULTIPLY:
                    res = d1 * d2;
                    break;
                case HotloaderValueOperator.DIVIDE:
                    res = d1 / d2;
                    break;
                case HotloaderValueOperator.MODULUS:
                    res = d1 % d2;
                    break;
                case HotloaderValueOperator.POWER:
                    res = Math.Pow(d1, d2);
                    break;
            }

            buffer = toType(res, HotloaderValueType.DECIMAL, type);
            #endregion
        }



        Monitor.Exit(p_Mutex);
        return buffer;
    }

    public void SetValue(object value) {
        lock (p_Mutex) { 
            //deturmine what value type to fit in
            HotloaderValueType type = HotloaderValueType.NONE;

            if (   
                   value is sbyte ||
                   value is byte ||
                   value is ushort ||
                   value is short ||
                   value is uint ||
                   value is int ||
                   value is long ||
                   value is ulong) {
                type = HotloaderValueType.NUMERICAL;    
            }
            else if (
                  value is float ||
                  value is double ||
                  value is decimal) {
                type = HotloaderValueType.DECIMAL;
            }
            else if (value is bool) {
                type = HotloaderValueType.BOOLEAN;
            }
            else if (value is string) {
                type = HotloaderValueType.STRING;
            }
            else {
                throw new Exception("Type \"" + value.GetType().FullName + "\" is not supported");
            }
           
            //
            Clear();
            AddOperand(
                value,
                type,
                -1, -1);
        }
    }
    public void SetCallback(GetValueCallback callback) {
        p_Callback = callback;
    }

    public bool Empty {
        get {
            return p_Operands.Length == 0;
        }
    }
    public void Clear() {
        lock (p_Mutex) {
            p_Operands = new HotloaderValueOperand[0];
            p_Operators = new HotloaderValueOperator[0];
        }
    }

    public object Value {
        get { return Evaluate(); }
        set {
            SetValue(value);
        }
    }

    public HotloaderVariable Variable { get { return p_Variable; } }
    public int Operands { get { return p_Operands.Length; } }
    public int Operators { get { return p_Operators.Length; } }

    private object toType(object value, HotloaderValueType sourceType, HotloaderValueType destType) {
        //same type?
        if (sourceType == destType) {
            return value;
        }


        switch (destType) { 
            case HotloaderValueType.DECIMAL:
                return Convert.ToDouble(value);
            
            case HotloaderValueType.NUMERICAL:
                return Convert.ToInt64(value);

            case HotloaderValueType.STRING:
                return Convert.ToString(value);

            default:
                throw new Exception("Cannot convert");
        }

    }

    public HotloaderExpression Parent {
        get { return p_Parent; }
        set { p_Parent = value; }
    }

    public bool Valid {
        get { 
            //blank?
            int opLength = p_Operators.Length;
            int anLength = p_Operands.Length;
            if (opLength == 0 && anLength == 0) {
                return true;
            }

            //operators must be 1 less than the operand length
            //for every 2 operands, we have 1 operator.
            return
                opLength ==
                anLength - 1;

        }
    }

    public delegate object GetValueCallback(HotloaderExpression expression);
}
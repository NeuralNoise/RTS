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
using System.Text;
using System.Collections.Generic;

public unsafe partial class Hotloader {
    private void load(HotloaderFile file, byte* data, int length) {
        byte* ptr = data;
        byte* ptrEnd = data + length;

        byte* blockStart = ptr;
        byte* blockEnd = ptr;

        bool negativeFlag = false;

        //keep track of what classes/variables are being added.
        List<HotloaderClass> fileClasses = new List<HotloaderClass>();
        List<HotloaderVariable> fileVariables = new List<HotloaderVariable>();

        //
        HotloaderClass currentClass = p_GlobalClass;
        HotloaderVariable currentVariable = null;
        HotloaderExpression currentExpression = null;

        //define what mode we are in (what type of block
        //we are reading).
        parserMode mode = parserMode.NONE;

        //
        HotloaderAccessor currentAccessor = HotloaderAccessor.NONE;

        //where we are in a more usable way.
        int x = 0, y = 0;

        while (ptr != ptrEnd) {
            byte current = *(ptr++);
            x++;

            //are we at the end of the file?
            bool atEnd = ptr == ptrEnd;

            //define what current line and column we are on
            int currentColumn = x;
            int currentLine = y + 1;

            #region control characters
            //newline?
            bool newLine =
                current == '\n' ||
                current == '\r';
            if (newLine) {
                x = 0;
                y++;
            }

            //whitespace
            bool whitespace =
                newLine ||
                current == ' ' ||
                current == '\t';

            //alpha numeric? 
            //(only make the call if
            //we know the character is not a whitespace.
            bool nameChar =
                !whitespace &&
                isNameCharacter(current);
            #endregion
                        
            #region byte block evaluation
            bool evaluateBlock = atEnd || whitespace || !nameChar;

            if (evaluateBlock) {
                //if we are at the end, make sure
                //we include the last character
                //in the block.
                if (atEnd && !whitespace && nameChar) { blockEnd++; }
                
                //is the block blank?
                bool isBlank = blockStart == blockEnd;
                int blockLength = (int)(blockEnd - blockStart);

                //read the block as a string
                if (!isBlank) {
                    handleBlock(
                        fileClasses,
                        fileVariables,
                        file,
                        blockStart,
                        blockEnd,
                        currentLine,
                        currentColumn - blockLength + 1,
                        ref negativeFlag,
                        ref mode,
                        ref currentAccessor,
                        ref currentVariable,
                        ref currentExpression,
                        ref currentClass);

                    //block been changed outside pointer?
                    if (blockStart > ptr) {
                        ptr = blockStart;
                    }
                }

                //reset
                blockStart = blockEnd = ptr;
                
                //do not process anything else if it is a whitespace
                if (whitespace) {
                    continue;
                }
            }
            #endregion

            #region comment
            if (current == '#') { 
                //skip over line
                while (ptr != ptrEnd && *(ptr++) != '\n') ;
                blockStart = blockEnd = ptr;
                continue;
            }
            #endregion

            #region string literal
            if (current == '"') {
                //valid?
                if (mode != parserMode.ASSIGNMENT) {
                    throw new HotloaderParserException(
                        currentLine,
                        currentColumn,
                        "Unexpected string literal");
                }

                byte* literalStart = ptr;
                if (!readStringLiteral(ref ptr, ptrEnd)) {
                    throw new HotloaderParserException(
                        currentLine,
                        currentColumn,
                        "String literal did not terminate");
                }

                //deturmine where the string ends
                byte* literalEnd = ptr - 1;

                //read the string and remove 
                //string terminating characters
                string read = readString(literalStart, literalEnd);
                read = read.Replace("\\", "");

                //add operand
                currentExpression.AddOperand(
                    read,
                    HotloaderValueType.STRING,
                    currentLine,
                    currentColumn);

                //update line position
                currentColumn += read.Length - 1;

                blockStart = blockEnd = ptr;
                continue;
            }
            #endregion

            #region expression scopes

            if (current == '(' || current == ')') { 
                //valid?
                if (mode != parserMode.ASSIGNMENT) {
                    throw new HotloaderParserException(
                        currentLine,
                        currentColumn,
                        "Unexpected expression character");
                }

                //close?
                if (current == ')') { 
                    //can we close?
                    if (currentExpression.Parent == null) {
                        throw new HotloaderParserException(
                            currentLine,
                            currentColumn,
                            "Unexpected end of expression scope");
                    }

                    //add the expression as an operand
                    HotloaderExpression parent = currentExpression.Parent;
                    HotloaderExpression expression = currentExpression;
                    HotloaderValueOperand op = parent.AddOperand(
                        expression, 
                        HotloaderValueType.EVALUATION, 
                        currentLine, 
                        currentColumn);

                    //close
                    currentExpression = currentExpression.Parent;
                }

                //open?
                if (current == '(') {
                    HotloaderExpression newExpression = new HotloaderExpression(this, currentVariable);
                    newExpression.Parent = currentExpression;
                    currentExpression = newExpression;
                }

                //
                blockStart = blockEnd = ptr;
                continue;
            }

            #endregion

            #region operators
            #region class
            if (current == ':') { 
                //valid?
                if (mode != parserMode.NONE) {
                    throw new HotloaderParserException(
                        currentLine,
                        currentColumn,
                        "Unexpected class symbol");
                }

                mode = parserMode.CLASS;
                blockStart = blockEnd = ptr;
                continue;
            }
            #endregion

            #region assignment
            if (current == '=') { 
                //valid?
                if (mode != parserMode.VARIABLE) {
                    throw new HotloaderParserException(
                        currentLine,
                        currentColumn,
                        "Unexpected assignment operator");
                }
                mode = parserMode.ASSIGNMENT;
                blockStart = blockEnd = ptr;
                continue;
            }
            #endregion

            #region end assignment
            if (current == ';') { 
                //valid?
                if (mode != parserMode.ASSIGNMENT ||
                    !currentExpression.Valid ||
                    currentExpression.Parent != null ||
                    currentExpression.Empty) {
                    throw new HotloaderParserException(
                        currentLine,
                        currentColumn,
                        "Unexpected end-of-expression character");
                }

                mode = parserMode.NONE;
                blockStart = blockEnd = ptr;

                currentExpression = null;
                currentVariable = null;
                continue;
            }
            #endregion

            #region maths
            HotloaderValueOperator mathOp = HotloaderValueOperator.NONE;
            switch ((char)current) {
                case '+': mathOp = HotloaderValueOperator.ADD; break;
                case '-': mathOp = HotloaderValueOperator.SUBTRACT; break;
                case '*': mathOp = HotloaderValueOperator.MULTIPLY; break;
                case '/': mathOp = HotloaderValueOperator.DIVIDE; break;
                case '^': mathOp = HotloaderValueOperator.POWER; break;
                case '%': mathOp = HotloaderValueOperator.MODULUS; break;
                case '&': mathOp = HotloaderValueOperator.AND; break;
                case '|': mathOp = HotloaderValueOperator.OR; break;
                case '?': mathOp = HotloaderValueOperator.XOR; break;               
            }

            //are we expecting a math operation?
            if (mathOp != HotloaderValueOperator.NONE &&
               mode != parserMode.ASSIGNMENT) {
                   throw new HotloaderParserException(
                       currentLine,
                       currentColumn,
                       String.Format(
                            "Unexpected operator {0}",
                            (char)current));
            }

            //wait, was this negative?
            bool addOp = true;
            if (mathOp == HotloaderValueOperator.SUBTRACT) { 
                //make sure there would be an operand before
                //the subtract. Otherwise we assume it's a 
                //negative integer/decimal.
                negativeFlag =
                    currentExpression.Operands ==
                    currentExpression.Operators;
                addOp = false;
            }

            if (mathOp != HotloaderValueOperator.NONE) {
                //if we have discovered a negate operator
                //do not add this as a maths operator!
                if (addOp) {
                    currentExpression.AddOperator(mathOp);                
                }

                blockStart = blockEnd = ptr;
                continue;
            }
            #endregion
            #endregion

            //invalid character?
            if (!nameChar) {
                throw new HotloaderParserException(
                    currentLine,
                    currentColumn,
                    String.Format(
                        "Invalid character {0}",
                        (char)current));
            }

            //incriment block end to include this
            //byte so later we can evaluate blocks
            //of the file.
            blockEnd++;
        }

        //not ended correctly?
        if (currentClass.Parent != null) {
            throw new HotloaderParserException(
                -1,
                -1,
                "Class not terminated");
        }

        return;
    }

    private void handleBlock(
                             List<HotloaderClass> classes,
                             List<HotloaderVariable> variables,
                             HotloaderFile file, 
                             byte* blockPtr, byte* blockEnd,
                             int currentLine, int currentColumn,
                             ref bool negativeFlag,
                             ref parserMode mode,               
                             ref HotloaderAccessor currentAccessor, 
                             ref HotloaderVariable currentVariable,
                             ref HotloaderExpression currentExpression,
                             ref HotloaderClass currentClass) { 
        
        //read the block as a string
        string block = readString(blockPtr, blockEnd);

        //hash the string so we can do quick compares with keywords
        int hash = block.GetHashCode();

        #region reserve word check
        /*since we detect errors if some keywords
         are used anyway, some are discarded here.*/
        if (mode != parserMode.ASSIGNMENT) {

            if (hash == STRING_TRUE_HASH ||
               hash == STRING_FALSE_HASH) {

                   throw new HotloaderParserException(
                       currentLine,
                       currentColumn,
                       String.Format(
                            "Unexpected use of symbol \"{0}\"",
                            block));
            }

        }
        #endregion

        #region accessors

        bool isConst = hash == STRING_CONST_HASH;
        bool isStatic = hash == STRING_STATIC_HASH;

        //valid mode?
        if (mode != parserMode.NONE) {
            if (isConst || isStatic) {
                throw new HotloaderParserException(
                    currentLine,
                    currentColumn,
                    "Unexpected accessor");
            }
        }


        if (isConst) { currentAccessor |= HotloaderAccessor.CONST; }
        if (isStatic) { currentAccessor |= HotloaderAccessor.STATIC; }

        if (isConst || isStatic) {
            return;
        }
        #endregion

        #region end of a class?
        if (hash == STRING_END_HASH) { 
            //verify that we can end a class
            if (currentClass.Parent == null ||
                mode != parserMode.NONE) {
                throw new HotloaderParserException(
                    currentLine,
                    currentColumn,
                    "Unexpected end of class token.");
            }

            //end the class by just setting
            //the current class to the parent
            currentClass = currentClass.Parent;
            return;
        }
        #endregion

        #region class declaration

        //are we expecting a class name?
        if (mode == parserMode.CLASS) {
            //a class cannot have accessors!
            if (currentAccessor != HotloaderAccessor.NONE) {
                throw new HotloaderParserException(
                    currentLine,
                    currentColumn,
                    "Classes cannot have accessors");
            }

            //get the class
            HotloaderClass cls = currentClass.GetClass(block);
            if (cls == null) {
                cls = new HotloaderClass(block);

                //only reason this will return false is 
                //if it already exists!
                if (!currentClass.AddClass(cls)) {
                    throw new HotloaderParserException(
                        currentLine,
                        currentColumn,
                        String.Format(
                            "Cannot declare class \"{0}\". Name already used elsewhere.",
                            block));
                }
            }
            

            classes.Add(cls);

            //set the current class to the newly created one
            //so every variable/class added will be added 
            //to this one.
            currentClass = cls;

            //we are no longer reading a class
            mode = parserMode.NONE;
            return;
        }

        #endregion

        #region variable declaration

        //we must be in assignment mode by now..
        if (mode == parserMode.VARIABLE) {
            throw new HotloaderParserException(
                currentLine,
                currentColumn,
                "Unexpected variable declaration");
        }

        //we assume this is an alpha-numerical string
        if (mode == parserMode.NONE) {
            
            //does this variable already exist?
            //if not, add it.
            currentVariable = currentClass.GetVariable(block);
            if (currentVariable == null) {
                //if the add function returns false, then
                //the variable name is already taken!
                currentVariable = new HotloaderVariable(block, this);
                if (!currentClass.AddVariable(currentVariable)) {
                    throw new HotloaderParserException(
                        currentLine,
                        currentColumn,
                        String.Format(
                            "Cannot declare variable \"{0}\". Name already used elsewhere.",
                            block));
                }
            }

            //add to the list of added variables from the file
            variables.Add(currentVariable);

            //if we can't change this variable (it's marked as static
            //just set the current variable to a dummy one.
            if ((currentVariable.Accessors & HotloaderAccessor.STATIC) == HotloaderAccessor.STATIC) {
                currentVariable = new HotloaderVariable("dummy", this);
                currentVariable.changeParent(currentClass);
            }

            //set expression
            currentExpression = currentVariable.Value;
            currentExpression.Clear();

            //set accessors
            currentVariable.Accessors = currentAccessor;
            currentAccessor = HotloaderAccessor.NONE;

            //wait for assignment etc..
            mode = parserMode.VARIABLE;
        }

        #endregion

        #region variable assignment

        if (mode == parserMode.ASSIGNMENT) {
            #region boolean
            string blockLower = block.ToLower();

            if (blockLower == "true" ||
                blockLower == "false") {

                   currentExpression.AddOperand(
                       (block == "true"),
                       HotloaderValueType.BOOLEAN,
                       currentLine,
                       currentColumn);
                   return;
            }
            #endregion

            #region numerical?

            double decimalValue;
            bool isDecimal = Double.TryParse(block, out decimalValue);
            if (isDecimal) {
                //is it an actual decimal or integer?
                bool explicitDecimal = block.Contains(".");

                //negate?
                if (negativeFlag) {
                    decimalValue = -decimalValue;
                    negativeFlag = false;
                }

                //deturmine value type
                HotloaderValueType type =
                    explicitDecimal ?
                        HotloaderValueType.DECIMAL :
                        HotloaderValueType.NUMERICAL;

                //deturmine raw object for the string
                object raw = decimalValue;
                if (!explicitDecimal) {
                    raw = (long)decimalValue;
                }

                currentExpression.AddOperand(
                    raw,
                    type,
                    currentLine,
                    currentColumn);
                return;
            }

            #endregion

            #region variable?

            //verify the name
            while (blockPtr != blockEnd) {
                byte current = *(blockPtr++);
                if (!isNameCharacter(current)) {
                    throw new HotloaderParserException(
                        currentLine,
                        currentColumn,
                        String.Format(
                            "Invalid variable name \"{0}\"",
                            block));
                }
            }

            //default to a variable reference
            currentExpression.AddOperand(
                block,
                HotloaderValueType.VARIABLE,
                currentLine,
                currentColumn);
            #endregion
        }

        #endregion                         
    }

    private bool skipWhitespace(ref byte* ptr, byte* ptrEnd, ref int x, ref int y) {
        while (ptr != ptrEnd) {
            byte current = *ptr;

            
            bool whitespace =
                current == ' ' ||
                current == '\t';
            bool newLine = current == '\n';

            //if we hit a new line, update line number
            if (newLine) {
                ptr++;
                x = 0;
                y++;
                continue;
            }

            //not a whitespace?
            if (!whitespace && !newLine) {
                return false;
            }

            //update line position.
            ptr++;
            x++;
            
        }

        //return true if we are at the end of the string.
        return ptr == ptrEnd;

    }

    private bool isNameCharacter(byte value) {

        return
            /*alpha numeric test*/
            (value >= 'A' &&
            value <= 'Z') ||

            (value >= 'a' &&
            value <= 'z') ||

            (value >= '0' &&
            value <= '9') ||

            /*other name valid characters*/
            value == '_' ||
            value == '.';

    }

    private string readString(byte* start, byte* end) { 
        //empty?
        if (end == start) { return ""; }
        
        //flip?
        if (end < start) {
            byte* temp = end;
            end = start;
            start = temp;
        }

        //
        int length = (int)(end - start);
        return new String((sbyte*)start, 0, (int)(end - start));
    }

    private bool readStringLiteral(ref byte* ptr, byte* end) {        
        //define where we substring from
        while (ptr != end) {
            byte current = *(ptr++);

            //control character? if so, skip over the
            //next character
            if (current == '\\') {
                ptr++;
                continue;
            }

            //we found the end?
            if (current == '"') {
                break;
            }
        }
        
        //only return true if we did 
        //not run at the end of the data
        //meaning a string literal was not found!
        return (ptr != end);
    }

    private readonly int STRING_STATIC_HASH = "static".GetHashCode();
    private readonly int STRING_CONST_HASH = "const".GetHashCode();
    private readonly int STRING_END_HASH = "end".GetHashCode();
    private readonly int STRING_TRUE_HASH = "true".GetHashCode();
    private readonly int STRING_FALSE_HASH = "false".GetHashCode();

    [Flags]
    private enum parserMode {
        NONE =       0x00,
        CLASS =      0x01,
        VARIABLE =   0x02,
        NAME =       0x04,
        ASSIGNMENT = 0x08
    }
}
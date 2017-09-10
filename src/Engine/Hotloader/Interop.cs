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
using System.Reflection;

public partial class Hotloader {


    private void initDefaults() {
        AttachManagedClass(typeof(registers), Globals);
        AttachManagedClass(typeof(generalPurpose), Globals);
        AttachManagedClass(typeof(math));
        AttachManagedClass(typeof(time));
    }

    public void AttachManagedClass(Type type) {
        HotloaderClass cls = new HotloaderClass(type.Name, this);
        if (!Globals.AddClass(cls)) {
            cls = Globals.ResolveClass(type.Name);
        }

        AttachManagedClass(type, cls);

    }
    public void AttachManagedClass(Type type, HotloaderClass cls) {

        //attach fields
        FieldInfo[] fields = type.GetFields();
        int fieldCount = fields.Length;
        for (int c = 0; c < fieldCount; c++) {
            FieldInfo field = fields[c];

            //must be public
            if (!field.IsPublic) { continue; }

            BindField(field, cls);
        }

        //attach methods
        MethodInfo[] methods = type.GetMethods();
        int methodCount = methods.Length;
        for (int c = 0; c < methodCount; c++) {
            MethodInfo method = methods[c];

            string name = method.Name;

            //reserved name?
            if (
               name == "GetHashCode" ||
               name == "ToString" ||
               name == "Equals" ||
               name == "GetType") { continue; }

            //bind
            BindMethod(method, cls);
        }
    }

    public void BindField(FieldInfo field, HotloaderClass cls) {
        string name = field.Name;
        HotloaderVariable v = cls.GetVariable(name);
        if (v == null) { 
            v = new HotloaderVariable(name, this);

            if (!cls.AddVariable(v)) {
                throw new Exception(
                    String.Format(
                        "Cannot bind field. Name \"{0}\" already in use.",
                        name));
            }
        }

        BindField(field, v);
    }
    public void BindField(FieldInfo field, HotloaderVariable var) {
        //const?
        bool isConst = field.IsLiteral && !field.IsInitOnly;

        //bind
        HotloaderExpression expression = var.Value;
        if (!isConst) {
            expression.SetAssignmentCallback(delegate(object value) {
                field.SetValue(null, value);
            });
        }
        else {
            expression.SetValue(field.GetValue(null));
        }

        //set accessors
        var.Accessors = (isConst ?
            HotloaderAccessor.CONST | HotloaderAccessor.STATIC :
            HotloaderAccessor.NONE);
    }

    public void BindMethod(MethodInfo method, HotloaderClass cls) {
        string name = method.Name;
        HotloaderVariable v = cls.GetVariable(name);
        if (v == null) { 
            v = new HotloaderVariable(name, this);

            if (!cls.AddVariable(v)) {
                throw new Exception(
                    String.Format(
                        "Cannot bind method. Name \"{0}\" already in use.",
                        name));
            }
        }

        BindMethod(method, v);        
    }
    public void BindMethod(MethodInfo method, HotloaderVariable var) {
        //convert the method to a callback
        //and add it with a variable so it
        //will be called every time it is 
        //accessed.
        HotloaderEvaluationCallback callback = (HotloaderEvaluationCallback)
            Delegate.CreateDelegate(
                typeof(HotloaderEvaluationCallback),
                method);
        var.Value.SetEvaluationCallback(callback);
    }

    private static class registers {
        /*a dummy register used for just calling methods.*/
        public static object call = null;

        /*doubles*/
        public static double x, y, z;

        /*ints*/
        public static long a, b, c, d;

        /*strings*/
        public static string txt1, txt2, txt3;
    }

    private static class generalPurpose {
        private static Random p_Random = new Random();

        public const double pi = Math.PI;
        public const double e = Math.E;

        public static object random() {
            return p_Random.NextDouble();
        }
    }

    private static class math {
        private static Random p_Random = new Random();

        public static object abs() {
            return Math.Abs(registers.x);
        }
        public static object sqrt() {
            return Math.Sqrt(registers.x);
        }

        public static object pow() {
            return Math.Pow(registers.x, registers.y);
        }

        public static object sin() {
            return Math.Sin(registers.x);
        }
        public static object sinh() {
            return Math.Sinh(registers.x);
        }
        public static object sign() {
            return Math.Sign(registers.x);
        }
        public static object cos() {
            return Math.Cos(registers.x);
        }
        public static object cosh() {
            return Math.Cosh(registers.x);
        }

        public static object tan() {
            return Math.Tan(registers.x);
        }
        public static object tanh() {
            return Math.Tanh(registers.x);
        }

        public static object ceiling() {
            return Math.Ceiling(registers.x);
        }
        public static object floor() {
            return Math.Floor(registers.x);
        }

        public static object log() {
            return Math.Log(registers.x, registers.y);
        }
    }
    private static class time {
        public static object date() {
            return DateTime.Now;
        }

        public static object ticksPerMillisecond() {
            return TimeSpan.TicksPerMillisecond;
        }

        public static object dayOfWeek() {
            return DateTime.Now.DayOfWeek;
        }
        public static object year() {
            return DateTime.Now.Year;
        }
        public static object month() {
            return DateTime.Now.Month;
        }
        public static object day() {
            return DateTime.Now.Day;
        }
        public static object hour() {
            return DateTime.Now.Hour;
        }
        public static object minute() {
            return DateTime.Now.Minute;
        }
        public static object second() {
            return DateTime.Now.Second;
        }
        public static object millisecond() {
            return DateTime.Now.Millisecond;
        }
        public static object ticks() {
            return DateTime.Now.Ticks;
        }
    }
}
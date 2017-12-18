﻿using System;
using System.Collections.Generic;
using SonarAnalyzer.Rules;

namespace Tests.Diagnostics
{
    public class Mod
    {
        public static void DoSomething(ref int x)
        {
        }
        public static void DoSomething2(out int x)
        {
            x = 6;
        }
    }

    class Person
    {
        private int _birthYear;  // Noncompliant {{Make '_birthYear' 'readonly'.}}
//                  ^^^^^^^^^^
        int _birthMonth = 3;  // Noncompliant
        int _birthDay = 31;  // Compliant, the setter action references it
        int _birthDay2 = 31;  // Compliant, it is used in a delegate
        int _birthDay3 = 31;  // Compliant, it is passed as ref outside the ctor
        int _birthDay4 = 31;  // Compliant, it is passed as out outside the ctor
        int _legSize = 3;
        int _legSize2 = 3;
        int _neverUsed;

        private readonly Action<int> setter;

        Person(int birthYear)
        {
            setter = i => { _birthDay = i; };

            System.Threading.Thread t1 = new System.Threading.Thread
                (delegate()
                {
                    _birthDay2 = 42;
                });
            t1.Start();

            _birthYear = birthYear;
        }

        private void M()
        {
            Mod.DoSomething(ref this._birthDay3);
            Mod.DoSomething2(out _birthDay4);
        }

        public int LegSize
        {
            get
            {
                _legSize2++;
                return _legSize;
            }
            set { _legSize = value; }
        }
    }

    partial class Partial
    {
        private int i; // Non-compliant, but not reported now because of the partial
    }
    partial class Partial
    {
        public Partial()
        {
            i = 42;
        }
    }

    class X
    {
        private int x; // Compliant
        private int y; // Compliant
        private int z = 10; // Noncompliant
        private int w = 10; // Noncompliant
        public X()
        {
            new X().x = 12;
            var xx = new X();
            Modif(ref xx.y);

            Modif(ref (z));
            this.w = 42;
        }

        private void Modif(ref int i) { }
    }

    struct X1Struct
    {
        public Y1 y;
    }
    class X1Class
    {
        public Y1 y;
    }
    struct Y1
    {
        public string z;
    }

    class MyClass
    {
        private X1Struct x; // Compliant
        private X1Struct y; // Noncompliant

        private X1Class z; // Noncompliant

        private bool field = false;

        public MyClass()
        {
            x = new X1Struct();
            y = new X1Struct();
            z = new X1Class();
            (this.y.y).z = "a";
            (this.z.y).z = "a";
            if (this.field)
            { }
        }

        public void M()
        {
            (this.x.y).z = "a";
            (this.z.y).z = "a";
        }

        private class Nested
        {
            private readonly MyClass inst;
            public Nested()
            {
                inst = new MyClass();
            }
            private void Method()
            {
                this.inst.field = false;
            }
        }
    }

    class Attributed
    {
        [My]
        private int myField1; // Compliant because of the attribute

        public Attributed()
        {
            myField1 = 42;
        }
    }

    public class MyAttribute : Attribute { }

    // See https://github.com/SonarSource/sonar-csharp/issues/1009
    // Error with leading trivia not moved to the readonly modifier
    public class MyClassWithField
    {
        #region Test
        string teststring = null; // Noncompliant
        #endregion

        public void Do()
        {
            var x = teststring;
        }
    }
}

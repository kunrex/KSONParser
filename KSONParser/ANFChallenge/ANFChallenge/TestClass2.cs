using System;
using HelpfulExtensions;

namespace KSON
{
    [Serialisable]
    public class TestClass2
    {
        public TestClass2()
        {

        }

        public TestClass2(string val)
        {
            stringValue = val;
            class3 = new TestClass3(0);
        }

        public string stringValue;

        public TestClass3 class3;
    }
}

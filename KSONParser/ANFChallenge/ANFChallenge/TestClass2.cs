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
        }

        public string stringValue;
    }
}

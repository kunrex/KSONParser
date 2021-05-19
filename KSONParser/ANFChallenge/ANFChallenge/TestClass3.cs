using System;
using HelpfulExtensions;

namespace KSON
{
    [Serialisable]
    public class TestClass3
    {
        public TestClass3(int num)
        {
            popertyCheck = num;
        }

        public TestClass3()
        {

        }

        public int popertyCheck;
    }
}

using System;
using System.Collections.Generic;
using HelpfulExtensions;

namespace KSON
{
    [Serialisable]
    public class TestClass
    {
        public TestClass()
        {

        }

        public TestClass(int _int, string _string, float _float, bool _bool, string _nullableTest, int[] _numbers, string[] _words, float[] _floats, bool[] _bools)
        {
            newInt = _int;
            newString = _string;
            newFloat = _float;
            newBool = _bool;

            nullableTest = _nullableTest;
            numbers = _numbers;
            words = _words;
            floatingPoints = _floats;
            bools = _bools;

            testClass = new TestClass2(_string);
        }

        public int newInt;
        public string newString;
        public float newFloat;
        public bool newBool;
        public string nullableTest;

        public int[] numbers;
        public string[] words;
        public float[] floatingPoints;
        public bool[] bools;
        public TestClass2 testClass;
    }
}

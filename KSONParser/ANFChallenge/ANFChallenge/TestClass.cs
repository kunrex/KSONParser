using System;
using System.Collections.Generic;
using HelpfulExtensions;

namespace KSON
{
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
        }

        [Serialisable] public int newInt;
        [Serialisable] public string newString;
        [Serialisable] public float newFloat;
        [Serialisable] public bool newBool;
        [Serialisable] public string nullableTest;

        [Serialisable] public int[] numbers;
        [Serialisable] public string[] words;
        [Serialisable] public float[] floatingPoints;
        [Serialisable] public bool[] bools;
    }
}

using System;

namespace KSON
{
    class Program
    {
        static void Main(string[] args)
        {
            TestClass testClass_1 = new TestClass(10, "hello", .5f, true, null, new[] { 1, 2, 3 }, new[] { "hello", "this", "be", "KSON" }, new[] { 1.5f, 2.5f, 3.5f }, new[] { true, false});

            string parsed_1 = KsonParser.ToKson(testClass_1);
            Console.WriteLine(parsed_1);

            var testClass_2 = KsonParser.FromKson<TestClass>(parsed_1);

            string parsed_2 = KsonParser.ToKson(testClass_2);
            Console.WriteLine(parsed_2);
        }
    }
}

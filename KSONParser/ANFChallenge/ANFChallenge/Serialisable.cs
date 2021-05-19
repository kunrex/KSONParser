using System;
using System.Reflection;

namespace HelpfulExtensions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class Serialisable : Attribute
    {
        public string Serialise(int value) => value.ToString();

        public string Serialise(float value) => value.ToString();

        public string Serialise(string value) => value;

        public string Serialise(char value) => value.ToString();

        public string Serialise(bool value) => value.ToString().ToUpper();

        public dynamic Deserialise(string value, Type type)
        {
            if (!type.IsValidType(true))
                if(type.GetCustomAttribute<Serialisable>() == null)
                    throw new InvalidTypeException(false);

            switch (type)
            {
                case Type t when t == typeof(int):
                    return int.Parse(value);
                case Type t when t == typeof(float):
                    return float.Parse(value);
                case Type t when t == typeof(bool):
                    return value == "TRUE";
                case Type t when t == typeof(string):
                    return value;
                case Type t when t == typeof(char):
                    return value.ToChar();
            }

            return null;
        }
    }
}

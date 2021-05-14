using System;
namespace HelpfulExtensions
{
    public static class Extensions
    {
        public static bool IsViableToBeSerialised(this char _char) => char.IsLetter(_char) || char.IsNumber(_char) || _char == '"' || _char == '.' || _char == ',' || _char == '[' || _char == ']';

        public static bool IsValidToBeDeserialised_Array(this char _char) => char.IsLetter(_char) || char.IsNumber(_char) || _char == '.';

        public static bool DeterminesEndOfSerialisedArray(this char _char) => _char == ',' || _char == ']';

        public static bool IsValidType(this Type type, bool considerBool = false)
        {
            if (type.IsArray)
                return type.GetElementType() == typeof(int) || type.GetElementType() == typeof(float) || type.GetElementType() == typeof(string) || type.GetElementType() == typeof(char) || (type.GetElementType() == typeof(bool) && considerBool);
            else
                return type == typeof(int) || type == typeof(float) || type == typeof(string) || type == typeof(char) || (type == typeof(bool) && considerBool);
        }

        public static char ToChar(this string _string) 
        {
            if (_string.Length < 0)
                throw new ArgumentException("String has a length of more than 0");
            else if(_string == "" || _string == string.Empty)
                throw new ArgumentException("String is empty");
            else
                return _string[0];
        }
    }
}

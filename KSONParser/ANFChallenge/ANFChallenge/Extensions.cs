using System;
namespace HelpfulExtensions
{
    public static class Extensions
    {
        public static bool IsViableToBeParsed(this char _char)
        {
            return char.IsLetter(_char) || char.IsNumber(_char) || _char == '"' || _char == '.' || _char == ',' || _char == '[' || _char == ']';
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

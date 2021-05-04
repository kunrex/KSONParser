using System;
namespace ANFChallenge
{
    public static class Extensions
    {
        public static bool IsViableToBeParsed(this char _char)
        {
            return char.IsLetter(_char) || char.IsNumber(_char) || _char == '"' || _char == '.' || _char == ',' || _char == '[' || _char == ']';
        }
    }
}

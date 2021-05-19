using System;
namespace HelpfulExtensions
{
    public class InvalidTypeException : Exception
    {
        public InvalidTypeException(bool serialise) : base(serialise ? "Invalid Type To be Serialised" : "Invalid Type To be Deserialised")
        {

        }
    }
}

using System;
namespace HelpfulExtensions
{
    public class TypeNotSerialisable : Exception
    {
        public TypeNotSerialisable(string name) : base($"Class/struct {name} does not have Serialisable attribute applied")
        {

        }
    }
}

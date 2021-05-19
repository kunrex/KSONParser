using System;
namespace HelpfulExtensions
{
    public class FieldNotSerialisable : Exception
    {
        public FieldNotSerialisable(string name) : base($"Field {name} does not have Serialisable attribute applied")
        {

        }
    }
}

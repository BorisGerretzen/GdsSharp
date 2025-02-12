using System;

namespace GdsSharp.SourceGenerators.New
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Field)]
    public class BigEndianAttribute : Attribute
    {
        public readonly string FieldName;

        public BigEndianAttribute(string fieldName = null)
        {
            FieldName = fieldName;
        }
    }
}
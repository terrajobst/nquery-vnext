using System;

namespace NQuery.Symbols
{
    public abstract class ColumnDefinition
    {
        public abstract string Name { get; }
        public abstract Type DataType { get; }
        public abstract object GetValue(object row);
    }
}
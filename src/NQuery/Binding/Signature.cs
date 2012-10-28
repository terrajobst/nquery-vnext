using System;
using System.Collections.Generic;

namespace NQuery.Binding
{
    internal abstract class Signature
    {
        public abstract Type ReturnType { get; }
        public abstract Type GetParameterType(int index);
        public abstract int ParameterCount { get; }

        public IEnumerable<Type> GetParameterTypes()
        {
            for (var i = 0; i < ParameterCount; i++)
                yield return GetParameterType(i);
        }
    }
}
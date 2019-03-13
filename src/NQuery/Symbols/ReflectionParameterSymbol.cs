#nullable enable

using System;
using System.Reflection;

namespace NQuery.Symbols
{
    public class ReflectionParameterSymbol : ParameterSymbol
    {
        public ReflectionParameterSymbol(ParameterInfo parameterInfo)
            : this(parameterInfo, parameterInfo?.Name)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));

            ParameterInfo = parameterInfo;
        }

        public ReflectionParameterSymbol(ParameterInfo parameterInfo, string name)
            : base(name, parameterInfo?.ParameterType)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));

            ParameterInfo = parameterInfo;
        }

        public ParameterInfo ParameterInfo { get; }
    }
}
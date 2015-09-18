using System;
using System.Reflection;

namespace NQuery.Symbols
{
    public class ReflectionParameterSymbol : ParameterSymbol
    {
        public ReflectionParameterSymbol(ParameterInfo parameterInfo)
            : this(parameterInfo, parameterInfo == null ? null : parameterInfo.Name)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));

            ParameterInfo = parameterInfo;
        }

        public ReflectionParameterSymbol(ParameterInfo parameterInfo, string name)
            : base(name, parameterInfo == null ? null : parameterInfo.ParameterType)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));

            ParameterInfo = parameterInfo;
        }

        public ParameterInfo ParameterInfo { get; private set; }
    }
}
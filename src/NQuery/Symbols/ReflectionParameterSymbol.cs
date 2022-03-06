using System.Reflection;

namespace NQuery.Symbols
{
    public class ReflectionParameterSymbol : ParameterSymbol
    {
        public ReflectionParameterSymbol(ParameterInfo parameterInfo)
            : this(parameterInfo, parameterInfo?.Name)
        {
            ArgumentNullException.ThrowIfNull(parameterInfo);

            ParameterInfo = parameterInfo;
        }

        public ReflectionParameterSymbol(ParameterInfo parameterInfo, string name)
            : base(name, parameterInfo?.ParameterType)
        {
            ArgumentNullException.ThrowIfNull(parameterInfo);

            ParameterInfo = parameterInfo;
        }

        public ParameterInfo ParameterInfo { get; }
    }
}
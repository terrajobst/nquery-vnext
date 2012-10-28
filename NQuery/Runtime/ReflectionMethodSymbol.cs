using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NQuery.Symbols;

namespace NQuery.Runtime
{
    public class ReflectionMethodSymbol : MethodSymbol
    {
        public ReflectionMethodSymbol(MethodInfo methodInfo)
            : this(methodInfo, methodInfo == null ? null : methodInfo.Name)
        {
        }

        public ReflectionMethodSymbol(MethodInfo methodInfo, string name)
            : base(name, methodInfo == null ? null : methodInfo.ReturnType, ConvertParameters(methodInfo))
        {
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");

            MethodInfo = methodInfo;
        }

        private static IList<ParameterSymbol> ConvertParameters(MethodInfo methodInfo)
        {
            return (methodInfo == null
                        ? Enumerable.Empty<ParameterSymbol>()
                        : methodInfo.GetParameters().Select(p => new ReflectionParameterSymbol(p))).ToList();
        }

        public MethodInfo MethodInfo { get; private set; }
    }
}
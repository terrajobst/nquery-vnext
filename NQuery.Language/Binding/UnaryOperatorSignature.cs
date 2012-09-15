using System;
using System.Reflection;

namespace NQuery.Language.Binding
{
    internal sealed class UnaryOperatorSignature : Signature
    {
        private readonly UnaryOperatorKind _kind;
        private readonly Type _returnType;
        private readonly Type _argumentType;
        private readonly MethodInfo _methodInfo;

        private UnaryOperatorSignature(UnaryOperatorKind kind, Type returnType, Type argumentType, MethodInfo methodInfo)
        {
            _kind = kind;
            _returnType = returnType;
            _argumentType = argumentType;
            _methodInfo = methodInfo;
        }

        public UnaryOperatorSignature(UnaryOperatorKind kind, MethodInfo methodInfo)
            : this(kind, methodInfo.ReturnType, methodInfo.GetParameters()[0].ParameterType, methodInfo)
        {
        }

        public UnaryOperatorSignature(UnaryOperatorKind kind, Type returnType, Type argumentType)
            : this(kind, returnType, argumentType, null)
        {
        }

        public override Type ReturnType
        {
            get { return _returnType; }
        }

        public override Type GetParameterType(int index)
        {
            return _argumentType;
        }

        public override int ParameterCount
        {
            get { return 1; }
        }

        public UnaryOperatorKind Kind
        {
            get { return _kind; }
        }

        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
        }

        public override string ToString()
        {
            return string.Format("{0}({1}) AS {2}", _kind, _argumentType.ToDisplayName(), _returnType.ToDisplayName());
        }
    }
}
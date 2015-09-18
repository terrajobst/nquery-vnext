using System;
using System.Reflection;

namespace NQuery.Binding
{
    internal sealed class BinaryOperatorSignature : Signature
    {
        private readonly BinaryOperatorKind _kind;
        private readonly Type _returnType;
        private readonly Type _leftParameterType;
        private readonly Type _rightParameterType;
        private readonly MethodInfo _methodInfo;

        public BinaryOperatorSignature(BinaryOperatorKind kind, Type returnType, Type leftParameterType, Type rightParameterType, MethodInfo methodInfo)
        {
            _kind = kind;
            _returnType = returnType;
            _leftParameterType = leftParameterType;
            _rightParameterType = rightParameterType;
            _methodInfo = methodInfo;
        }

        public BinaryOperatorSignature(BinaryOperatorKind kind, MethodInfo methodInfo)
            : this(kind, methodInfo.ReturnType, methodInfo.GetParameters()[0].ParameterType, methodInfo.GetParameters()[1].ParameterType, methodInfo)
        {
        }

        public BinaryOperatorSignature(BinaryOperatorKind kind, Type returnType, Type parameterType)
            : this(kind, returnType, parameterType, parameterType, null)
        {
        }

        public BinaryOperatorSignature(BinaryOperatorKind kind, Type returnType, Type leftParameterType, Type rightParameterType)
            : this(kind, returnType, leftParameterType, rightParameterType, null)
        {
        }

        public override Type ReturnType
        {
            get { return _returnType; }
        }

        public override Type GetParameterType(int index)
        {
            return index == 0 ? _leftParameterType : _rightParameterType;
        }

        public override int ParameterCount
        {
            get { return 2; }
        }

        public BinaryOperatorKind Kind
        {
            get { return _kind; }
        }

        public MethodInfo MethodInfo
        {
            get { return _methodInfo; }
        }

        public override string ToString()
        {
            return $"{_kind}({_leftParameterType.ToDisplayName()}, {_rightParameterType.ToDisplayName()}) AS {_returnType.ToDisplayName()}";
        }
    }
}
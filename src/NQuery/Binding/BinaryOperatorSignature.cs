using System.Reflection;

namespace NQuery.Binding
{
    internal sealed class BinaryOperatorSignature : Signature
    {
        private readonly Type _returnType;
        private readonly Type _leftParameterType;
        private readonly Type _rightParameterType;

        public BinaryOperatorSignature(BinaryOperatorKind kind, Type returnType, Type leftParameterType, Type rightParameterType, MethodInfo methodInfo)
        {
            Kind = kind;
            _returnType = returnType;
            _leftParameterType = leftParameterType;
            _rightParameterType = rightParameterType;
            MethodInfo = methodInfo;
        }

        public BinaryOperatorSignature(BinaryOperatorKind kind, MethodInfo methodInfo)
            : this(kind, methodInfo.ReturnType, methodInfo.GetParameters()[0].ParameterType, methodInfo.GetParameters()[1].ParameterType, methodInfo)
        {
        }

        public BinaryOperatorSignature(BinaryOperatorKind kind, Type type)
            : this(kind, type, type)
        {
        }

        public BinaryOperatorSignature(BinaryOperatorKind kind, Type returnType, Type parameterType)
            : this(kind, returnType, parameterType, parameterType)
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

        public BinaryOperatorKind Kind { get; }

        public MethodInfo MethodInfo { get; }

        public override string ToString()
        {
            return $"{Kind}({_leftParameterType.ToDisplayName()}, {_rightParameterType.ToDisplayName()}) AS {_returnType.ToDisplayName()}";
        }
    }
}
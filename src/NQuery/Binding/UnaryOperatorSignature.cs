using System.Reflection;

namespace NQuery.Binding
{
    internal sealed class UnaryOperatorSignature : Signature
    {
        private readonly Type _returnType;
        private readonly Type _argumentType;

        private UnaryOperatorSignature(UnaryOperatorKind kind, Type returnType, Type argumentType, MethodInfo methodInfo)
        {
            Kind = kind;
            _returnType = returnType;
            _argumentType = argumentType;
            MethodInfo = methodInfo;
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

        public UnaryOperatorKind Kind { get; }

        public MethodInfo MethodInfo { get; }

        public override string ToString()
        {
            return $"{Kind}({_argumentType.ToDisplayName()}) AS {_returnType.ToDisplayName()}";
        }
    }
}
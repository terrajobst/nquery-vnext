using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundMethodInvocationExpression : BoundExpression
    {
        public BoundMethodInvocationExpression(BoundExpression target, IEnumerable<BoundExpression> arguments, OverloadResolutionResult<MethodSymbolSignature> result)
        {
            Target = target;
            Arguments = arguments.ToImmutableArray();
            Result = result;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.MethodInvocationExpression; }
        }

        public override Type Type
        {
            get { return Symbol is null ? TypeFacts.Unknown : Symbol.Type; }
        }

        public MethodSymbol Symbol
        {
            get { return Result.Selected is null ? null : Result.Selected.Signature.Symbol; }
        }

        public BoundExpression Target { get; }

        public ImmutableArray<BoundExpression> Arguments { get; }

        public OverloadResolutionResult<MethodSymbolSignature> Result { get; }

        public BoundMethodInvocationExpression Update(BoundExpression target, IEnumerable<BoundExpression> arguments, OverloadResolutionResult<MethodSymbolSignature> result)
        {
            var newArguments = arguments.ToImmutableArray();

            if (target == Target && newArguments == Arguments && result == Result)
                return this;

            return new BoundMethodInvocationExpression(target, newArguments, result);
        }

        public override string ToString()
        {
            return $"{Target}.{Symbol.Name}({string.Join(@", ", Arguments)})";
        }
    }
}
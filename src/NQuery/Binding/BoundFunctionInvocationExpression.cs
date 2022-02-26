using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundFunctionInvocationExpression : BoundExpression
    {
        public BoundFunctionInvocationExpression(IEnumerable<BoundExpression> arguments, OverloadResolutionResult<FunctionSymbolSignature> result)
        {
            Arguments = arguments.ToImmutableArray();
            Result = result;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.FunctionInvocationExpression; }
        }

        public override Type Type
        {
            get { return Symbol == null ? TypeFacts.Unknown : Symbol.Type; }
        }

        public FunctionSymbol Symbol
        {
            get { return Result.Selected == null ? null : Result.Selected.Signature.Symbol; }
        }

        public ImmutableArray<BoundExpression> Arguments { get; }

        public OverloadResolutionResult<FunctionSymbolSignature> Result { get; }

        public BoundFunctionInvocationExpression Update(IEnumerable<BoundExpression> arguments, OverloadResolutionResult<FunctionSymbolSignature> result)
        {
            var newArguments = arguments.ToImmutableArray();

            if (newArguments == Arguments && result == Result)
                return this;

            return new BoundFunctionInvocationExpression(newArguments, result);
        }

        public override string ToString()
        {
            return $"{Symbol.Name}({string.Join(@",", Arguments)})";
        }
    }
}
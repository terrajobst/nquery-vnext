#nullable enable

using System.Collections.Immutable;

using NQuery.Refactor.Binding;
using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Refactor.Algebra
{
    internal sealed class LogicalFunctionInvocationExpression : LogicalExpression
    {
        public LogicalFunctionInvocationExpression(ImmutableArray<LogicalExpression> arguments, OverloadResolutionResult<FunctionSymbolSignature> result)
        {
            Arguments = arguments;
            Result = result;
        }

        public override LogicalExpressionKind Kind => LogicalExpressionKind.FunctionInvocation;

        public override Type Type => Symbol is null ? TypeFacts.Unknown : Symbol.Type;

        public FunctionSymbol? Symbol => Result.Selected?.Signature.Symbol;

        public ImmutableArray<LogicalExpression> Arguments { get; }

        public OverloadResolutionResult<FunctionSymbolSignature> Result { get; }
    }
}

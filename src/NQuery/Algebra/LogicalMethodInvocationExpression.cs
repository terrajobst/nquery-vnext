#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class LogicalMethodInvocationExpression : LogicalExpression
    {
        public LogicalMethodInvocationExpression(LogicalExpression target, ImmutableArray<LogicalExpression> arguments, OverloadResolutionResult<MethodSymbolSignature> result)
        {
            Target = target;
            Arguments = arguments;
            Result = result;
        }

        public override LogicalExpressionKind Kind => LogicalExpressionKind.MethodInvocation;

        public override Type Type => Symbol is null ? TypeFacts.Unknown : Symbol.Type;

        public MethodSymbol? Symbol => Result.Selected?.Signature.Symbol;

        public LogicalExpression Target { get; }

        public ImmutableArray<LogicalExpression> Arguments { get; }

        public OverloadResolutionResult<MethodSymbolSignature> Result { get; }
    }
}

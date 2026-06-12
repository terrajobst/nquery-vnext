#nullable enable

using NQuery.Refactor.Binding;
using NQuery.Binding;

namespace NQuery.Refactor.Algebra
{
    internal sealed class LogicalBinaryExpression : LogicalExpression
    {
        public LogicalBinaryExpression(LogicalExpression left, BinaryOperatorKind operatorKind, OverloadResolutionResult<BinaryOperatorSignature> result, LogicalExpression right)
        {
            Left = left;
            OperatorKind = operatorKind;
            Result = result;
            Right = right;
        }

        public override LogicalExpressionKind Kind => LogicalExpressionKind.Binary;

        public override Type Type => Result.Selected is null ? TypeFacts.Unknown : Result.Selected.Signature.ReturnType;

        public LogicalExpression Left { get; }

        public BinaryOperatorKind OperatorKind { get; }

        public OverloadResolutionResult<BinaryOperatorSignature> Result { get; }

        public LogicalExpression Right { get; }
    }
}

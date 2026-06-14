using NQuery.Symbols;
using NQuery.Syntax;

using NQuery.Binding;

namespace NQuery.Binding
{
    internal sealed class BoundQueryState
    {
        public BoundQueryState(BoundQueryState parent)
        {
            Parent = parent;
        }

        public BoundQueryState Parent { get; }

        public Dictionary<TableInstanceSymbol, SyntaxToken> IntroducedTables { get; } = new();

        public List<BoundComputedValueWithSyntax> AccessibleComputedValues { get; } = new();

        public List<BoundComputedValueWithSyntax> ComputedGroupings { get; } = new();

        public List<BoundComputedValueWithSyntax> ComputedAggregates { get; } = new();

        public List<BoundComputedValueWithSyntax> ComputedProjections { get; } = new();

        public Dictionary<ExpressionSyntax, IBoundValue> ReplacedExpression { get; } = new();
    }
}
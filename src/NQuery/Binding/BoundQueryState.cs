using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Binding
{
    internal sealed class BoundQueryState
    {
        public BoundQueryState(BoundQueryState parent)
        {
            Parent = parent;
        }

        public BoundQueryState Parent { get; }

        public Dictionary<TableInstanceSymbol, SyntaxToken> IntroducedTables { get; } = new Dictionary<TableInstanceSymbol, SyntaxToken>();

        public List<BoundComputedValueWithSyntax> AccessibleComputedValues { get; } = new List<BoundComputedValueWithSyntax>();

        public List<BoundComputedValueWithSyntax> ComputedGroupings { get; } = new List<BoundComputedValueWithSyntax>();

        public List<BoundComputedValueWithSyntax> ComputedAggregates { get; } = new List<BoundComputedValueWithSyntax>();

        public List<BoundComputedValueWithSyntax> ComputedProjections { get; } = new List<BoundComputedValueWithSyntax>();

        public Dictionary<ExpressionSyntax, ValueSlot> ReplacedExpression { get; } = new Dictionary<ExpressionSyntax, ValueSlot>();
    }
}
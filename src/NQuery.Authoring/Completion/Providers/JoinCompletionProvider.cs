using System.Collections.Immutable;
using System.Text;

using NQuery.CodeAnalysis;
using NQuery.Metadata;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Completion.Providers;

internal sealed class JoinCompletionProvider : CompletionProvider<ConditionedJoinedTableReferenceSyntax>
{
    protected override IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position, ConditionedJoinedTableReferenceSyntax node)
    {
        if (node.OnKeyword.IsMissing || position < node.OnKeyword.Span.End)
            return Enumerable.Empty<CompletionItem>();

        var leftInstances = semanticModel.GetDeclaredSymbols(node.Left)!.ToImmutableArray();
        var rightInstances = semanticModel.GetDeclaredSymbols(node.Right)!.ToImmutableArray();
        var relationships = semanticModel.Compilation.Catalog.Relationships;

        return from left in leftInstances
               from right in rightInstances
               from relation in relationships
               where IsApplicable(relation, GetDefinition(left.Table), GetDefinition(right.Table))
               select GetCompletionItem(relation, left, right);
    }

    private static TableDefinition? GetDefinition(TableSymbol table)
    {
        return table.Definition;
    }

    private static bool IsApplicable(RelationshipDefinition relation, TableDefinition? leftTable, TableDefinition? rightTable)
    {
        if (leftTable is null || rightTable is null)
            return false;

        var isParentAndChild = (relation.ParentTable == leftTable && relation.ChildTable == rightTable);
        var isChildAndParent = (relation.ParentTable == rightTable && relation.ChildTable == leftTable);
        return isParentAndChild || isChildAndParent;
    }

    private static CompletionItem GetCompletionItem(RelationshipDefinition relationship, TableInstanceSymbol leftInstance, TableInstanceSymbol rightInstance)
    {
        var leftIsParent = GetDefinition(leftInstance.Table) == relationship.ParentTable;

        var leftColumns = leftIsParent
                              ? relationship.ParentColumns
                              : relationship.ChildColumns;

        var rightColumns = leftIsParent
                               ? relationship.ChildColumns
                               : relationship.ParentColumns;

        var condition = CreateCondition(leftInstance, leftColumns, rightInstance, rightColumns);
        var displayText = condition;
        var insertionText = condition;
        var description = condition;
        return new CompletionItem(displayText, insertionText, description, Glyph.Relation);
    }

    private static string CreateCondition(TableInstanceSymbol left, IReadOnlyList<ColumnDefinition> leftColumns, TableInstanceSymbol right, IReadOnlyList<ColumnDefinition> rightColumns)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < leftColumns.Count; i++)
        {
            if (sb.Length > 0)
                sb.Append(@" AND ");

            var leftColumn = leftColumns[i];
            var rightColumn = rightColumns[i];

            sb.Append(SyntaxFacts.GetValidIdentifier(right.Name));
            sb.Append(@".");
            sb.Append(SyntaxFacts.GetValidIdentifier(rightColumn.Name));
            sb.Append(@" = ");
            sb.Append(SyntaxFacts.GetValidIdentifier(left.Name));
            sb.Append(@".");
            sb.Append(SyntaxFacts.GetValidIdentifier(leftColumn.Name));
        }

        return sb.ToString();
    }
}

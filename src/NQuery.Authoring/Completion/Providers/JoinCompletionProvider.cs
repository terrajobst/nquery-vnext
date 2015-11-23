using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.Completion.Providers
{
    internal sealed class JoinCompletionProvider : CompletionProvider<ConditionedJoinedTableReferenceSyntax>
    {
        protected override IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position, ConditionedJoinedTableReferenceSyntax node)
        {
            if (node.OnKeyword.IsMissing || position < node.OnKeyword.Span.End)
                return Enumerable.Empty<CompletionItem>();

            var leftInstances = semanticModel.GetDeclaredSymbols(node.Left).ToImmutableArray();
            var rightInstances = semanticModel.GetDeclaredSymbols(node.Right).ToImmutableArray();
            var relations = semanticModel.Compilation.DataContext.Relations;

            return from left in leftInstances
                   from right in rightInstances
                   from relation in relations
                   where IsApplicable(relation, left.Table, right.Table)
                   select GetCompletionItem(relation, left, right);
        }

        private static bool IsApplicable(TableRelation relation, TableSymbol leftTable, TableSymbol rightTable)
        {
            var isParentAndChild = (relation.ParentTable == leftTable && relation.ChildTable == rightTable);
            var isChildAndParent = (relation.ParentTable == rightTable && relation.ChildTable == leftTable);
            return isParentAndChild || isChildAndParent;
        }

        private static CompletionItem GetCompletionItem(TableRelation tableRelation, TableInstanceSymbol leftInstance, TableInstanceSymbol rightInstance)
        {
            var leftColumns = leftInstance.Table == tableRelation.ParentTable
                                  ? tableRelation.ParentColumns
                                  : tableRelation.ChildColumns;

            var rightColumns = leftInstance.Table == tableRelation.ParentTable
                                   ? tableRelation.ChildColumns
                                   : tableRelation.ParentColumns;

            var condition = CreateCondition(leftInstance, leftColumns, rightInstance, rightColumns);
            var displayText = condition;
            var insertionText = condition;
            var description = condition;
            return new CompletionItem(displayText, insertionText, description, Glyph.Relation);
        }

        private static string CreateCondition(TableInstanceSymbol left, IReadOnlyList<ColumnSymbol> leftColumns, TableInstanceSymbol right, IReadOnlyList<ColumnSymbol> rightColumns)
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
}
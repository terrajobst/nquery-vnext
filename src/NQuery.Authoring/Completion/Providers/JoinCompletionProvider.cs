using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.Completion.Providers
{
    internal sealed class JoinCompletionProvider : ICompletionProvider
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenContext(position);
            var conditionedJoin = token.Parent.AncestorsAndSelf()
                                              .OfType<ConditionedJoinedTableReferenceSyntax>()
                                              .FirstOrDefault(c => !c.OnKeyword.IsMissing && c.OnKeyword.Span.End <= position);

            if (conditionedJoin == null)
                return Enumerable.Empty<CompletionItem>();

            var leftInstances = semanticModel.GetDeclaredSymbols(conditionedJoin.Left).ToArray();
            var rightInstances = semanticModel.GetDeclaredSymbols(conditionedJoin.Right).ToArray();
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
            return new CompletionItem(displayText, insertionText, description, NQueryGlyph.Relation);
        }

        private static string CreateCondition(TableInstanceSymbol left, IReadOnlyList<ColumnSymbol> leftColumns, TableInstanceSymbol right, IReadOnlyList<ColumnSymbol> rightColumns)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < leftColumns.Count; i++)
            {
                if (sb.Length > 0)
                    sb.Append(" AND ");

                var leftColumn = leftColumns[i];
                var rightColumn = rightColumns[i];

                sb.Append(right.Name);
                sb.Append(".");
                sb.Append(rightColumn.Name);
                sb.Append(" = ");
                sb.Append(left.Name);
                sb.Append(".");
                sb.Append(leftColumn.Name);
            }

            return sb.ToString();
        }
    }
}
using NQuery.Symbols;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Issues
{
    internal sealed class UnusedQueryColumnCodeIssueProvider : CodeIssueProvider<CompilationUnitSyntax>
    {
        protected override IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, CompilationUnitSyntax node)
        {
            if (node.Root is not QuerySyntax query)
                return Enumerable.Empty<CodeIssue>();

            var referencedColumns = new HashSet<ColumnInstanceSymbol>();

            // Mark final output columns as referenced

            var roots = semanticModel.GetOutputColumns(query);
            referencedColumns.UnionWith(roots);

            // Mark all columns occurring in expressions as referenced.

            var expressions = node.DescendantNodesAndSelf()
                                  .OfType<ExpressionSyntax>();

            foreach (var expression in expressions)
            {
                var symbol = semanticModel.GetSymbol(expression);
                if (symbol is ColumnInstanceSymbol column)
                    referencedColumns.Add(column);
            }

            // Mark all columns implied by wildcard selects as referenced
            
            var wildcards = node.DescendantNodesAndSelf()
                                .OfType<WildcardSelectColumnSyntax>();

            foreach (var wildcard in wildcards)
            {
                var symbols = semanticModel.GetColumnInstances(wildcard);
                foreach (var symbol in symbols)
                {
                    referencedColumns.Add(symbol);
                }
            }

            static bool IsBinaryQuery(QuerySyntax query)
            {
                return query is UnionQuerySyntax or
                                IntersectQuerySyntax or
                                ExceptQuerySyntax;
            }

            // For queries combined with INTERSECT, EXCEPT and UNION, we need need to mark
            // the input columns as used as the columns themselves cannot be removed without
            // changing the semantics of the query.

            var combinedQueries = node.DescendantNodesAndSelf()
                                      .OfType<QuerySyntax>()
                                      .Where(IsBinaryQuery)
                                      .Where(q => q is not UnionQuerySyntax u || u.AllKeyword is null);

            foreach (var combinedQuery in combinedQueries)
            {
                foreach (var symbol in semanticModel.GetOutputColumns(combinedQuery))
                    referencedColumns.Add(symbol);
            }

            // For ordered queries, we need to mark the ordered column as required

            var orderedQueries = node.DescendantNodesAndSelf()
                                     .OfType<OrderedQuerySyntax>();

            foreach (var orderedQuery in orderedQueries)
            {
                foreach (var column in orderedQuery.Columns)
                {
                    var symbol = semanticModel.GetSymbol(column);
                    referencedColumns.Add(symbol);
                }
            }

            // Note: We need to track "across" query boundaries, that is if the column we're referencing belongs
            //       to a query, we need to mark the underlying query column as used too.
            //
            //       We're doing this last by repeatedly expanding column instances.

            var needsExpanding = true;

            while (needsExpanding)
            {
                needsExpanding = false;

                foreach (var column in referencedColumns.ToArray())
                {
                    switch (column)
                    {
                        case TableColumnInstanceSymbol { Column: QueryColumnSymbol queryColumn }:
                            needsExpanding |= referencedColumns.Add(queryColumn.QueryColumnInstance);
                            break;
                        case QueryColumnInstanceSymbol queryColumnInstance:
                            foreach (var joinedColumn in queryColumnInstance.JoinedColumns)
                                needsExpanding |= referencedColumns.Add(joinedColumn);
                            break;
                    }
                }
            }

            // Now we can walk over all columns defined in queries and see whether they
            // are actually used.

            var result = new List<CodeIssue>();

            var expressionColumns = node.DescendantNodesAndSelf().OfType<ExpressionSelectColumnSyntax>();
            foreach (var expressionColumn in expressionColumns)
            {
                // We'll not mark SELECT expression as redundant if they are the only expression because that's
                // syntactically not valid.
                //
                // TODO: Maybe we could rewrite this SELECT NULL?
                if (expressionColumn.Parent is SelectClauseSyntax {Columns.Count: 1})
                    continue;

                // Skip columns that are used.
                var declaredColumn = semanticModel.GetDeclaredSymbol(expressionColumn);
                if (referencedColumns.Contains(declaredColumn))
                    continue;

                var actions = new[] { new RemoveSelectColumnCodeAction(expressionColumn) };
                var issue = new CodeIssue(CodeIssueKind.Unnecessary, expressionColumn.Span, "Unused column", actions);
                result.Add(issue);
            }

            return result;
        }

        private sealed class RemoveSelectColumnCodeAction : CodeAction
        {
            private readonly ExpressionSelectColumnSyntax _node;

            public RemoveSelectColumnCodeAction(ExpressionSelectColumnSyntax node)
                : base(node.SyntaxTree)
            {
                _node = node;
            }

            public override string Description
            {
                get { return "Removed unused SELECT expression"; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var selectClause = (SelectClauseSyntax)_node.Parent;
                var span = GetDeletionRange(selectClause.Columns, _node);
                changeSet.DeleteText(span);
            }

            private static TextSpan GetDeletionRange<T>(SeparatedSyntaxList<T> list, T node)
                where T: SyntaxNode
            {
                var index = list.IndexOf(node);
                if (index < 0)
                    return new TextSpan();

                var start = node.Span.Start;
                if (index > 0 && index == list.Count - 1)
                    start = list.GetSeparator(index - 1).Span.Start;

                var end = node.Span.End;
                if (index < list.Count - 1)
                    end = list[index + 1].Span.Start;

                return TextSpan.FromBounds(start, end);
            }
        }
    }
}
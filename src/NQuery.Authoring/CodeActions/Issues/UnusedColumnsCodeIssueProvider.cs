using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.CodeActions.Issues
{
    internal sealed class UnusedColumnsCodeIssueProvider : CodeIssueProvider<CompilationUnitSyntax>
    {
        protected override IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, CompilationUnitSyntax node)
        {
            var rootColumns = new HashSet<QueryColumnInstanceSymbol>(GetRootColumns(semanticModel, node));
            var readColumns = new HashSet<ColumnSymbol>(GetReadColumns(semanticModel, node));
            var mappings = GetMappings(semanticModel, node).ToLookup(t => t.Item1, t => t.Item2);

            var expressionColumns = node.DescendantNodes().OfType<ExpressionSelectColumnSyntax>();
            foreach (var expressionColumn in expressionColumns)
            {
                var queryColumn = semanticModel.GetDeclaredSymbol(expressionColumn);
                if (queryColumn != null)
                {
                    var isRoot = rootColumns.Contains(queryColumn);
                    var isRead = mappings[queryColumn].Any(readColumns.Contains);
                    var used = isRoot || isRead;
                    if (!used)
                        yield return new CodeIssue(CodeIssueKind.Unnecessary, expressionColumn.Span, "Unused column");
                }
            }
        }

        private static IEnumerable<QueryColumnInstanceSymbol> GetRootColumns(SemanticModel semanticModel, CompilationUnitSyntax node)
        {
            var rootQuery = node.Root as QuerySyntax;
            return rootQuery == null
                ? Enumerable.Empty<QueryColumnInstanceSymbol>()
                : semanticModel.GetOutputColumns(rootQuery);
        }

        private static IEnumerable<ColumnSymbol> GetReadColumns(SemanticModel semanticModel, CompilationUnitSyntax node)
        {
            var nameExpressions = node.DescendantNodes().OfType<ExpressionSyntax>();
            foreach (var nameExpressionSyntax in nameExpressions)
            {
                var symbol = semanticModel.GetSymbol(nameExpressionSyntax) as TableColumnInstanceSymbol;
                if (symbol != null)
                    yield return symbol.Column;
            }
        }

        private static IEnumerable<Tuple<QueryColumnInstanceSymbol, ColumnSymbol>> GetMappings(SemanticModel semanticModel, CompilationUnitSyntax node)
        {
            var derivedTableMappinngs = GetDerivedTableMappinngs(semanticModel, node);
            var commonTableExpressionMappings = GetCommonTableExpressionMappings(semanticModel, node);
            return derivedTableMappinngs.Concat(commonTableExpressionMappings);
        }

        private static IEnumerable<Tuple<QueryColumnInstanceSymbol, ColumnSymbol>> GetDerivedTableMappinngs(SemanticModel semanticModel, CompilationUnitSyntax node)
        {
            var derivedTableNodes = node.DescendantNodes().OfType<DerivedTableReferenceSyntax>();
            foreach (var derivedTableNode in derivedTableNodes)
            {
                var tableInstanceSymbol = semanticModel.GetDeclaredSymbol(derivedTableNode);
                if (tableInstanceSymbol != null)
                {
                    foreach (var columnInstance in tableInstanceSymbol.ColumnInstances)
                    {
                        var queryColumn = columnInstance.Column as QueryColumnSymbol;
                        if (queryColumn != null)
                        {
                            var from = queryColumn.QueryColumnInstance;
                            var to = queryColumn;
                            yield return Tuple.Create<QueryColumnInstanceSymbol, ColumnSymbol>(@from, to);
                        }
                    }
                }
            }
        }

        private static IEnumerable<Tuple<QueryColumnInstanceSymbol, ColumnSymbol>> GetCommonTableExpressionMappings(SemanticModel semanticModel, CompilationUnitSyntax node)
        {
            var commonTableExpressions = node.DescendantNodes().OfType<CommonTableExpressionSyntax>();
            foreach (var commonTableExpression in commonTableExpressions)
            {
                var commonTableExpressionSymbol = semanticModel.GetDeclaredSymbol(commonTableExpression);
                if (commonTableExpressionSymbol != null)
                {
                    foreach (var columnSymbol in commonTableExpressionSymbol.Columns)
                    {
                        var queryColumn = columnSymbol as QueryColumnSymbol;
                        if (queryColumn != null)
                        {
                            var from = queryColumn.QueryColumnInstance;
                            var to = queryColumn;
                            yield return Tuple.Create<QueryColumnInstanceSymbol, ColumnSymbol>(@from, to);
                        }
                    }
                }
            }
        }
    }
}
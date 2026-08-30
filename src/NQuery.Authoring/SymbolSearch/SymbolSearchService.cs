using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.SymbolSearch;

public sealed class SymbolSearchService
{
    public SymbolSpan? FindSymbol(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        return FindSymbol(view.Document.GetSemanticModel(cancellationToken), view.Position);
    }

    public ImmutableArray<SymbolSpan> FindUsages(Document document, Symbol symbol, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);
        ThrowIfNull(symbol);

        return [.. FindUsages(document.GetSemanticModel(cancellationToken), symbol)];
    }

    // What both entry points above are defined in terms of, so that "what counts as a reference to
    // this symbol" is defined exactly once.
    private static SymbolSpan? FindSymbol(SemanticModel semanticModel, int position)
    {
        return semanticModel.SyntaxTree.Root.FindNodes(position)
                            .SelectMany(n => GetSymbolSpans(semanticModel, n))
                            .Where(s => s.Span.ContainsOrTouches(position))
                            .Select(s => s).Cast<SymbolSpan?>().FirstOrDefault();
    }

    private static IEnumerable<SymbolSpan> FindUsages(SemanticModel semanticModel, Symbol symbol)
    {
        return from n in semanticModel.SyntaxTree.Root.DescendantNodes()
               from s in GetSymbolSpans(semanticModel, n)
               where s.Symbol == symbol
               select s;
    }

    private static IEnumerable<SymbolSpan> GetSymbolSpans(SemanticModel semanticModel, SyntaxNode node)
    {
        switch (node.Kind)
        {
            case SyntaxKind.NameExpression:
            {
                var expression = (NameExpressionSyntax)node;
                var symbol = semanticModel.GetSymbol(expression);
                if (symbol is not null)
                    yield return SymbolSpan.CreateReference(symbol!, expression.IdentifierToken.Span);
                break;
            }
            case SyntaxKind.PropertyAccessExpression:
            {
                var expression = (PropertyAccessExpressionSyntax)node;
                var symbol = semanticModel.GetSymbol(expression);
                if (symbol is not null)
                    yield return SymbolSpan.CreateReference(symbol!, expression.IdentifierToken.Span);
                break;
            }
            case SyntaxKind.MethodInvocationExpression:
            {
                var expression = (MethodInvocationExpressionSyntax)node;
                var symbol = semanticModel.GetSymbol(expression);
                if (symbol is not null)
                    yield return SymbolSpan.CreateReference(symbol!, expression.IdentifierToken.Span);
                break;
            }
            case SyntaxKind.FunctionInvocationExpression:
            {
                var expression = (FunctionInvocationExpressionSyntax)node;
                var symbol = semanticModel.GetSymbol(expression);
                if (symbol is not null)
                    yield return SymbolSpan.CreateReference(symbol!, expression.IdentifierToken.Span);
                break;
            }
            case SyntaxKind.CountAllExpression:
            {
                var countAllExpression = (CountAllExpressionSyntax)node;
                var symbol = semanticModel.GetSymbol(countAllExpression);
                yield return SymbolSpan.CreateReference(symbol!, countAllExpression.IdentifierToken.Span);
                break;
            }
            case SyntaxKind.ExpressionSelectColumn:
            {
                var selectColumn = (ExpressionSelectColumnSyntax)node;
                if (selectColumn.Alias is not null)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(selectColumn);
                    yield return SymbolSpan.CreateDefinition(symbol!, selectColumn.Alias.IdentifierToken.Span);
                }
                break;
            }
            case SyntaxKind.CommonTableExpression:
            {
                var commonTableExpression = (CommonTableExpressionSyntax)node;
                var symbol = semanticModel.GetDeclaredSymbol(commonTableExpression);
                yield return SymbolSpan.CreateDefinition(symbol!, commonTableExpression.IdentifierToken.Span);

                if (commonTableExpression.ColumnNameList is not null)
                {
                    foreach (var columnName in commonTableExpression.ColumnNameList.ColumnNames)
                    {
                        var columnSymbol = semanticModel.GetDeclaredSymbol(columnName);
                        if (columnSymbol is not null)
                            yield return SymbolSpan.CreateDefinition(columnSymbol, columnName.Span);
                    }
                }
                break;
            }
            case SyntaxKind.DerivedTableReference:
            {
                var derivedTable = (DerivedTableReferenceSyntax)node;
                var symbol = semanticModel.GetDeclaredSymbol(derivedTable);
                yield return SymbolSpan.CreateDefinition(symbol!, derivedTable.IdentifierToken.Span);
                break;
            }
            case SyntaxKind.NamedTableReference:
            {
                var namedTable = (NamedTableReferenceSyntax)node;
                var tableInstanceSymbol = semanticModel.GetDeclaredSymbol(namedTable);
                if (tableInstanceSymbol is not null)
                {
                    yield return SymbolSpan.CreateReference(tableInstanceSymbol.Table, namedTable.IdentifierToken.Span);
                    if (namedTable.Alias is not null)
                        yield return SymbolSpan.CreateDefinition(tableInstanceSymbol, namedTable.Alias.IdentifierToken.Span);
                }
                break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.Completion.Providers
{
    // TODO: We should consider making this an abstract base class so we can provide different flavors:
    //       - table.Column <op> <value>
    //       - table.Column IN (<value1>, <value2>)
    //       - table.Column BETWEEN <value1> and <value2>
    //       - NULLIF(table.Column, <value>)
    //       - CASE table.Column WHEN <value> THEN <expression> END CASE
    // TODO: Completion doesn't handle cases like this properly because the completion span is includes all text until EOF
    //         SELECT  *
    //         FROM    Employees e
    //         WHERE   e.City = '|
    //         ORDER   BY 1
    // TODO: We need tests
    // TODO: We need a glyph for data items
    // TODO: Since data suggestions aren't necessarily the value the developer wants we should have the current token as a builder
    // TODO: Should we allow the host to customize the behavior? E.g. disabling it for certai columns, tweaking the TOP N etc.
    // TODO: Should we ever support non strings?
    // TODO: Should we centralize the trigger / commit characters? Should we allow providers to define them?
    internal sealed class DataCompletionProvider : ICompletionProvider
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position)
        {
            var dataContext = semanticModel.Compilation.DataContext;
            var syntaxTree = semanticModel.Compilation.SyntaxTree;

            BinaryExpressionSyntax comparison;
            if (!TryGetComparisonWithLiteralOnRightHandSide(syntaxTree, position, out comparison))
                return Enumerable.Empty<CompletionItem>();

            TableColumnInstanceSymbol symbol;
            if (!TryGetSchemaColumnInstance(semanticModel, comparison.Left, out symbol))
                return Enumerable.Empty<CompletionItem>();

            if (!IsCompletableType(symbol.Type))
                return Enumerable.Empty<CompletionItem>();

            return GetCompletions(dataContext, symbol);
        }

        private static bool TryGetComparisonWithLiteralOnRightHandSide(SyntaxTree syntaxTree, int position, out BinaryExpressionSyntax comparison)
        {
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            comparison = token.Parent.AncestorsAndSelf()
                                     .OfType<BinaryExpressionSyntax>()
                                     .FirstOrDefault();

            if (comparison == null || !IsComparison(comparison.Kind))
                return false;

            if (!comparison.Right.FullSpan.ContainsOrTouches(position))
                return false;

            return comparison.Right.IsMissing || comparison.Right is LiteralExpressionSyntax;
        }

        private static bool IsComparison(SyntaxKind kind)
        {
            return kind == SyntaxKind.EqualExpression ||
                   kind == SyntaxKind.NotEqualExpression ||
                   kind == SyntaxKind.LessExpression ||
                   kind == SyntaxKind.LessOrEqualExpression ||
                   kind == SyntaxKind.GreaterExpression ||
                   kind == SyntaxKind.GreaterOrEqualExpression;
        }

        private static bool TryGetSchemaColumnInstance(SemanticModel semanticModel, ExpressionSyntax expressionSyntax, out TableColumnInstanceSymbol columnInstanceSymbol)
        {
            var symbol = semanticModel.GetSymbol(expressionSyntax) as TableColumnInstanceSymbol;
            if (symbol != null && symbol.TableInstance.Table is SchemaTableSymbol)
            {
                columnInstanceSymbol = symbol;
                return true;
            }

            columnInstanceSymbol = null;
            return false;
        }

        private static bool IsCompletableType(Type type)
        {
            return GetSourceConverter(type) != null;
        }

        private static IEnumerable<CompletionItem> GetCompletions(DataContext dataContext, TableColumnInstanceSymbol symbol)
        {
            var tableName = SyntaxFacts.GetQuotedIdentifier(symbol.TableInstance.Table.Name);
            var columnName = SyntaxFacts.GetQuotedIdentifier(symbol.Column.Name);
            var sql = $@"
                SELECT DISTINCT TOP 100
                        {columnName}
                FROM   {tableName}
                WHERE  {columnName} IS NOT NULL
                ORDER BY 1
            ";

            var converter = GetSourceConverter(symbol.Type);

            var query = Query.Create(dataContext, sql);
            using (var reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    var value = converter(reader[0]);
                    yield return new CompletionItem(value, value, value, true);
                }
            }
        }

        private static Func<object, string> GetSourceConverter(Type type)
        {
            if (type == typeof (string))
                return GetStringLiteral;

            return null;
        }

        private static string GetStringLiteral(object value)
        {
            // TODO: SyntaxFacts should support escaping
            return "'" + Convert.ToString(value) + "'";
        }
    }
}
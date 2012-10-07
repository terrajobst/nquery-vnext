using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor.Completion
{
    [Export(typeof(ICompletionProvider))]
    internal sealed class SymbolCompletionProvider : ICompletionProvider
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position)
        {
            var root = semanticModel.Compilation.SyntaxTree.Root;

            // We don't want to show a completion when typing an alias name.
            // The CTE name and column list as well the name of a derived table count
            // as an alias context, too.
            if (InAlias(root, position) || InCteName(root,position) || InCteColumnList(root, position) || InDerivedTableName(root, position))
                return Enumerable.Empty<CompletionItem>();

            // Comments and literals don't get completion information
            if (InComment(root, position) || InLiteral(root, position))
                return Enumerable.Empty<CompletionItem>();

            var propertyAccessExpression = GetPropertyAccessExpression(root, position);

            var completions = propertyAccessExpression == null
                                  ? GetGlobalCompletions(semanticModel, position)
                                  : GetMemberCompletions(semanticModel, propertyAccessExpression);

            var sortedCompletions = completions.OrderBy(c => c.InsertionText);

            return sortedCompletions;
        }

        private static IEnumerable<CompletionItem> GetGlobalCompletions(SemanticModel semanticModel, int position)
        {
            var symbolCompletions = GetGlobalSymbolCompletions(semanticModel, position);
            var keywordCompletions = GetKeywordCompletions();
            var completions = symbolCompletions.Concat(keywordCompletions);
            return completions;
        }

        private static IEnumerable<CompletionItem> GetMemberCompletions(SemanticModel semanticModel, PropertyAccessExpressionSyntax propertyAccessExpression)
        {
            var tableInstanceSymbol = semanticModel.GetSymbol(propertyAccessExpression.Target) as TableInstanceSymbol;
            if (tableInstanceSymbol != null)
                return CreateSymbolCompletions(tableInstanceSymbol.ColumnInstances);

            var targetType = semanticModel.GetExpressionType(propertyAccessExpression.Target);
            if (targetType != null)
                return GetTypeCompletions(semanticModel, targetType);

            return Enumerable.Empty<CompletionItem>();
        }

        private static IEnumerable<CompletionItem> GetTypeCompletions(SemanticModel semanticModel, Type targetType)
        {
            var properties = semanticModel.LookupProperties(targetType);
            var methods = semanticModel.LookupMethods(targetType);
            var members = properties.Concat<Symbol>(methods);
            return CreateSymbolCompletions(members);
        }

        private static PropertyAccessExpressionSyntax GetPropertyAccessExpression(SyntaxNode root, int position)
        {
            var p = root.FindTokenTouched(position, descendIntoTrivia: true).Parent.AncestorsAndSelf().OfType<PropertyAccessExpressionSyntax>().FirstOrDefault();

            if (p != null)
            {
                var afterDot = p.Dot.Span.End <= position && position <= p.Name.Span.End;
                if (afterDot)
                    return p;
            }

            return null;
        }

        private static bool InAlias(SyntaxNode root, int position)
        {
            var syntaxToken = root.FindTokenTouched(position, descendIntoTrivia:true);
            return syntaxToken.Parent is AliasSyntax;
        }

        private static bool InCteName(SyntaxNode root, int position)
        {
            var token = root.FindTokenTouched(position, descendIntoTrivia: true);
            var cte = token.Parent as CommonTableExpressionSyntax;
            return cte != null && cte.Name.Span.Contains(position);
        }

        private static bool InCteColumnList(SyntaxNode root, int position)
        {
            var syntaxToken = root.FindTokenTouched(position, descendIntoTrivia: true);
            return syntaxToken.Parent is CommonTableExpressionColumnNameSyntax ||
                   syntaxToken.Parent is CommonTableExpressionColumnNameListSyntax;
        }

        private static bool InDerivedTableName(SyntaxNode root, int position)
        {
            var syntaxToken = root.FindTokenTouched(position, descendIntoTrivia: true);
            var derivedTable = syntaxToken.Parent as DerivedTableReferenceSyntax;
            return derivedTable != null && derivedTable.Name.FullSpan.Contains(position);
        }

        private static bool InComment(SyntaxNode root, int position)
        {
            var contains = root.FullSpan.Start <= position && position <= root.FullSpan.End;
            if (!contains)
                return false;

            var relevantChildren = from n in root.ChildNodesAndTokens()
                                   where n.FullSpan.Start <= position && position <= n.FullSpan.End
                                   select n;

            foreach (var child in relevantChildren)
            {
                if (child.IsNode)
                {
                    if (InComment(child.AsNode(), position))
                        return true;
                }
                else
                {
                    var token = child.AsToken();
                    var inComment = (from t in token.LeadingTrivia.Concat(token.TrailingTrivia)
                                     where InSingleLineComment(t, position) ||
                                           InMultiLineComment(t, position)
                                     select t).Any();
                    if (inComment)
                        return true;
                }
            }

            return false;
        }

        private static bool InSingleLineComment(SyntaxTrivia token, int position)
        {
            return token.Kind == SyntaxKind.SingleLineCommentTrivia &&
                   token.Span.Start <= position &&
                   position <= token.Span.End;
        }

        private static bool InMultiLineComment(SyntaxTrivia token, int position)
        {
            return token.Kind == SyntaxKind.MultiLineCommentTrivia &&
                   token.Span.Start <= position &&
                   position <= token.Span.End;
        }

        private static bool InLiteral(SyntaxNode root, int position)
        {
            return root.FindTokenTouched(position, descendIntoTrivia: true).Kind.IsLiteral();
        }

        private static IEnumerable<CompletionItem> GetGlobalSymbolCompletions(SemanticModel semanticModel, int position)
        {
            var symbols = semanticModel.LookupSymbols(position);
            if (!symbols.Any())
                symbols = semanticModel.LookupSymbols(position - 1);

            return from s in symbols
                   group s by s.Name
                   into g
                   select CreateSymbolCompletionGroup(g.Key, g);
        }

        private static IEnumerable<CompletionItem> CreateSymbolCompletions(IEnumerable<Symbol> symbols)
        {
            return from s in symbols
                   group s by s.Name
                   into g
                   select CreateSymbolCompletionGroup(g.Key, g);
        }

        private static CompletionItem CreateSymbolCompletionGroup(string name, IEnumerable<Symbol> symbols)
        {
            var multiple = symbols.Skip(1).Any();
            if (!multiple)
                return CreateSymbolCompletion(symbols.First());

            var hasNonInvocables = symbols.Any(s => !(s is InvocableSymbol));
            if (!hasNonInvocables)
                return CreateInvocableCompletionGroup(symbols);

            var displayText = name;
            var insertionText = name;

            var sb = new StringBuilder();
            sb.Append("Ambiguous Name:");
            foreach (var symbol in symbols)
            {
                sb.AppendLine();
                sb.Append("  ");
                sb.Append(symbol);
            }

            var description = sb.ToString();
            return new CompletionItem(displayText, insertionText, description, NQueryGlyph.AmbiguousName);
        }

        private static CompletionItem CreateInvocableCompletionGroup(IEnumerable<Symbol> symbols)
        {
            var first = CreateSymbolCompletion(symbols.First());
            var numberOfOverloads = symbols.Count() - 1;

            var displayText = first.DisplayText;
            var insertionText = first.InsertionText;
            var description = string.Format("{0} (+ {1} overload(s))", first.Description, numberOfOverloads);
            var glyph = first.Glyph;
            return new CompletionItem(displayText, insertionText, description, glyph);
        }

        private static CompletionItem CreateSymbolCompletion(Symbol symbol)
        {
            var displayText = symbol.Name;
            var insertionText = symbol.Name;
            var description = symbol.ToString();
            var glyph = GetGlyph(symbol);
            return new CompletionItem(displayText, insertionText, description, glyph);
        }

        private static IEnumerable<CompletionItem> GetKeywordCompletions()
        {
            return from k in SyntaxFacts.GetKeywordKinds()
                   let text = k.GetText()
                   select new CompletionItem(text, text, null, NQueryGlyph.Keyword);
        }

        private static NQueryGlyph GetGlyph(Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Column:
                    return NQueryGlyph.Column;
                case SymbolKind.SchemaTable:
                case SymbolKind.DerivedTable:
                case SymbolKind.CommonTableExpression:
                    return NQueryGlyph.Table;
                case SymbolKind.TableInstance:
                    return NQueryGlyph.TableRef;
                case SymbolKind.ColumnInstance:
                    return NQueryGlyph.Column;
                case SymbolKind.Variable:
                    return NQueryGlyph.Variable;
                case SymbolKind.Function:
                    return NQueryGlyph.Function;
                case SymbolKind.Aggregate:
                    return NQueryGlyph.Aggregate;
                case SymbolKind.Property:
                    return NQueryGlyph.Property;
                case SymbolKind.Method:
                    return NQueryGlyph.Method;
                default:
                    throw new NotImplementedException(string.Format("Unknown symbol kind: {0}", symbol.Kind));
            }
        }
    }
}
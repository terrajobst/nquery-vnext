using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.Completion.Providers
{
    internal sealed class SymbolCompletionProvider : ICompletionProvider
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position)
        {
            var root = semanticModel.SyntaxTree.Root;

            // We don't want to show a completion when typing an alias name.
            if (root.PossiblyInUserGivenName(position))
                return Enumerable.Empty<CompletionItem>();

            // Comments and literals don't get completion information
            if (root.InComment(position) || root.InLiteral(position))
                return Enumerable.Empty<CompletionItem>();

            var propertyAccessExpression = GetPropertyAccessExpression(root, position);
            return propertyAccessExpression == null
                       ? GetGlobalCompletions(semanticModel, position)
                       : GetMemberCompletions(semanticModel, propertyAccessExpression);
        }

        private static IEnumerable<CompletionItem> GetGlobalCompletions(SemanticModel semanticModel, int position)
        {
            var symbols = semanticModel.LookupSymbols(position);

            // In the global context there two different cases:
            //
            // (1) We are in a table reference context
            // (2) We are somewhere else
            //
            // In case (1) we can ONLY see table symbols and in (2)
            // we can see all symbols EXCEPT table symbols.

            var syntaxTree = semanticModel.SyntaxTree;
            var findTokenContext = syntaxTree.Root.FindTokenContext(position);
            var isTableContext = findTokenContext.Parent.AncestorsAndSelf().OfType<NamedTableReferenceSyntax>().Any();

            var filteredSymbols = from s in symbols
                                  let isTable = s is TableSymbol
                                  where isTable && isTableContext ||
                                        !isTable && !isTableContext
                                  select s;

            return CreateSymbolCompletions(filteredSymbols);
        }

        private static IEnumerable<CompletionItem> GetMemberCompletions(SemanticModel semanticModel, PropertyAccessExpressionSyntax propertyAccessExpression)
        {
            var tableInstanceSymbol = semanticModel.GetSymbol(propertyAccessExpression.Target) as TableInstanceSymbol;
            if (tableInstanceSymbol != null)
                return GetTableCompletions(tableInstanceSymbol);

            var targetType = semanticModel.GetExpressionType(propertyAccessExpression.Target);
            if (targetType != null && !targetType.IsUnknown() && !targetType.IsError())
                return GetTypeCompletions(semanticModel, targetType);

            return Enumerable.Empty<CompletionItem>();
        }

        private static IEnumerable<CompletionItem> GetTableCompletions(TableInstanceSymbol tableInstanceSymbol)
        {
            return CreateSymbolCompletions(tableInstanceSymbol.ColumnInstances);
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
            var token = root.FindTokenOnLeft(position);
            var previous = token.GetPreviousToken(false, true);
            var dot = previous != null && previous.Kind == SyntaxKind.DotToken
                          ? previous
                          : token;

            var p = dot.Parent.AncestorsAndSelf().OfType<PropertyAccessExpressionSyntax>().FirstOrDefault();

            if (p != null)
            {
                var afterDot = p.Dot.Span.End <= position && position <= p.Name.Span.End;
                if (afterDot)
                    return p;
            }

            return null;
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
            var insertionText = SyntaxFacts.GetValidIdentifier(name);

            var sb = new StringBuilder();
            sb.Append(Resources.AmbiguousName);
            foreach (var symbol in symbols)
            {
                sb.AppendLine();
                sb.Append(@"  ");
                sb.Append(symbol);
            }

            var description = sb.ToString();
            return new CompletionItem(displayText, insertionText, description, Glyph.AmbiguousName);
        }

        private static CompletionItem CreateInvocableCompletionGroup(IEnumerable<Symbol> symbols)
        {
            var symbol = symbols.First();
            var first = CreateSymbolCompletion(symbol);
            var numberOfOverloads = symbols.Count() - 1;

            var displayText = first.DisplayText;
            var insertionText = first.InsertionText;
            var description = string.Format(Resources.CompletionItemWithOverloads, first.Description, numberOfOverloads);
            var glyph = first.Glyph;
            return new CompletionItem(displayText, insertionText, description, glyph, symbol);
        }

        private static CompletionItem CreateSymbolCompletion(Symbol symbol)
        {
            var displayText = symbol.Name;
            var insertionText = SyntaxFacts.GetValidIdentifier(symbol.Name);
            var description = SymbolMarkup.ForSymbol(symbol).ToString();
            var glyph = symbol.GetGlyph();
            return new CompletionItem(displayText, insertionText, description, glyph, symbol);
        }
    }
}
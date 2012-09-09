using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryCompletionSet : CompletionSet
    {
        private readonly ICompletionSession _session;
        private readonly INQuerySemanticModelManager _semanticModelManager;
        private readonly INQueryGlyphService _glyphService;

        public NQueryCompletionSet(ICompletionSession session, INQuerySemanticModelManager semanticModelManager, INQueryGlyphService glyphService)
        {
            _session = session;
            _semanticModelManager = semanticModelManager;
            _glyphService = glyphService;
            _semanticModelManager.SemanticModelChanged += SemanticModelManagerOnSemanticModelChanged;
            _session.Dismissed += SessionOnDismissed;
            Recalculate();
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= SessionOnDismissed;
            _semanticModelManager.SemanticModelChanged -= SemanticModelManagerOnSemanticModelChanged;
        }

        private void SemanticModelManagerOnSemanticModelChanged(object sender, EventArgs eventArgs)
        {
            Recalculate();
            SelectBestMatch();
        }

        public override void Recalculate()
        {
            WritableCompletions.Clear();
            var textBuffer = _session.TextView.TextBuffer;
            var snapshot = textBuffer.CurrentSnapshot;
            var position = _session.GetTriggerPoint(textBuffer).GetPosition(snapshot);

            var semanticModel = _semanticModelManager.SemanticModel;
            if (semanticModel == null)
                return;

            var root = semanticModel.Compilation.SyntaxTree.Root;
            var tokenAtPosition = GetIdentifierOrKeywordAtPosition(root, position) ??
                                  GetIdentifierOrKeywordAtPosition(root, position - 1);
            var span = tokenAtPosition == null
                           ? new TextSpan(position, 0)
                           : tokenAtPosition.Value.Span;

            var completions = GetCompletions(semanticModel, position);
            ApplicableTo = snapshot.CreateTrackingSpan(span.Start, span.Length, SpanTrackingMode.EdgeInclusive);
            WritableCompletions.AddRange(completions);

            if (WritableCompletions.Count == 0)
                _session.Dismiss();
        }

        private static SyntaxToken? GetIdentifierOrKeywordAtPosition(SyntaxNode root, int position)
        {
            var syntaxToken = root.FindToken(position);
            return syntaxToken.Kind.IsIdentifierOrKeyword()
                       ? syntaxToken
                       : (SyntaxToken?) null;
        }

        private IEnumerable<Completion> GetCompletions(SemanticModel semanticModel, int position)
        {
            var root = semanticModel.Compilation.SyntaxTree.Root;

            // We don't want to show a completion when typing an alias name.
            // The CTE column list as well the name of a derived table count
            // as an alias context, too.
            if (InAlias(root, position) || InCteColumnList(root, position) || InDerivedTableName(root, position))
                return Enumerable.Empty<Completion>();

            // Comments and literals don't get completion information
            if (InComment(root, position) || InLiteral(root, position))
                return Enumerable.Empty<Completion>();

            var propertyAccessExpression = GetPropertyAccessExpression(root, position);

            var completions = propertyAccessExpression == null
                                  ? GetGlobalCompletions(semanticModel, position)
                                  : GetMemberCompletions(semanticModel, propertyAccessExpression);

            var sortedCompletions = completions.OrderBy(c => c.InsertionText);

            return sortedCompletions;
        }

        private IEnumerable<Completion> GetGlobalCompletions(SemanticModel semanticModel, int position)
        {
            var symbolCompletions = GetGlobalSymbolCompletions(semanticModel, position);
            var keywordCompletions = GetKeywordCompletions();
            var completions = symbolCompletions.Concat(keywordCompletions);
            return completions;
        }

        private IEnumerable<Completion> GetMemberCompletions(SemanticModel semanticModel, PropertyAccessExpressionSyntax propertyAccessExpression)
        {
            var tableInstanceSymbol = semanticModel.GetSymbol(propertyAccessExpression.Target) as TableInstanceSymbol;
            if (tableInstanceSymbol != null)
                return CreateSymbolCompletions(tableInstanceSymbol.ColumnInstances);

            var targetType = semanticModel.GetExpressionType(propertyAccessExpression.Target);
            if (targetType != null)
                return GetTypeCompletions(semanticModel, targetType);

            return Enumerable.Empty<Completion>();
        }

        private IEnumerable<Completion> GetTypeCompletions(SemanticModel semanticModel, Type targetType)
        {
            var propertyCompletions = GetPropertyCompletions(semanticModel, targetType);
            var methodCompletions = GetMethodCompletions(semanticModel, targetType);
            return propertyCompletions.Concat(methodCompletions);
        }

        private IEnumerable<Completion> GetPropertyCompletions(SemanticModel semanticModel, Type targetType)
        {
            var dataContext = semanticModel.Compilation.DataContext;
            var propertyProvider = dataContext.PropertyProviders.LookupValue(targetType);
            return propertyProvider == null
                       ? Enumerable.Empty<Completion>()
                       : GetPropertyCompletions(propertyProvider.GetProperties(targetType));
        }

        private IEnumerable<Completion> GetPropertyCompletions(IEnumerable<PropertySymbol> propertySymbols)
        {
            return from m in propertySymbols
                   select CreateSymbolCompletion(m);
        }

        private IEnumerable<Completion> GetMethodCompletions(SemanticModel semanticModel, Type targetType)
        {
            var dataContext = semanticModel.Compilation.DataContext;
            var methodProvider = dataContext.MethodProviders.LookupValue(targetType);
            return methodProvider == null
                       ? Enumerable.Empty<Completion>()
                       : GetMethodCompletions(methodProvider.GetMethods(targetType));
        }

        private IEnumerable<Completion> GetMethodCompletions(IEnumerable<MethodSymbol> methodSymbols)
        {
            return from m in methodSymbols
                   group m by m.Name into g
                   select CreateSymbolCompletion(g.First());
        }

        private static PropertyAccessExpressionSyntax GetPropertyAccessExpression(SyntaxNode root, int position)
        {
            var p = root.FindToken(position).Parent.AncestorsAndSelf().OfType<PropertyAccessExpressionSyntax>().FirstOrDefault();

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
            var syntaxToken = root.FindToken(position);
            return syntaxToken.Parent is AliasSyntax;
        }

        private static bool InCteColumnList(SyntaxNode root, int position)
        {
            var syntaxToken = root.FindToken(position);
            return syntaxToken.Parent is CommonTableExpressionColumnNameSyntax ||
                   syntaxToken.Parent is CommonTableExpressionColumnNameListSyntax;
        }

        private static bool InDerivedTableName(SyntaxNode root, int position)
        {
            var syntaxToken = root.FindToken(position);
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
            // TODO: This is only true if the comment is terminated.
            //
            // If a comment is unterminated we should consider the end position
            // including.
            //
            // For the time being we consider all comments unterminated.
            return token.Kind == SyntaxKind.MultiLineCommentTrivia &&
                   token.Span.Start <= position &&
                   position <= token.Span.End;
        }

        private static bool InLiteral(SyntaxNode root, int position)
        {
            // TODO: This is only true if the literal is terminated (string, date literal).
            //
            // If a literal is unterminated we should consider the end position
            // including.
            //
            // For the time being we consider all literals unterminated.

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
                    if (InLiteral(child.AsNode(), position))
                        return true;
                }
                else
                {
                    var token = child.AsToken();
                    if (token.Kind.IsLiteral())
                        return true;
                }
            }

            return false;
        }

        private IEnumerable<Completion> GetGlobalSymbolCompletions(SemanticModel semanticModel, int position)
        {
            var symbols = semanticModel.LookupSymbols(position);
            if (!symbols.Any())
                symbols = semanticModel.LookupSymbols(position - 1);

            return from s in symbols
                   group s by s.Name
                   into g
                   select CreateSymbolCompletion(g.Key, g);
        }

        private IEnumerable<Completion> CreateSymbolCompletions(IEnumerable<Symbol> symbols)
        {
            return from s in symbols
                   group s by s.Name
                   into g
                   select CreateSymbolCompletion(g.Key, g);
        }

        private Completion CreateSymbolCompletion(string name, IEnumerable<Symbol> symbols)
        {
            var multiple = symbols.Skip(1).Any();
            if (!multiple)
                return CreateSymbolCompletion(symbols.First());

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
            var image = _glyphService.GetGlyph(NQueryGlyph.AmbiguousName);
            return new Completion(displayText, insertionText, description, image, null);
        }

        private Completion CreateSymbolCompletion(Symbol symbol)
        {
            var displayText = symbol.Name;
            var insertionText = symbol.Name;
            var description = symbol.ToString();
            var image = GetImage(symbol);
            return new Completion(displayText, insertionText, description, image, null);
        }

        private IEnumerable<Completion> GetKeywordCompletions()
        {
            var imageSource = _glyphService.GetGlyph(NQueryGlyph.Keyword);
            return from k in SyntaxFacts.GetKeywordKinds()
                   let text = k.GetText()
                   select new Completion(text, text, null, imageSource, null);
        }

        private ImageSource GetImage(Symbol symbol)
        {
            var glyph = GetGlyph(symbol);
            return glyph == null ? null : _glyphService.GetGlyph(glyph.Value);
        }

        private static NQueryGlyph? GetGlyph(Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Column:
                    return NQueryGlyph.Column;
                case SymbolKind.SchemaTable:
                    return NQueryGlyph.Table;
                case SymbolKind.DerivedTable:
                    return NQueryGlyph.Table;
                case SymbolKind.TableInstance:
                    return NQueryGlyph.TableRef;
                case SymbolKind.ColumnInstance:
                    return NQueryGlyph.Column;
                case SymbolKind.Variable:
                    return NQueryGlyph.Variable;
                case SymbolKind.Function:
                    return NQueryGlyph.Function;
                case SymbolKind.Property:
                    return NQueryGlyph.Property;
                case SymbolKind.Method:
                    return NQueryGlyph.Method;
                default:
                    return null;
            }
        }
    }
}
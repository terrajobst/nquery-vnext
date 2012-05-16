using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

using NQuery.Language.Semantic;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryCompletionSource : ICompletionSource
    {
        private readonly INQuerySemanticModelManager _semanticModelManager;
        private readonly INQueryGlyphService _glyphService;

        public NQueryCompletionSource(INQuerySemanticModelManager semanticModelManager, INQueryGlyphService glyphService)
        {
            _semanticModelManager = semanticModelManager;
            _glyphService = glyphService;
        }

        public void Dispose()
        {
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var textBuffer = session.TextView.TextBuffer;
            var snapshot = textBuffer.CurrentSnapshot;
            var position = session.GetTriggerPoint(textBuffer).GetPosition(snapshot);

            var semanticModel = _semanticModelManager.SemanticModel;
            if (semanticModel == null)
                return;

            var root = semanticModel.Compilation.SyntaxTree.Root;
            var tokenAtPosition = GetTokenAtPosition(root, position) ??
                                  GetTokenAtPosition(root, position - 1);
            var span = tokenAtPosition == null
                           ? new TextSpan(position, 0)
                           : tokenAtPosition.Value.Span;

            var symbolCompletions = GetSymbolCompletions(semanticModel, position);
            var keywordCompletions = GetKeywordCompletions();
            var completions = symbolCompletions.Concat(keywordCompletions).ToArray();
            Array.Sort(completions, (x, y) => x.InsertionText.CompareTo(y.InsertionText));

            var trackingSpan = snapshot.CreateTrackingSpan(span.Start, span.Length, SpanTrackingMode.EdgeInclusive);

            var completionSet = new CompletionSet("Tokens", "Tokens", trackingSpan, completions, null);
            completionSets.Add(completionSet);
        }

        private static SyntaxToken? GetTokenAtPosition(SyntaxNode root, int position)
        {
            if (!root.Span.Contains(position))
                return null;

            foreach (var nodeOrToken in root.GetChildren())
            {
                if (nodeOrToken.IsToken)
                {
                    if (nodeOrToken.Span.Contains(position))
                        return nodeOrToken.AsToken();
                }
                else
                {
                    var result = GetTokenAtPosition(nodeOrToken.AsNode(), position);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        private IEnumerable<Completion> GetSymbolCompletions(SemanticModel semanticModel, int position)
        {
            var symbols = semanticModel.LookupSymbols(position);
            if (!symbols.Any())
                symbols = semanticModel.LookupSymbols(position - 1);

            return from s in symbols
                   group s by s.Name
                   into g
                   select GetCompletion(g.Key, g);
        }

        private Completion GetCompletion(string name, IEnumerable<Symbol> symbols)
        {
            var multiple = symbols.Skip(1).Any();
            if (!multiple)
                return GetCompletion(symbols.First());

            var displayText = name;
            var insertionText = name;

            var sb = new StringBuilder();
            sb.AppendLine("Ambiguous Name:");
            foreach (var symbol in symbols)
            {
                sb.Append("  ");
                sb.AppendLine(symbol.ToString());
            }

            var description = sb.ToString();
            var image = _glyphService.GetGlyph(NQueryGlyph.AmbiguousName);
            return new Completion(displayText, insertionText, description, image, null);
        }

        private Completion GetCompletion(Symbol symbol)
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
                   let text = SyntaxFacts.GetText(k)
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
                default:
                    return null;
            }
        }
    }
}
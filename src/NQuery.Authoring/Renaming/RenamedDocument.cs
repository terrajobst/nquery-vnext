using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NQuery.Authoring.SymbolSearch;
using NQuery.Text;

namespace NQuery.Authoring.Renaming
{
    public sealed class RenamedDocument
    {
        private RenamedDocument(Document document)
        {
            IsRenamed = false;
            Document = Document;
            EditSpan = default(TextSpan);
            OtherSpans = ImmutableArray<TextSpan>.Empty;
        }

        private RenamedDocument(Document document, TextSpan editSpan, ImmutableArray<TextSpan> otherSpans)
        {
            IsRenamed = true;
            Document = document;
            EditSpan = editSpan;
            OtherSpans = otherSpans;
        }

        public static async Task<RenamedDocument> CreateAsync(Document document, TextChange change)
        {
            var semanticModel = await document.GetSemanticModelAsync();

            if (!TryApplyIdentifierEdit(semanticModel.SyntaxTree, change, out var token, out var newSpan, out var newIdentifer))
                return new RenamedDocument(document);

            var symbolSpan = semanticModel.FindSymbol(token.Span.Start);
            if (symbolSpan == null)
                return new RenamedDocument(document);

            var symbolSpans = semanticModel.FindUsages(symbolSpan.Value.Symbol).ToImmutableArray();
            if (!symbolSpans.Any(ss => ss.Kind == SymbolSpanKind.Definition))
                return new RenamedDocument(document);

            var resultingChanges = symbolSpans.Select(ss => new TextChange(ss.Span, newIdentifer)).ToImmutableArray();

            var changeSet = TextChangeSet.Create(resultingChanges);

            var newLength = newIdentifer.Length;
            var allSpans = symbolSpans.Select(ss => new TextSpan(changeSet.TranslatePosition(ss.Span.Start), newLength)).ToImmutableArray();
            var editSpan = new TextSpan(changeSet.TranslatePosition(symbolSpan.Value.Span.Start), newLength);
            var otherSpans = allSpans.Where(s => s != editSpan).ToImmutableArray();

            var newText = document.Text.WithChanges(resultingChanges);
            var newDocument = document.WithText(newText);

            return new RenamedDocument(newDocument, editSpan, otherSpans);
        }

        private static bool TryApplyIdentifierEdit(SyntaxTree syntaxTree, TextChange change, out SyntaxToken token, out TextSpan newSpan, out string newIdentifier)
        {
            token = null;
            newSpan = default(TextSpan);
            newIdentifier = null;

            token = FindOverlappingIdentifierOrKeyword(syntaxTree, change.Span);
            if (token == null)
                return false;

            var delta = change.NewText.Length - change.Span.Length;

            var editStart = change.Span.Start;
            var start = Math.Min(editStart, token.Span.Start);
            var end = token.Span.End + delta;

            var newText = syntaxTree.Text.WithChanges(change);
            newSpan = TextSpan.FromBounds(start, end);
            newIdentifier = newText.GetText(newSpan);

            return SyntaxFacts.IsValidIdentifierOrKeyword(newIdentifier);
        }

        private static SyntaxToken FindOverlappingIdentifierOrKeyword(SyntaxTree syntaxTree, TextSpan span)
        {
            var tokenAtStart = syntaxTree.Root.FindToken(span.Start);
            var tokenAtEnd = syntaxTree.Root.FindToken(span.End);
            var tokenBeforeEnd = tokenAtEnd.GetPreviousToken();

            // Case 1: Token contains span
            //
            //     [Span]
            // [----Token----]

            if (tokenAtStart == tokenAtEnd && tokenAtStart.Span.IntersectsWith(span) && tokenAtStart.Kind.IsIdentifierOrKeyword())
                return tokenAtStart;

            // Case 2: Token starts at end
            //
            // [Span]
            //      [----Token----]

            if (span.End == tokenAtEnd.Span.Start && tokenAtEnd.Kind.IsIdentifierOrKeyword())
                return tokenAtEnd;

            // Case 3: Beginning of token overlaps with span
            //
            // [Span]
            //    [----Token----]

            if (tokenAtEnd.Span.IntersectsWith(span) && tokenAtEnd.Kind.IsIdentifierOrKeyword())
                return tokenAtEnd;

            // Case 4: Token ends at start
            //
            //               [Span]
            // [----Token----]

            if (tokenBeforeEnd != null && tokenBeforeEnd.Span.End == span.Start && tokenBeforeEnd.Kind.IsIdentifierOrKeyword())
                return tokenBeforeEnd;

            // Case 5: End of token overlaps with span
            //
            //             [Span]
            // [----Token----]

            if (tokenAtStart.Span.IntersectsWith(span) && tokenAtStart.Kind.IsIdentifierOrKeyword())
                return tokenAtStart;

            return null;
        }

        public bool IsRenamed { get; }

        public Document Document { get; }

        public TextSpan EditSpan { get; }

        public ImmutableArray<TextSpan> OtherSpans { get; }

        public IEnumerable<TextSpan> Spans => new[] { EditSpan }.Concat(OtherSpans).OrderBy(s => s.Start);
    }
}

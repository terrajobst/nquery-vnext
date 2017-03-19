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
            OldDocument = document;
            NewDocument = document;
            OldSpans = ImmutableArray<TextSpan>.Empty;
            NewSpans = ImmutableArray<TextSpan>.Empty;
            Changes = ImmutableArray<TextChange>.Empty;
        }

        private RenamedDocument(Document oldDocument, Document newDocument, ImmutableArray<TextSpan> oldSpans, ImmutableArray<TextSpan> newSpans, ImmutableArray<TextChange> changes)
        {
            OldDocument = oldDocument;
            NewDocument = newDocument;
            OldSpans = oldSpans;
            NewSpans = newSpans;
            Changes = changes;
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
            var oldSpans = symbolSpans.Select(ss => ss.Span).ToImmutableArray();
            var newSpans = symbolSpans.Select(ss => new TextSpan(changeSet.TranslatePosition(ss.Span.Start), newLength)).ToImmutableArray();

            var newText = document.Text.WithChanges(resultingChanges);
            var newDocument = document.WithText(newText);

            return new RenamedDocument(document, newDocument, oldSpans, newSpans, resultingChanges);
        }

        public async Task<RenamedDocument> ApplyAsync(TextChange change)
        {
            var newSyntaxTree = await NewDocument.GetSyntaxTreeAsync();

            for (int i = 0; i < NewSpans.Length; i++)
            {
                var oldSpan = OldSpans[i];
                var newSpan = OldSpans[i];

                if (newSpan.IntersectsWith(change.Span))
                {
                    if (TryApplyIdentifierEdit(newSyntaxTree, change, out var token, out var s, out var newIdentifier))
                    {
                        var modifiedChange = TextChange.ForReplacement(oldSpan, newIdentifier);
                        return await CreateAsync(OldDocument, modifiedChange);
                    }
                }
            }

            return new RenamedDocument(NewDocument);
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

        public bool IsRenamed => Changes.Any();

        public Document OldDocument { get; }

        public Document NewDocument { get; }
       
        public ImmutableArray<TextSpan> OldSpans { get; }

        public ImmutableArray<TextSpan> NewSpans { get; }

        public ImmutableArray<TextChange> Changes { get; }
    }
}

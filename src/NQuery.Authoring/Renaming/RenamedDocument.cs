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
            Spans = ImmutableArray<TextSpan>.Empty;
            Changes = ImmutableArray<TextChange>.Empty;
        }

        private RenamedDocument(Document oldDocument, Document newDocument, ImmutableArray<TextSpan> newSpans, ImmutableArray<TextChange> changes)
        {
            OldDocument = oldDocument;
            NewDocument = newDocument;
            Spans = newSpans;
            Changes = changes;
        }

        public static async Task<RenamedDocument> CreateAsync(Document document, int position)
        {
            var semanticModel = await document.GetSemanticModelAsync();

            var symbolSpan = semanticModel.FindSymbol(position);
            if (symbolSpan != null)
            {
                var symbolSpans = semanticModel.FindUsages(symbolSpan.Value.Symbol).ToImmutableArray();
                if (symbolSpans.Any(ss => ss.Kind == SymbolSpanKind.Definition))
                {
                    var resultSpans = symbolSpans.Select(ss => ss.Span).ToImmutableArray();
                    var renames = ImmutableArray<TextChange>.Empty;

                    return new RenamedDocument(document, document, resultSpans, renames);
                }
            }

            return new RenamedDocument(document);
        }

        public static async Task<RenamedDocument> CreateAsync(Document oldDocument, Document newDocument)
        {
            if (TryGetChange(oldDocument, newDocument, out var change))
            {
                if (TryApplyIdentifierEdit(await oldDocument.GetSyntaxTreeAsync(), change, out var token, out var newIdentifierSpan, out var newIdentifer) ||
                    TryPostIdentifierEdit(await newDocument.GetSyntaxTreeAsync(), change, out token, out newIdentifierSpan, out newIdentifer))
                {
                    var semanticModel = await oldDocument.GetSemanticModelAsync();
                    var symbolSpan = semanticModel.FindSymbol(token.Span.Start);
                    if (symbolSpan != null)
                    {
                        var symbolSpans = semanticModel.FindUsages(symbolSpan.Value.Symbol).ToImmutableArray();
                        if (symbolSpans.Any(ss => ss.Kind == SymbolSpanKind.Definition))
                        {
                            var oldToNewChangeSet = TextChangeSet.Create(new[] { change });
                            var newDocumentSpans = symbolSpans.Select(ss => new TextSpan(oldToNewChangeSet.TranslatePosition(ss.Span.Start), ss.Span.Length))
                                                              .ToImmutableArray();

                            var renames = newDocumentSpans.Where(s => !newIdentifierSpan.IntersectsWith(s))
                                                          .Select(s => TextChange.ForReplacement(s, newIdentifer))
                                                          .ToImmutableArray();

                            var resultText = newDocument.Text.WithChanges(renames);
                            var resultDocument = newDocument.WithText(resultText);

                            var renameSet = TextChangeSet.Create(renames);

                            var resultSpans = newDocumentSpans.Select(s => new TextSpan(renameSet.TranslatePosition(s.Start), newIdentifer.Length))
                                                              .ToImmutableArray();

                            return new RenamedDocument(newDocument, resultDocument, resultSpans, renames);
                        }
                    }
                }
            }

            return new RenamedDocument(newDocument);
        }

        public Task<RenamedDocument> ApplyAsync(TextChange change)
        {
            var newText = NewDocument.Text.WithChanges(change);
            var newDocument = NewDocument.WithText(newText);
            return ApplyAsync(newDocument);
        }

        public async Task<RenamedDocument> ApplyAsync(Document document)
        {
            if (TryGetChange(NewDocument, document, out var change))
            {
                var newSyntaxTree = await NewDocument.GetSyntaxTreeAsync();

                foreach (var newSpan in Spans)
                {
                    if (newSpan.IntersectsWith(change.Span))
                    {
                        if (TryApplyIdentifierEdit(newSyntaxTree, change, out var token, out var s, out var newIdentifier))
                            return await CreateAsync(NewDocument, document);
                    }
                }
            }

            return new RenamedDocument(NewDocument);
        }

        private static bool TryGetChange(Document oldDocument, Document newDocument, out TextChange change)
        {
            change = default(TextChange);

            var changes = newDocument.Text.GetChanges(oldDocument.Text).GetEnumerator();
            if (!changes.MoveNext())
                return false;

            var first = changes.Current;

            if (changes.MoveNext())
                return false;

            change = first;
            return true;
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

            return newIdentifier.Length == 0 ||
                   SyntaxFacts.IsValidIdentifierOrKeyword(newIdentifier);
        }

        private static bool TryPostIdentifierEdit(SyntaxTree syntaxTree, TextChange change, out SyntaxToken token, out TextSpan newSpan, out string newIdentifier)
        {
            token = null;
            newSpan = default(TextSpan);
            newIdentifier = null;

            var span = new TextSpan(change.Span.Start, change.NewText.Length);

            token = FindOverlappingIdentifierOrKeyword(syntaxTree, span);
            if (token == null)
                return false;

            newSpan = token.Span;
            newIdentifier = syntaxTree.Text.GetText(newSpan);

            return newIdentifier.Length == 0 ||
                   SyntaxFacts.IsValidIdentifierOrKeyword(newIdentifier);
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

        public bool IsValid => Spans.Any();

        public Document OldDocument { get; }

        public Document NewDocument { get; }
       
        public ImmutableArray<TextSpan> Spans { get; }

        public ImmutableArray<TextChange> Changes { get; }
    }
}

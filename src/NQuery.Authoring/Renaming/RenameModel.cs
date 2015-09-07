using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using NQuery.Authoring.SymbolSearch;
using NQuery.Text;

namespace NQuery.Authoring.Renaming
{
    public sealed class RenameModel
    {
        private readonly Document _document;
        private readonly SymbolSpan? _symbolSpan;

        private RenameModel(Document document, SymbolSpan? symbolSpan)
        {
            _document = document;
            _symbolSpan = symbolSpan;
        }

        public static async Task<RenameModel> CreateAsync(Document document, int position)
        {
            var semanticModel = await document.GetSemanticModelAsync();
            var symbolSpan = semanticModel.FindSymbol(position);
            return new RenameModel(document, symbolSpan);
        }

        public bool CanRename
        {
            get { return _symbolSpan != null && Renamer.CanBeRenamed(_symbolSpan.Value.Symbol); }
        }

        public SymbolSpan? SymbolSpan
        {
            get { return _symbolSpan; }
        }

        public async Task<RenamedDocument> RenameAsync(string newName)
        {
            if (!CanRename)
                throw new InvalidOperationException("Rename cannot be performed at the present location");

            // Create a valid name

            newName = SyntaxFacts.GetValidIdentifier(newName);

            // Find symbol spans in old document

            var semanticModel = await _document.GetSemanticModelAsync();
            var usages = semanticModel.FindUsages(_symbolSpan.Value.Symbol).ToImmutableArray();

            // Compute changes

            var changes = new TextChangeSet();
            foreach (var usage in usages)
                changes.ReplaceText(usage.Span, newName);

            var text = semanticModel.Compilation.SyntaxTree.Text;
            var newText = text.WithChanges(changes);

            // Compute locations

            var locations = ImmutableArray.CreateBuilder<TextSpan>();

            foreach (var usage in usages)
            {
                var newSpan = ApplyChanges(usage.Span, changes);
                var location = GetSpanWithBracketsOrQuotes(newName, newSpan);
                locations.Add(location);
            }

            var newDocument = _document.WithText(newText);
            return new RenamedDocument(newDocument, locations.ToImmutable());
        }

        private static TextSpan GetSpanWithBracketsOrQuotes(string newName, TextSpan textSpan)
        {
            var isQuoted = newName.Length > 1 && (newName[0] == '[' || newName[0] == '"');
            return isQuoted
                ? new TextSpan(textSpan.Start + 1, textSpan.Length - 2)
                : textSpan;
        }

        private static TextSpan ApplyChanges(TextSpan textSpan, IEnumerable<TextChange> changes)
        {
            return changes.OrderByDescending(c => c.Span.Start)
                          .Aggregate(textSpan, ApplyChange);
        }

        private static TextSpan ApplyChange(TextSpan textSpan, TextChange textChange)
        {
            // NOTE: This isn't generalized. It works for the identifiers
            //       because with know changes cannot overlap.

            var delta = textChange.NewText.Length - textChange.Span.Length;

            if (textSpan.IntersectsWith(textChange.Span))
            {
                var newStart = Math.Min(textSpan.Start, textChange.Span.Start);
                var newLength = textSpan.Length + delta;
                return new TextSpan(newStart, newLength);
            }

            return textChange.Span.End < textSpan.Start
                ? new TextSpan(textSpan.Start + delta, textSpan.Length)
                : textSpan;
        }
    }
}
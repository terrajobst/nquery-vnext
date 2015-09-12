using System;
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

            // Find symbol spans in old document

            var semanticModel = await _document.GetSemanticModelAsync();
            var usages = semanticModel.FindUsages(_symbolSpan.Value.Symbol).ToImmutableArray();

            // Compute changes & new locations

            var changes = new TextChangeSet();
            var locations = ImmutableArray.CreateBuilder<TextSpan>();

            var oldLength = _symbolSpan.Value.Symbol.Name.Length;
            var newLength = newName.Length;
            var delta = newLength - oldLength;
            var deltaAccumulator = 0;

            foreach (var usage in usages.OrderBy(u => u.Span.Start))
            {
                changes.ReplaceText(usage.Span, newName);

                var location = new TextSpan(usage.Span.Start + deltaAccumulator, newLength);
                locations.Add(location);
                deltaAccumulator += delta;
            }

            var text = semanticModel.Compilation.SyntaxTree.Text;
            var newText = text.WithChanges(changes);

            var newDocument = _document.WithText(newText);
            return new RenamedDocument(newDocument, changes.ToImmutableArray(), locations.ToImmutable());
        }
    }
}
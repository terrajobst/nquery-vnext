using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor.Squiggles
{
    internal sealed class NQuerySemanticErrorTagger : NQueryErrorTagger
    {
        private readonly INQueryDocument _document;

        public NQuerySemanticErrorTagger(INQueryDocument document)
            : base(PredefinedErrorTypeNames.CompilerError)
        {
            _document = document;
            _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
            InvalidateTags();
        }

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<Diagnostic>>> GetRawTagsAsync()
        {
            var semanticModel = await _document.GetSemanticModelAsync();
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var snapshot = _document.GetTextSnapshot(syntaxTree);
            return Tuple.Create(snapshot, semanticModel.GetDiagnostics());
        }
    }
}
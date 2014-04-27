using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

using NQuery.Authoring.Document;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.Squiggles
{
    internal sealed class NQuerySemanticErrorTagger : NQueryErrorTagger
    {
        private readonly NQueryDocument _document;

        public NQuerySemanticErrorTagger(NQueryDocument document)
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
            var snapshot = semanticModel.GetTextSnapshot();
            return Tuple.Create(snapshot, semanticModel.GetDiagnostics());
        }
    }
}
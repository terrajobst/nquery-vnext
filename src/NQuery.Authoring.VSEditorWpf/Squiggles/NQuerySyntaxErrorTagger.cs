using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

using NQuery.Authoring.Document;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.Squiggles
{
    internal sealed class NQuerySyntaxErrorTagger : NQueryErrorTagger
    {
        private readonly NQueryDocument _document;

        public NQuerySyntaxErrorTagger(NQueryDocument document)
            : base(PredefinedErrorTypeNames.SyntaxError)
        {
            _document = document;
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
            InvalidateTags();
        }

        private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs eventArgs)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<Diagnostic>>> GetRawTagsAsync()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var snapshot = syntaxTree.GetTextSnapshot();
            return Tuple.Create(snapshot, syntaxTree.GetDiagnostics());
        }
    }
}
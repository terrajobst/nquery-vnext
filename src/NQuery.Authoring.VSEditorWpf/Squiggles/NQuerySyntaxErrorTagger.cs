using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

namespace NQuery.Authoring.VSEditorWpf.Squiggles
{
    internal sealed class NQuerySyntaxErrorTagger : NQueryErrorTagger
    {
        private readonly Workspace _workspace;

        public NQuerySyntaxErrorTagger(Workspace workspace)
            : base(PredefinedErrorTypeNames.SyntaxError)
        {
            _workspace = workspace;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            InvalidateTags();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<Diagnostic>>> GetRawTagsAsync()
        {
            var document = _workspace.CurrentDocument;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var snapshot = document.GetTextSnapshot();
            return Tuple.Create(snapshot, syntaxTree.GetDiagnostics());
        }
    }
}
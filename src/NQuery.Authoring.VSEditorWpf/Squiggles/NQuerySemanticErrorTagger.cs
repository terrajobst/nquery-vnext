using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

namespace NQuery.Authoring.VSEditorWpf.Squiggles
{
    internal sealed class NQuerySemanticErrorTagger : NQueryErrorTagger
    {
        private readonly Workspace _workspace;

        public NQuerySemanticErrorTagger(Workspace workspace)
            : base(PredefinedErrorTypeNames.CompilerError)
        {
            _workspace = workspace;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            InvalidateTagsAsync();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            InvalidateTagsAsync();
        }

        protected override async Task<(ITextSnapshot Snapshot, IEnumerable<Diagnostic> RawTags)> GetRawTagsAsync()
        {
            var document = _workspace.CurrentDocument;
            var semanticModel = await document.GetSemanticModelAsync();
            var snapshot = document.GetTextSnapshot();
            return (snapshot, semanticModel.GetDiagnostics());
        }
    }
}
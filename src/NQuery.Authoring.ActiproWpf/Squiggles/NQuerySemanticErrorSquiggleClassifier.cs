using ActiproSoftware.Text;

using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Squiggles
{
    internal sealed class NQuerySemanticErrorSquiggleClassifier : NQuerySquiggleClassifier
    {
        private readonly Workspace _workspace;

        public NQuerySemanticErrorSquiggleClassifier(ICodeDocument document)
            : base(ClassificationTypes.CompilerError, typeof(NQuerySemanticErrorSquiggleClassifier).Name, null, document, true)
        {
            _workspace = document.GetWorkspace();
            if (_workspace is null)
                return;

            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            UpdateTags();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateTags();
        }

        protected override async Task<(SourceText Text, IEnumerable<Diagnostic> Diagnostics)> GetDiagnosticsAsync()
        {
            var document = _workspace.CurrentDocument;
            var semanticModel = await document.GetSemanticModelAsync();
            var diagnostics = await Task.Run(() => semanticModel.GetDiagnostics());
            return (document.Text, diagnostics);
        }
    }
}
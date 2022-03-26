using ActiproSoftware.Text;

using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Squiggles
{
    internal sealed class NQuerySyntaxErrorSquiggleClassifier : NQuerySquiggleClassifier
    {
        private readonly Workspace _workspace;

        public NQuerySyntaxErrorSquiggleClassifier(ICodeDocument document)
            : base(ClassificationTypes.SyntaxError, nameof(NQuerySemanticErrorSquiggleClassifier), null, document, true)
        {
            _workspace = document.GetWorkspace();
            if (_workspace is null)
                return;

            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            UpdateTagsAsync();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateTagsAsync();
        }

        protected override async Task<(SourceText Text, IEnumerable<Diagnostic> Diagnostics)> GetDiagnosticsAsync()
        {
            var document = _workspace.CurrentDocument;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var diagnostics = await Task.Run(() => syntaxTree.GetDiagnostics());
            return (document.Text, diagnostics);
        }
    }
}
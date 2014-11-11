using System;

using ActiproSoftware.Text;

namespace NQuery.Authoring.ActiproWpf
{
    internal sealed class NQueryParseDataSynchronizer
    {
        private readonly ICodeDocument _codeDocument;
        private readonly Workspace _workspace;

        public NQueryParseDataSynchronizer(ICodeDocument codeDocument, Workspace workspace)
        {
            _codeDocument = codeDocument;
            _workspace = workspace;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
        }

        private async void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            var document = _workspace.CurrentDocument;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            _codeDocument.ParseData = new NQueryParseData(syntaxTree);
        }
    }
}
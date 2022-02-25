using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ActiproSoftware.Text;

using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Squiggles
{
    internal sealed class NQuerySyntaxErrorSquiggleClassifier : NQuerySquiggleClassifier
    {
        private readonly Workspace _workspace;

        public NQuerySyntaxErrorSquiggleClassifier(ICodeDocument document)
            : base(ClassificationTypes.SyntaxError, typeof(NQuerySemanticErrorSquiggleClassifier).Name, null, document, true)
        {
            _workspace = document.GetWorkspace();
            if (_workspace == null)
                return;

            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            UpdateTags();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateTags();
        }

        protected override async Task<Tuple<SourceText, IEnumerable<Diagnostic>>> GetDiagnosticsAsync()
        {
            var document = _workspace.CurrentDocument;
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var diagnostics = await Task.Run(() => syntaxTree.GetDiagnostics());
            return Tuple.Create(document.Text, diagnostics);
        }
    }
}
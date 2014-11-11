using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            if (_workspace == null)
                return;

            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            UpdateTags();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateTags();
        }

        protected override async Task<Tuple<SourceText, IEnumerable<Diagnostic>>> GetDiagnosticsAync()
        {
            var document = _workspace.CurrentDocument;
            var semanticModel = await document.GetSemanticModelAsync();
            var diagnostics = await Task.Run(() => semanticModel.GetDiagnostics());
            return Tuple.Create(document.Text, diagnostics);
        }
    }
}
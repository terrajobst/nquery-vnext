using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ActiproSoftware.Text;

using NQuery.Authoring.Document;

namespace NQuery.Authoring.ActiproWpf.Squiggles
{
    internal sealed class NQuerySyntaxErrorSquiggleClassifier : NQuerySquiggleClassifier
    {
        private readonly NQueryDocument _queryDocument;

        public NQuerySyntaxErrorSquiggleClassifier(ICodeDocument document)
            : base(ClassificationTypes.SyntaxError, typeof(NQuerySemanticErrorSquiggleClassifier).Name, null, document, true)
        {
            _queryDocument = document.GetNQueryDocument();
            if (_queryDocument == null)
                return;

            _queryDocument.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
            UpdateTags();
        }

        private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs e)
        {
            UpdateTags();
        }

        protected override async Task<Tuple<SyntaxTree, IEnumerable<Diagnostic>>> GetDiagnosticsAync()
        {
            var syntaxTree = await _queryDocument.GetSyntaxTreeAsync();
            var diagnostics = await Task.Run(() => syntaxTree.GetDiagnostics());
            return Tuple.Create(syntaxTree, diagnostics);
        }
    }
}
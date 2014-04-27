using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ActiproSoftware.Text;

using NQuery.Authoring.Document;

namespace NQuery.Authoring.ActiproWpf.Squiggles
{
    internal sealed class NQuerySemanticErrorSquiggleClassifier : NQuerySquiggleClassifier
    {
        private readonly NQueryDocument _queryDocument;

        public NQuerySemanticErrorSquiggleClassifier(ICodeDocument document)
            : base(ClassificationTypes.CompilerError, typeof(NQuerySemanticErrorSquiggleClassifier).Name, null, document, true)
        {
            _queryDocument = document.GetNQueryDocument();
            if (_queryDocument == null)
                return;

            _queryDocument.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
            UpdateTags();
        }

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs eventArgs)
        {
            UpdateTags();
        }

        protected override async Task<Tuple<SyntaxTree, IEnumerable<Diagnostic>>> GetDiagnosticsAync()
        {
            var semanticModel = await _queryDocument.GetSemanticModelAsync();
            var diagnostics = await Task.Run(() => semanticModel.GetDiagnostics());
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            return Tuple.Create(syntaxTree, diagnostics);
        }
    }
}
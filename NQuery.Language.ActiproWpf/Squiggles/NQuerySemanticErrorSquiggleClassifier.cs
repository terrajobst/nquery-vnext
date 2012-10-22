using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ActiproSoftware.Text;
using NQuery.Language;

namespace NQueryViewerActiproWpf
{
    internal sealed class NQuerySemanticErrorSquiggleClassifier : NQuerySquiggleClassifier
    {
        public NQuerySemanticErrorSquiggleClassifier(ICodeDocument document)
            : base(ClassificationTypes.CompilerError, typeof(NQuerySemanticErrorSquiggleClassifier).Name, null, document, true)
        {
            var nqueryDocument = document as NQueryDocument;
            if (nqueryDocument != null)
            {
                nqueryDocument.SemanticModelChanged += NqueryDocumentOnSemanticModelChanged;
                UpdateTags();
            }
        }

        private void NqueryDocumentOnSemanticModelChanged(object sender, EventArgs eventArgs)
        {
            UpdateTags();
        }

        protected override async Task<Tuple<NQueryParseData, IEnumerable<Diagnostic>>> GetDiagnosticsAync()
        {
            var semanticData = await Document.GetSemanticDataAsync();
            var diagnostics = await Task.Run(() => semanticData.SemanticModel.GetDiagnostics());
            return Tuple.Create(semanticData.ParseData, diagnostics);
        }
    }
}
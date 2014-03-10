using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Parsing;

namespace NQuery.Authoring.ActiproWpf.Squiggles
{
    internal sealed class NQuerySyntaxErrorSquiggleClassifier : NQuerySquiggleClassifier
    {
        public NQuerySyntaxErrorSquiggleClassifier(ICodeDocument document)
            : base(ClassificationTypes.SyntaxError, typeof(NQuerySemanticErrorSquiggleClassifier).Name, null, document, true)
        {
            var nqueryDocument = document as NQueryDocument;
            if (nqueryDocument != null)
            {
                nqueryDocument.ParseDataChanged += NqueryDocumentOnParseDataChanged;
                UpdateTags();
            }
        }

        private void NqueryDocumentOnParseDataChanged(object sender, ParseDataPropertyChangedEventArgs parseDataPropertyChangedEventArgs)
        {
            UpdateTags();
        }

        protected override async Task<Tuple<SyntaxTree, IEnumerable<Diagnostic>>> GetDiagnosticsAync()
        {
            var parseData = await Document.GetParseDataAsync();
            var syntaxTree = parseData.SyntaxTree;
            var diagnostics = await Task.Run(() => syntaxTree.GetDiagnostics());
            return Tuple.Create(syntaxTree, diagnostics);
        }
    }
}
using System;

using ActiproSoftware.Text;

using NQuery.Authoring.Document;

namespace NQuery.Authoring.ActiproWpf
{
    internal sealed class NQueryParseDataSynchronizer
    {
        private readonly ICodeDocument _codeDocument;
        private readonly NQueryDocument _document;

        public NQueryParseDataSynchronizer(ICodeDocument codeDocument, NQueryDocument document)
        {
            _codeDocument = codeDocument;
            _document = document;
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
        }

        private async void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs e)
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            _codeDocument.ParseData = new NQueryParseData(syntaxTree);
        }
    }
}
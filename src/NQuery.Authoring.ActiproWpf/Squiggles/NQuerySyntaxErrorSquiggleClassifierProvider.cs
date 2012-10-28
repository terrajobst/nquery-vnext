using System;

using ActiproSoftware.Text.Tagging.Implementation;

namespace NQuery.Authoring.ActiproWpf.Squiggles
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySyntaxErrorSquiggleClassifierProvider>))]
    internal sealed class NQuerySyntaxErrorSquiggleClassifierProvider : CodeDocumentTaggerProvider<NQuerySyntaxErrorSquiggleClassifier>
    {
    }
}
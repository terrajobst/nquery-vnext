using ActiproSoftware.Text.Tagging.Implementation;

namespace NQueryViewerActiproWpf
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySyntaxErrorSquiggleClassifierProvider>))]
    internal sealed class NQuerySyntaxErrorSquiggleClassifierProvider : CodeDocumentTaggerProvider<NQuerySyntaxErrorSquiggleClassifier>
    {
    }
}
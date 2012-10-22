using ActiproSoftware.Text.Tagging.Implementation;

namespace NQueryViewerActiproWpf
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySemanticErrorSquiggleClassifierProvider>))]
    internal sealed class NQuerySemanticErrorSquiggleClassifierProvider : CodeDocumentTaggerProvider<NQuerySemanticErrorSquiggleClassifier>
    {
    }
}
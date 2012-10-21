using ActiproSoftware.Text.Tagging.Implementation;

namespace NQueryViewerActiproWpf
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySemanticClassifier>))]
    internal sealed class NQuerySemanticClassifierProvider : CodeDocumentTaggerProvider<NQuerySemanticClassifier>
    {
    }
}
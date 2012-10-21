using ActiproSoftware.Text.Tagging.Implementation;

namespace NQueryViewerActiproWpf
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySyntacticClassifier>))]
    internal sealed class NQuerySyntacticClassifierProvider : CodeDocumentTaggerProvider<NQuerySyntacticClassifier>
    {
    }
}
using System.ComponentModel.Composition;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Tagging.Implementation;

namespace NQueryViewerActiproWpf
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySemanticClassifier>))]
    internal sealed class NQuerySemanticClassifierProvider : CodeDocumentTaggerProvider<NQuerySemanticClassifier>
    {
    }
}
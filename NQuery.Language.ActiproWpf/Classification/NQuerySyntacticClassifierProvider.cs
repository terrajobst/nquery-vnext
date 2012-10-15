using System.ComponentModel.Composition;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Tagging.Implementation;

namespace NQueryViewerActiproWpf
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySyntacticClassifier>))]
    internal sealed class NQuerySyntacticClassifierProvider : CodeDocumentTaggerProvider<NQuerySyntacticClassifier>
    {
    }
}
using System;

using ActiproSoftware.Text.Tagging.Implementation;

namespace NQuery.Authoring.ActiproWpf.Classification
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySemanticClassifier>))]
    internal sealed class NQuerySemanticClassifierProvider : CodeDocumentTaggerProvider<NQuerySemanticClassifier>
    {
    }
}
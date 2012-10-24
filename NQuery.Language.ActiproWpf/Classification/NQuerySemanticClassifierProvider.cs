using System;

using ActiproSoftware.Text.Tagging.Implementation;

namespace NQuery.Language.ActiproWpf.Classification
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySemanticClassifier>))]
    internal sealed class NQuerySemanticClassifierProvider : CodeDocumentTaggerProvider<NQuerySemanticClassifier>
    {
    }
}
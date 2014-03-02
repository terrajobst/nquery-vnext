using System;

using ActiproSoftware.Text.Tagging.Implementation;

namespace NQuery.Authoring.ActiproWpf.Classification
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQueryUnnecessaryCodeClassifier>))]
    internal sealed class NQueryUnnecessaryCodeClassifierProvider : CodeDocumentTaggerProvider<NQueryUnnecessaryCodeClassifier>
    {
    }
}
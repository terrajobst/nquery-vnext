using System;

using ActiproSoftware.Text.Tagging.Implementation;

namespace NQuery.Authoring.ActiproWpf.Classification
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySyntacticClassifier>))]
    internal sealed class NQuerySyntacticClassifierProvider : CodeDocumentTaggerProvider<NQuerySyntacticClassifier>
    {
    }
}
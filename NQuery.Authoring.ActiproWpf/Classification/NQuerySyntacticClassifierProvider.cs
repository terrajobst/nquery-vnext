using System;

using ActiproSoftware.Text.Tagging.Implementation;

namespace NQuery.Language.ActiproWpf.Classification
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySyntacticClassifier>))]
    internal sealed class NQuerySyntacticClassifierProvider : CodeDocumentTaggerProvider<NQuerySyntacticClassifier>
    {
    }
}
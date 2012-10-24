using System;

using ActiproSoftware.Text.Tagging.Implementation;

namespace NQuery.Language.ActiproWpf.Squiggles
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySemanticErrorSquiggleClassifierProvider>))]
    internal sealed class NQuerySemanticErrorSquiggleClassifierProvider : CodeDocumentTaggerProvider<NQuerySemanticErrorSquiggleClassifier>
    {
    }
}
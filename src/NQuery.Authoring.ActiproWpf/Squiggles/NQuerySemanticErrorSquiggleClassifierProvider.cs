using System;

using ActiproSoftware.Text.Tagging.Implementation;

namespace NQuery.Authoring.ActiproWpf.Squiggles
{
    [ExportLanguageService(typeof(CodeDocumentTaggerProvider<NQuerySemanticErrorSquiggleClassifierProvider>))]
    internal sealed class NQuerySemanticErrorSquiggleClassifierProvider : CodeDocumentTaggerProvider<NQuerySemanticErrorSquiggleClassifier>
    {
    }
}
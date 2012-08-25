using System.Collections.Generic;

using Microsoft.VisualStudio.Language.Intellisense;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryCompletionSource : ICompletionSource
    {
        private readonly INQuerySemanticModelManager _semanticModelManager;
        private readonly INQueryGlyphService _glyphService;

        public NQueryCompletionSource(INQuerySemanticModelManager semanticModelManager, INQueryGlyphService glyphService)
        {
            _semanticModelManager = semanticModelManager;
            _glyphService = glyphService;
        }

        public void Dispose()
        {
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var completionSet = new NQueryCompletionSet(session, _semanticModelManager, _glyphService);
            completionSets.Add(completionSet);
        }
    }
}
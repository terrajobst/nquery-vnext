using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Language.Intellisense;

using NQuery.Authoring.Wpf;

namespace NQuery.Authoring.VSEditorWpf.Completion
{
    internal sealed class NQueryCompletionSource : ICompletionSource
    {
        private readonly INQueryGlyphService _glyphService;

        public NQueryCompletionSource(INQueryGlyphService glyphService)
        {
            _glyphService = glyphService;
        }

        public void Dispose()
        {
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var completionModel = session.Properties.GetProperty<ICompletionModelManager>(typeof(ICompletionModelManager));
            var completionSet = new NQueryCompletionSet(session, _glyphService, completionModel);
            completionSets.Add(completionSet);
        }
    }
}
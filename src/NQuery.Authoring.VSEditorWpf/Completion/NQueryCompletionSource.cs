using Microsoft.VisualStudio.Language.Intellisense;

namespace NQuery.Authoring.VSEditorWpf.Completion
{
    internal sealed class NQueryCompletionSource : ICompletionSource
    {
        public void Dispose()
        {
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var completionModel = session.Properties.GetProperty<ICompletionModelManager>(typeof(ICompletionModelManager));
            var completionSet = new NQueryCompletionSet(session, completionModel);
            completionSets.Add(completionSet);
        }
    }
}
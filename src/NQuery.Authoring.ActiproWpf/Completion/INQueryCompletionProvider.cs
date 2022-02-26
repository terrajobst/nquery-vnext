using System.Collections.ObjectModel;

using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

namespace NQuery.Authoring.ActiproWpf.Completion
{
    public interface INQueryCompletionProvider : ICompletionProvider
    {
        Collection<Authoring.Completion.ICompletionProvider> Providers { get; }
    }
}
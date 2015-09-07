using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    [Export(typeof(IRenameServiceProvider))]
    internal sealed class RenameServiceProvider : IRenameServiceProvider
    {
        public IRenameService GetRenameService(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new RenameService(textBuffer));
        }
    }
}
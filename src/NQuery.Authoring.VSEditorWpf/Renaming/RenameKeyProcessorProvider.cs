using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    [Export(typeof(IKeyProcessorProvider))]
    [Name("NQuery.RenameKeyProcessorProvider")]
    [ContentType("NQuery")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class RenameKeyProcessorProvider : IKeyProcessorProvider
    {
        [Import]
        public IRenameServiceProvider RenameServiceProvider { get; set; }

        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var renameService = RenameServiceProvider.GetRenameService(wpfTextView.TextBuffer);
                return new RenameKeyProcessor(wpfTextView, renameService);
            });
        }
    }
}
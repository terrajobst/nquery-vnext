
using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("NQuery")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class RenameSelectionTracker : IWpfTextViewCreationListener
    {
        [Import]
        public IRenameServiceProvider RenameServiceProvider { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            var renameService = RenameServiceProvider.GetRenameService(textView.TextBuffer);

            EventHandler<EventArgs> updated = (s, e) =>
            {
                if (renameService.ActiveSession != null)
                {
                    var position = textView.Caret.Position.BufferPosition;
                    var locationAtCaret = renameService.ActiveSession.Locations.Single(l => l.Contains(position) ||
                                                                                            l.End == position);
                    textView.Selection.Select(locationAtCaret, false);
                }
            };

            renameService.ActiveSessionChanged += updated;

            textView.Closed += (s, e) =>
            {
                renameService.ActiveSessionChanged -= updated;
            };
        }
    }
}

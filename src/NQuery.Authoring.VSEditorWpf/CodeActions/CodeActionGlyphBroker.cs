using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    [Export(typeof(ICodeActionGlyphBroker))]
    internal sealed class CodeActionGlyphBroker : ICodeActionGlyphBroker
    {
        public ICodeActionGlyphController GetController(ITextView textView)
        {
            return textView.Properties.GetProperty<NQueryCodeActionsMargin>(typeof(NQueryCodeActionsMargin));
        }
    }
}
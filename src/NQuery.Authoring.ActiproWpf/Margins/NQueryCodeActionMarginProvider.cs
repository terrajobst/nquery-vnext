using System;

using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Margins;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Margins.Implementation;

using NQuery.Authoring.ActiproWpf.CodeActions;

namespace NQuery.Authoring.ActiproWpf.Margins
{
    public class NQueryEditorViewMarginFactory : IEditorViewMarginFactory
    {
        public IEditorViewMarginCollection CreateMargins(IEditorView view)
        {
            return new EditorViewMarginCollection
                   {
                       new NQueryEditorViewCodeActionMargin(view)
                   };
        }
    }
}
using System;

using Microsoft.VisualStudio.Text.Tagging;

namespace NQuery.Authoring.VSEditorWpf.Highlighting
{
    public sealed class HighlightTag : TextMarkerTag
    {
        public HighlightTag()
            : base(@"bracehighlight")
        {
        }
    }
}
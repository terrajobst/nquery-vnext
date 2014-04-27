using System;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.VSEditorWpf.Text;

using TextBuffer = NQuery.Text.TextBuffer;

namespace NQuery.Authoring.VSEditorWpf.Document
{
    public static class TextBufferExensions
    {
        public static ITextSnapshot GetTextSnapshot(this TextBuffer textBuffer)
        {
            var snapshotTextBuffer = textBuffer as SnapshotTextBuffer;
            return snapshotTextBuffer == null ? null : snapshotTextBuffer.Snapshot;
        }

        public static ITextSnapshot GetTextSnapshot(this SyntaxTree syntaxTree)
        {
            return syntaxTree.TextBuffer.GetTextSnapshot();
        }

        public static ITextSnapshot GetTextSnapshot(this SemanticModel semanticModel)
        {
            return semanticModel.Compilation.SyntaxTree.GetTextSnapshot();
        }

        public static int GetCaretPosition(this ITextView textView, ITextSnapshot snapshot)
        {
            return textView.Caret.Position.BufferPosition.TranslateTo(snapshot, PointTrackingMode.Negative);
        }
    }
}
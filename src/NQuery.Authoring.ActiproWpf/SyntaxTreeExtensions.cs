using System;

using ActiproSoftware.Text;

using NQuery.Authoring.ActiproWpf.Text;

namespace NQuery.Authoring.ActiproWpf
{
    public static class SyntaxTreeExtensions
    {
        public static ITextSnapshot GetTextSnapshot(this SyntaxTree syntaxTree)
        {
            var textBuffer = syntaxTree.TextBuffer as SnapshotTextBuffer;
            return textBuffer == null ? null : textBuffer.Snapshot;
        }
    }
}
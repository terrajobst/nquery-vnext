using System;

using ActiproSoftware.Text;

using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Text
{
    // I've considered implementing this text buffer directly against
    // the ITextSnapshot instead of parsing the text.
    //
    // The problem is tha that the ITextSnapshot doesn't counts line
    // breaks as zero.
    //
    // This results in a lot of funky behavior as this means that
    // NQuery's offset doesn't align with the offset that the Actipro's
    // editor would accept.

    internal sealed class SnapshotTextBuffer : TextBuffer
    {
        private readonly ITextSnapshot _snapshot;
        private readonly TextBuffer _textBuffer;

        public SnapshotTextBuffer(ITextSnapshot snapshot)
        {
            _snapshot = snapshot;
            _textBuffer = From(snapshot.Text);
        }

        public ITextSnapshot Snapshot
        {
            get { return _snapshot; }
        }

        public override int GetLineNumberFromPosition(int position)
        {
            return _textBuffer.GetLineNumberFromPosition(position);
        }

        public override string GetText(TextSpan textSpan)
        {
            return _textBuffer.GetText(textSpan);
        }

        public override char this[int index]
        {
            get { return _textBuffer[index]; }
        }

        public override int Length
        {
            get { return _textBuffer.Length; }
        }

        public override TextLineCollection Lines
        {
            get { return _textBuffer.Lines; }
        }
    }
}
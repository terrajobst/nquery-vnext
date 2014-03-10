using System;

using Microsoft.VisualStudio.Text;

using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Text
{
    internal sealed class SnapshotTextBuffer : TextBuffer
    {
        private readonly ITextSnapshot _snapshot;
        private readonly SnapshotTextLineCollection _lines;

        public SnapshotTextBuffer(ITextSnapshot snapshot)
        {
            _snapshot = snapshot;
            _lines = new SnapshotTextLineCollection(this, snapshot);
        }

        public override int GetLineNumberFromPosition(int position)
        {
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException("position");

            return _snapshot.GetLineNumberFromPosition(position);
        }

        public override string GetText(TextSpan textSpan)
        {
            return _snapshot.GetText(textSpan.Start, textSpan.Length);
        }

        public ITextSnapshot Snapshot
        {
            get { return _snapshot; }
        }

        public override char this[int index]
        {
            get { return _snapshot[index]; }
        }

        public override int Length
        {
            get { return _snapshot.Length; }
        }

        public override TextLineCollection Lines
        {
            get { return _lines; }
        }
    }
}
using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Text;

using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Text
{
    internal sealed class SnapshotTextLineCollection : TextLineCollection
    {
        private readonly ITextSnapshot _snapshot;
        private readonly TextBuffer _textBuffer;

        public SnapshotTextLineCollection(TextBuffer textBuffer, ITextSnapshot snapshot)
        {
            _textBuffer = textBuffer;
            _snapshot = snapshot;
        }

        public override IEnumerator<TextLine> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        public override int Count
        {
            get { return _snapshot.LineCount; }
        }

        public override TextLine this[int index]
        {
            get
            {
                var line = _snapshot.GetLineFromLineNumber(index);
                return new TextLine(_textBuffer, line.Start, line.Length);
            }
        }
    }
}
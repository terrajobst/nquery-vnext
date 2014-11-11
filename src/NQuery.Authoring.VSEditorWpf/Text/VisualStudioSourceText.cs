using System;

using Microsoft.VisualStudio.Text;

using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Text
{
    internal sealed class VisualStudioSourceText : SourceText
    {
        private readonly ITextSnapshot _snapshot;
        private readonly VisualStudioTextLineCollection _lines;

        public VisualStudioSourceText(VisualStudioSourceTextContainer container, ITextSnapshot snapshot)
            : base(container)
        {
            _snapshot = snapshot;
            _lines = new VisualStudioTextLineCollection(this, snapshot);
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
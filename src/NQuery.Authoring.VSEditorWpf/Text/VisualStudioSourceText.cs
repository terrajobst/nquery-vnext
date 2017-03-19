using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Text
{
    internal sealed class VisualStudioSourceText : SourceText
    {
        private readonly VisualStudioTextLineCollection _lines;

        public VisualStudioSourceText(VisualStudioSourceTextContainer container, ITextSnapshot snapshot)
            : base(container)
        {
            Snapshot = snapshot;
            _lines = new VisualStudioTextLineCollection(this, snapshot);
        }

        public override int GetLineNumberFromPosition(int position)
        {
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            return Snapshot.GetLineNumberFromPosition(position);
        }

        public override string GetText(TextSpan textSpan)
        {
            return Snapshot.GetText(textSpan.Start, textSpan.Length);
        }

        public ITextSnapshot Snapshot { get; }

        public override char this[int index]
        {
            get { return Snapshot[index]; }
        }

        public override int Length
        {
            get { return Snapshot.Length; }
        }

        public override TextLineCollection Lines
        {
            get { return _lines; }
        }

        public override IEnumerable<TextChange> GetChanges(SourceText oldText)
        {
            if (oldText is VisualStudioSourceText vsOldText)
            {
                var oldSnapshot = vsOldText.Snapshot;
                var newSnapshot = Snapshot;

                var current = oldSnapshot.Version;
                var changes = new List<TextChange>();

                while (current != null)
                {
                    if (current == newSnapshot.Version)
                        return changes;

                    if (current.Changes == null)
                        break;

                    foreach (var vsChange in current.Changes)
                    {
                        var span = new TextSpan(vsChange.OldPosition, vsChange.OldLength);
                        var change = new TextChange(span, vsChange.NewText);
                        changes.Add(change);
                    }

                    current = current.Next;
                }
            }

            return base.GetChanges(oldText);
        }
    }
}
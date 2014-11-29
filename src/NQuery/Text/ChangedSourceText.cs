using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Text
{
    internal sealed class ChangedSourceText : SourceText
    {
        private readonly SourceText _oldText;
        private readonly SourceText _newText;
        private readonly ImmutableArray<TextChange> _changes;

        public ChangedSourceText(SourceText oldText, SourceText newText, IEnumerable<TextChange> changes)
            : base(newText.Container)
        {
            _oldText = oldText;
            _newText = newText;
            _changes = changes.ToImmutableArray();
        }

        public SourceText OldText
        {
            get { return _oldText; }
        }

        public SourceText NewText
        {
            get { return _newText; }
        }

        public ImmutableArray<TextChange> Changes
        {
            get { return _changes; }
        }

        public override int GetLineNumberFromPosition(int position)
        {
            return _newText.GetLineNumberFromPosition(position);
        }

        public override string GetText(TextSpan textSpan)
        {
            return _newText.GetText(textSpan);
        }

        public override char this[int index]
        {
            get { return _newText[index]; }
        }

        public override int Length
        {
            get { return _newText.Length; }
        }

        public override TextLineCollection Lines
        {
            get { return _newText.Lines; }
        }
    }
}
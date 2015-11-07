using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Text
{
    internal sealed class ChangedSourceText : SourceText
    {
        public ChangedSourceText(SourceText oldText, SourceText newText, IEnumerable<TextChange> changes)
            : base(newText.Container)
        {
            OldText = oldText;
            NewText = newText;
            Changes = changes.ToImmutableArray();
        }

        public SourceText OldText { get; }

        public SourceText NewText { get; }

        public ImmutableArray<TextChange> Changes { get; }

        public override int GetLineNumberFromPosition(int position)
        {
            return NewText.GetLineNumberFromPosition(position);
        }

        public override string GetText(TextSpan textSpan)
        {
            return NewText.GetText(textSpan);
        }

        public override char this[int index]
        {
            get { return NewText[index]; }
        }

        public override int Length
        {
            get { return NewText.Length; }
        }

        public override TextLineCollection Lines
        {
            get { return NewText.Lines; }
        }
    }
}
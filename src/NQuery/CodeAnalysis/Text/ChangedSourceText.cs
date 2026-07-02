using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Text;

internal sealed class ChangedSourceText : SourceText
{
    public ChangedSourceText(SourceText oldText, SourceText newText, IEnumerable<TextChange> changes)
        : base(newText.Container)
    {
        ThrowIfNull(oldText);
        ThrowIfNull(newText);
        ThrowIfNull(changes);

        OldText = oldText;
        NewText = newText;
        Changes = [.. changes];
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

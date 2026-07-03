using Microsoft.VisualStudio.Text;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.VSEditorWpf.Text;

internal sealed class VisualStudioSourceText : SourceText
{
    private readonly VisualStudioTextLineCollection _lines;

    public VisualStudioSourceText(VisualStudioSourceTextContainer container, ITextSnapshot snapshot)
        : base(container)
    {
        ThrowIfNull(container);
        ThrowIfNull(snapshot);

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
}

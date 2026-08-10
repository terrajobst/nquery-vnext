using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring;

// A document plus where the user is in it. Services whose subject is a caret or a selection take
// one of these; services whose subject is the whole document take a Document.
//
// A service that reads Position and ignores Selection isn't being handed more than it needs -- an
// empty selection is the caret, which is what the position-only factory produces.
public sealed class DocumentView
{
    private DocumentView(Document document, int position, TextSpan selection)
    {
        Document = document;
        Position = position;
        Selection = selection;
    }

    public static DocumentView Create(Document document, int position)
    {
        ThrowIfNull(document);

        return Create(document, position, new TextSpan(position, 0));
    }

    public static DocumentView Create(Document document, int position, TextSpan selection)
    {
        ThrowIfNull(document);

        if (position < 0 || position > document.Text.Length)
            throw new ArgumentOutOfRangeException(nameof(position));

        if (selection.Start < 0 || selection.Start > document.Text.Length)
            throw new ArgumentOutOfRangeException(nameof(selection));

        if (selection.End < 0 || selection.End > document.Text.Length)
            throw new ArgumentOutOfRangeException(nameof(selection));

        return new DocumentView(document, position, selection);
    }

    public Document Document { get; }

    public SourceText Text
    {
        get { return Document.Text; }
    }

    public int Position { get; }

    public TextSpan Selection { get; }
}

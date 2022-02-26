using NQuery.Text;

namespace NQuery.Authoring
{
    public sealed class DocumentView
    {
        public DocumentView(Document document, int position)
            : this(document, position, new TextSpan(position, 0))
        {
        }

        public DocumentView(Document document, int position, TextSpan selection)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (position < 0 || position > document.Text.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (selection.Start < 0 || selection.Start > document.Text.Length)
                throw new ArgumentOutOfRangeException(nameof(selection));

            if (selection.End < 0 || selection.End > document.Text.Length)
                throw new ArgumentOutOfRangeException(nameof(selection));

            Document = document;
            Position = position;
            Selection = selection;
        }

        public Document Document { get; }

        public SourceText Text
        {
            get { return Document.Text; }
        }

        public int Position { get; }

        public TextSpan Selection { get; }
    }
}
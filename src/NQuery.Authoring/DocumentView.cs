using System;

using NQuery.Text;

namespace NQuery.Authoring
{
    public sealed class DocumentView
    {
        private readonly Document _document;
        private readonly int _position;
        private readonly TextSpan _selection;

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

            _document = document;
            _position = position;
            _selection = selection;
        }

        public Document Document
        {
            get { return _document; }
        }

        public SourceText Text
        {
            get { return _document.Text; }
        }

        public int Position
        {
            get { return _position; }
        }

        public TextSpan Selection
        {
            get { return _selection; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace NQuery.Language
{
    public struct SyntaxTrivia
    {
        private readonly SyntaxKind _kind;
        private readonly string _text;
        private readonly TextSpan _span;
        private readonly StructuredTriviaSyntax _structure;
        private readonly ReadOnlyCollection<Diagnostic> _diagnostics;

        public SyntaxTrivia(SyntaxKind kind, string text, TextSpan span, StructuredTriviaSyntax structure, IList<Diagnostic> diagnostics)
        {
            _kind = kind;
            _text = text;
            _span = span;
            _structure = structure;
            _diagnostics = new ReadOnlyCollection<Diagnostic>(diagnostics);
        }

        public SyntaxKind Kind
        {
            get { return _kind; }
        }

        public string Text
        {
            get { return _text; }
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public StructuredTriviaSyntax Structure
        {
            get { return _structure; }
        }

        public ReadOnlyCollection<Diagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }

        public void WriteTo(TextWriter writer)
        {
            writer.Write(_text);
        }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                WriteTo(writer);
                return writer.ToString();
            }
        }
    }
}
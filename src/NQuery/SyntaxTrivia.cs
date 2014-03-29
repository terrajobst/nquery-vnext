using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery
{
    public sealed class SyntaxTrivia
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly SyntaxKind _kind;
        private readonly string _text;
        private readonly TextSpan _span;
        private readonly StructuredTriviaSyntax _structure;
        private readonly ImmutableArray<Diagnostic> _diagnostics;

        internal SyntaxTrivia(SyntaxTree syntaxTree, SyntaxKind kind, string text, TextSpan span, StructuredTriviaSyntax structure, IEnumerable<Diagnostic> diagnostics)
        {
            _syntaxTree = syntaxTree;
            _kind = kind;
            _text = text;
            _span = span;
            _structure = structure;
            _diagnostics = diagnostics.ToImmutableArray();
        }

        public SyntaxToken Parent
        {
            get { return _syntaxTree == null ? null : _syntaxTree.GetParentToken(this); }
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

        public ImmutableArray<Diagnostic> Diagnostics
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
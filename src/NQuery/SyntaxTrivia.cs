using System.Collections.Immutable;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery
{
    public sealed class SyntaxTrivia
    {
        private readonly SyntaxTree _syntaxTree;

        internal SyntaxTrivia(SyntaxTree syntaxTree, SyntaxKind kind, string text, TextSpan span, StructuredTriviaSyntax structure, IEnumerable<Diagnostic> diagnostics)
        {
            _syntaxTree = syntaxTree;
            Kind = kind;
            Text = text;
            Span = span;
            Structure = structure;
            Diagnostics = diagnostics.ToImmutableArray();
        }

        public SyntaxToken Parent => _syntaxTree?.GetParentToken(this);

        public SyntaxKind Kind { get; }

        public string Text { get; }

        public TextSpan Span { get; }

        public StructuredTriviaSyntax Structure { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public void WriteTo(TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);

            writer.Write(Text);
        }

        public override string ToString()
        {
            using var writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }
    }
}
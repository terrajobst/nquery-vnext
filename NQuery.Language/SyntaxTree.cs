using System;

namespace NQuery.Language
{
    public sealed class SyntaxTree
    {
        private readonly CompilationUnitSyntax _root;
        private readonly TextBuffer _textBuffer;

        private SyntaxTree(CompilationUnitSyntax root, TextBuffer textBuffer)
        {
            _root = root;
            _textBuffer = textBuffer;
        }

        public static SyntaxTree ParseQuery(string source)
        {
            var parser = new Parser(source);
            var query = parser.ParseRootQuery();
            var textBuffer = new TextBuffer(source);
            return new SyntaxTree(query, textBuffer);
        }

        public static SyntaxTree ParseExpression(string source)
        {
            var parser = new Parser(source);
            var expression = parser.ParseRootQuery();
            var textBuffer = new TextBuffer(source);
            return new SyntaxTree(expression, textBuffer);
        }

        public CompilationUnitSyntax Root
        {
            get { return _root; }
        }

        public TextBuffer TextBuffer
        {
            get { return _textBuffer; }
        }
    }
}
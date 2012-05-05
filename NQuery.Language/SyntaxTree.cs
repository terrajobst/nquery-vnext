using System;

namespace NQuery.Language
{
    public sealed class SyntaxTree
    {
        private readonly SyntaxNode _root;
        private readonly TextBuffer _textBuffer;

        private SyntaxTree(SyntaxNode root, TextBuffer textBuffer)
        {
            _root = root;
            _textBuffer = textBuffer;
        }

        public static SyntaxTree ParseQuery(string source)
        {
            var parser = new Parser(source);
            var query = parser.ParseQueryWithOptionalCte();
            var textBuffer = new TextBuffer(source);
            return new SyntaxTree(query, textBuffer);
        }

        public static SyntaxTree ParseExpression(string source)
        {
            var parser = new Parser(source);
            var expression = parser.ParseExpression();
            var textBuffer = new TextBuffer(source);
            return new SyntaxTree(expression, textBuffer);
        }

        public SyntaxNode Root
        {
            get { return _root; }
        }

        public TextBuffer TextBuffer
        {
            get { return _textBuffer; }
        }
    }
}
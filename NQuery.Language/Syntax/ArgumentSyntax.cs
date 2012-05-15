using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ArgumentSyntax : SyntaxNode
    {
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken? _comma;

        public ArgumentSyntax(ExpressionSyntax expression, SyntaxToken? comma)
        {
            _expression = expression;
            _comma = comma;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.Argument; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _expression;

            if (_comma != null)
                yield return _comma.Value;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken? Comma
        {
            get { return _comma; }
        }
    }
}
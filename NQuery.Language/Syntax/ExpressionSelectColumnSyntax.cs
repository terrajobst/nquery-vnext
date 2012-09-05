using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ExpressionSelectColumnSyntax : SelectColumnSyntax
    {
        private readonly ExpressionSyntax _expression;
        private readonly AliasSyntax _alias;

        public ExpressionSelectColumnSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, AliasSyntax alias, SyntaxToken? commaToken)
            : base(syntaxTree, commaToken)
        {
            _expression = expression;
            _alias = alias;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExpressionSelectColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _expression;
            if (_alias != null)
                yield return _alias;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public AliasSyntax Alias
        {
            get { return _alias; }
        }
    }
}
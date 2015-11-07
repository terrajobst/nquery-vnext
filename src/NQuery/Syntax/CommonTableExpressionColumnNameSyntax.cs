using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class CommonTableExpressionColumnNameSyntax : SyntaxNode
    {
        private readonly SyntaxToken _identifier;

        internal CommonTableExpressionColumnNameSyntax(SyntaxTree syntaxTree, SyntaxToken identifier)
            : base(syntaxTree)
        {
            _identifier = identifier;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionColumnName; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _identifier;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }
    }
}
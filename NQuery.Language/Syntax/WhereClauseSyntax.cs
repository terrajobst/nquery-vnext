using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class WhereClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _whereKeyword;
        private readonly ExpressionSyntax _predicate;

        public WhereClauseSyntax(SyntaxToken whereKeyword, ExpressionSyntax predicate)
        {
            _whereKeyword = whereKeyword;
            _predicate = predicate;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.WhereClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _whereKeyword;
            yield return _predicate;
        }

        public SyntaxToken WhereKeyword
        {
            get { return _whereKeyword; }
        }

        public ExpressionSyntax Predicate
        {
            get { return _predicate; }
        }
    }
}
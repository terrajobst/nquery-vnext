using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class HavingClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _havingKeyword;
        private readonly ExpressionSyntax _predicate;

        public HavingClauseSyntax(SyntaxTree syntaxTree, SyntaxToken havingKeyword, ExpressionSyntax predicate)
            : base(syntaxTree)
        {
            _havingKeyword = havingKeyword.WithParent(this);
            _predicate = predicate;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.HavingClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _havingKeyword;
            yield return _predicate;
        }

        public SyntaxToken HavingKeyword
        {
            get { return _havingKeyword; }
        }

        public ExpressionSyntax Predicate
        {
            get { return _predicate; }
        }
    }
}
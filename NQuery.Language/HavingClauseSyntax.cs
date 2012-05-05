using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class HavingClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _havingKeyword;
        private readonly ExpressionSyntax _predicate;

        public HavingClauseSyntax(SyntaxToken havingKeyword, ExpressionSyntax predicate)
        {
            _havingKeyword = havingKeyword;
            _predicate = predicate;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.HavingClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
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
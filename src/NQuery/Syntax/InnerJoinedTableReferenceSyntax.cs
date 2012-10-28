using System;
using System.Collections.Generic;

namespace NQuery
{
    public sealed class InnerJoinedTableReferenceSyntax : ConditionedJoinedTableReferenceSyntax
    {
        private readonly SyntaxToken _innerKeyword;
        private readonly SyntaxToken _joinKeyword;

        public InnerJoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, SyntaxToken innerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition)
            : base(syntaxTree, left, right, onKeyword, 2, condition)
        {
            _innerKeyword = innerKeyword;
            _joinKeyword = joinKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.InnerJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            if (_innerKeyword != null)
                yield return _innerKeyword;
            yield return _joinKeyword;
            yield return Right;
            yield return OnKeyword;
            yield return Condition;
        }

        public SyntaxToken InnerKeyword
        {
            get { return _innerKeyword; }
        }

        public SyntaxToken JoinKeyword
        {
            get { return _joinKeyword; }
        }
    }
}
using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class InnerJoinedTableReferenceSyntax : ConditionedJoinedTableReferenceSyntax
    {
        private readonly SyntaxToken? _innerKeyword;
        private readonly SyntaxToken _joinKeyword;
        private readonly SyntaxToken _onKeyword;

        public InnerJoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, SyntaxToken? innerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition)
            : base(syntaxTree, left, right, condition)
        {
            _innerKeyword = innerKeyword.WithParent(this);
            _joinKeyword = joinKeyword.WithParent(this);
            _onKeyword = onKeyword.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.InnerJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            if (_innerKeyword != null)
                yield return _innerKeyword.Value;
            yield return _joinKeyword;
            yield return Right;
            yield return _onKeyword;
            yield return Condition;
        }

        public SyntaxToken? InnerKeyword
        {
            get { return _innerKeyword; }
        }

        public SyntaxToken JoinKeyword
        {
            get { return _joinKeyword; }
        }

        public SyntaxToken OnKeyword
        {
            get { return _onKeyword; }
        }
    }
}
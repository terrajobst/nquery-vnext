using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class InnerJoinedTableReferenceSyntax : ConditionedJoinedTableReferenceSyntax
    {
        private readonly SyntaxToken? _innerKeyword;
        private readonly SyntaxToken _joinKeyword;
        private readonly SyntaxToken _onKeyword;

        public InnerJoinedTableReferenceSyntax(TableReferenceSyntax left, SyntaxToken? innerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition, SyntaxToken? commaToken)
            : base(left, right, condition, commaToken)
        {
            _innerKeyword = innerKeyword;
            _joinKeyword = joinKeyword;
            _onKeyword = onKeyword;
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
            if (CommaToken != null)
                yield return CommaToken.Value;
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
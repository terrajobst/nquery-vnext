using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class InnerJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        private readonly SyntaxToken? _innerKeyword;
        private readonly SyntaxToken _joinKeyword;
        private readonly SyntaxToken _onKeyword;
        private readonly ExpressionSyntax _condition;

        public InnerJoinedTableReferenceSyntax(TableReferenceSyntax left, SyntaxToken? innerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition, SyntaxToken? commaToken)
            : base(left, right, commaToken)
        {
            _innerKeyword = innerKeyword;
            _joinKeyword = joinKeyword;
            _onKeyword = onKeyword;
            _condition = condition;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.InnerJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return Left;
            if (_innerKeyword != null)
                yield return _innerKeyword.Value;
            yield return _joinKeyword;
            yield return Right;
            yield return _onKeyword;
            yield return _condition;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }
    }
}
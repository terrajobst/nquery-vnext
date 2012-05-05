using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class OuterJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        private readonly SyntaxToken _typeKeyword;
        private readonly SyntaxToken? _outerKeyword;
        private readonly SyntaxToken _joinKeyword;
        private readonly SyntaxToken _onKeyword;
        private readonly ExpressionSyntax _condition;

        public OuterJoinedTableReferenceSyntax(TableReferenceSyntax left, SyntaxToken typeKeyword, SyntaxToken? outerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition, SyntaxToken? commaToken)
            : base(left, right, commaToken)
        {
            _typeKeyword = typeKeyword;
            _outerKeyword = outerKeyword;
            _joinKeyword = joinKeyword;
            _onKeyword = onKeyword;
            _condition = condition;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OuterJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return Left;
            yield return _typeKeyword;
            if (_outerKeyword != null)
                yield return _outerKeyword.Value;
            yield return _joinKeyword;
            yield return Right;
            yield return _onKeyword;
            yield return _condition;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }
    }
}
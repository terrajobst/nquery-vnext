using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class OuterJoinedTableReferenceSyntax : ConditionedJoinedTableReferenceSyntax
    {
        private readonly SyntaxToken _typeKeyword;
        private readonly SyntaxToken? _outerKeyword;
        private readonly SyntaxToken _joinKeyword;
        private readonly SyntaxToken _onKeyword;

        public OuterJoinedTableReferenceSyntax(TableReferenceSyntax left, SyntaxToken typeKeyword, SyntaxToken? outerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition, SyntaxToken? commaToken)
            : base(left, right, condition, commaToken)
        {
            _typeKeyword = typeKeyword;
            _outerKeyword = outerKeyword;
            _joinKeyword = joinKeyword;
            _onKeyword = onKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OuterJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            yield return _typeKeyword;
            if (_outerKeyword != null)
                yield return _outerKeyword.Value;
            yield return _joinKeyword;
            yield return Right;
            yield return _onKeyword;
            yield return Condition;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }

        public SyntaxToken TypeKeyword
        {
            get { return _typeKeyword; }
        }

        public SyntaxToken? OuterKeyword
        {
            get { return _outerKeyword; }
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
using System;
using System.Collections.Generic;

namespace NQuery
{
    public sealed class OuterJoinedTableReferenceSyntax : ConditionedJoinedTableReferenceSyntax
    {
        private readonly SyntaxToken _typeKeyword;
        private readonly SyntaxToken _outerKeyword;
        private readonly SyntaxToken _joinKeyword;

        public OuterJoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, SyntaxToken typeKeyword, SyntaxToken outerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition)
            : base(syntaxTree, left, right, onKeyword, 3, condition)
        {
            _typeKeyword = typeKeyword;
            _outerKeyword = outerKeyword;
            _joinKeyword = joinKeyword;
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
                yield return _outerKeyword;
            yield return _joinKeyword;
            yield return Right;
            yield return OnKeyword;
            yield return Condition;
        }

        public SyntaxToken TypeKeyword
        {
            get { return _typeKeyword; }
        }

        public SyntaxToken OuterKeyword
        {
            get { return _outerKeyword; }
        }

        public SyntaxToken JoinKeyword
        {
            get { return _joinKeyword; }
        }
    }
}
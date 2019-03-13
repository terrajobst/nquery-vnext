#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class InnerJoinedTableReferenceSyntax : ConditionedJoinedTableReferenceSyntax
    {
        internal InnerJoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, SyntaxToken? innerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition)
            : base(syntaxTree, left, right, onKeyword, condition)
        {
            InnerKeyword = innerKeyword;
            JoinKeyword = joinKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.InnerJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            if (InnerKeyword != null)
                yield return InnerKeyword;
            yield return JoinKeyword;
            yield return Right;
            yield return OnKeyword;
            yield return Condition;
        }

        public SyntaxToken? InnerKeyword { get; }

        public SyntaxToken JoinKeyword { get; }
    }
}
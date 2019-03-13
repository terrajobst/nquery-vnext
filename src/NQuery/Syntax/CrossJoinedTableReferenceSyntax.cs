#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class CrossJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        internal CrossJoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, SyntaxToken crossKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right)
            : base(syntaxTree, left, right)
        {
            CrossKeyword = crossKeyword;
            JoinKeyword = joinKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CrossJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            yield return CrossKeyword;
            yield return JoinKeyword;
            yield return Right;
        }

        public SyntaxToken CrossKeyword { get; }

        public SyntaxToken JoinKeyword { get; }
    }
}
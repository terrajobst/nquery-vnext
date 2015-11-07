using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class ParenthesizedTableReferenceSyntax : TableReferenceSyntax
    {
        internal ParenthesizedTableReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, TableReferenceSyntax tableReference, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            LeftParenthesis = leftParenthesis;
            TableReference = tableReference;
            RightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return LeftParenthesis;
            yield return TableReference;
            yield return RightParenthesis;
        }

        public SyntaxToken LeftParenthesis { get; }

        public TableReferenceSyntax TableReference { get; }

        public SyntaxToken RightParenthesis { get; }
    }
}
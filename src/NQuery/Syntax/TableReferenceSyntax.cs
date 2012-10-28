using System;

namespace NQuery
{
    public abstract class TableReferenceSyntax : SyntaxNode
    {
        protected TableReferenceSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
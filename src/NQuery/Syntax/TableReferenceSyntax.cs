using System;

namespace NQuery.Syntax
{
    public abstract class TableReferenceSyntax : SyntaxNode
    {
        protected TableReferenceSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
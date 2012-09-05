using System;

namespace NQuery.Language
{
    public abstract class StructuredTriviaSyntax : SyntaxNode
    {
        protected StructuredTriviaSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
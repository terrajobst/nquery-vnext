using System;

namespace NQuery.Language
{
    public abstract class SubselectExpressionSyntax : ExpressionSyntax
    {
        protected SubselectExpressionSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
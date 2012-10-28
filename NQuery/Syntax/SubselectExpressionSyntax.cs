using System;

namespace NQuery
{
    public abstract class SubselectExpressionSyntax : ExpressionSyntax
    {
        protected SubselectExpressionSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
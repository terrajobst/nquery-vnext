using System;

namespace NQuery.Syntax
{
    public abstract class SubselectExpressionSyntax : ExpressionSyntax
    {
        protected SubselectExpressionSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
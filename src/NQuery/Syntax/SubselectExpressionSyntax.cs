#nullable enable

using System;

namespace NQuery.Syntax
{
    public abstract class SubselectExpressionSyntax : ExpressionSyntax
    {
        internal SubselectExpressionSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
#nullable enable

using System;

namespace NQuery.Syntax
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        internal ExpressionSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
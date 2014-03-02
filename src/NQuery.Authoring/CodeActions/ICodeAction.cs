using System;

namespace NQuery.Authoring.CodeActions
{
    public interface ICodeAction
    {
        string Description { get; }
        SyntaxTree GetEdit();
    }
}
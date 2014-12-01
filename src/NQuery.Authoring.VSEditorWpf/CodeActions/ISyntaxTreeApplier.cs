using System;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    internal interface ISyntaxTreeApplier
    {
        void Apply(SyntaxTree syntaxTree, string description);
    }
}
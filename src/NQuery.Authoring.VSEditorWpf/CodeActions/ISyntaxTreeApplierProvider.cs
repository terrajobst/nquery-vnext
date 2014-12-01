using System;

using Microsoft.VisualStudio.Text;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    internal interface ISyntaxTreeApplierProvider
    {
        ISyntaxTreeApplier GetSyntaxTreeApplier(ITextBuffer textBuffer);
    }
}
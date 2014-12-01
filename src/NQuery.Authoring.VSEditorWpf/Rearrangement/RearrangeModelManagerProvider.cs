using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.Composition.Rearrangement;
using NQuery.Authoring.VSEditorWpf.CodeActions;

namespace NQuery.Authoring.VSEditorWpf.Rearrangement
{
    [Export(typeof(IRearrangeModelManagerProvider))]
    internal sealed class RearrangeModelManagerProvider : IRearrangeModelManagerProvider
    {
        [Import]
        public IRearrangersService RearrangersService { get; set; }

        [Import]
        public ISyntaxTreeApplierProvider SyntaxTreeApplierProvider { get; set; }

        public IRearrangeModelManager GetRearrangeModelManager(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var workspace = textView.TextBuffer.GetWorkspace();
                var rearrangers = RearrangersService.Rearrangers;
                var syntaxTreeApplier = SyntaxTreeApplierProvider.GetSyntaxTreeApplier(textView.TextBuffer);
                return new RearrangeModelManager(workspace, textView, syntaxTreeApplier, rearrangers);
            });
        }
    }
}
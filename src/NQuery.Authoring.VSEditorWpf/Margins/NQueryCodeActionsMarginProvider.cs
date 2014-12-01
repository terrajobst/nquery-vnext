using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.Composition.CodeActions;
using NQuery.Authoring.VSEditorWpf.CodeActions;

namespace NQuery.Authoring.VSEditorWpf.Margins
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(@"NQueryCodeActionsMarginProvider")]
    [MarginContainer(PredefinedMarginNames.Left)]
    [Order(Before = PredefinedMarginNames.LeftSelection)]
    [ContentType(@"NQuery")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class NQueryCodeActionsMarginProvider : IWpfTextViewMarginProvider
    {
        [Import]
        public ICodeFixProviderService CodeFixProviderService { get; set; }

        [Import]
        public ICodeIssueProviderService CodeIssueProviderService { get; set; }

        [Import]
        public ICodeRefactoringProviderService CodeRefactoringProviderService { get; set; }

        [Import]
        public ISyntaxTreeApplierProvider SyntaxTreeApplierProvider { get; set; }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            var textBuffer = wpfTextViewHost.TextView.TextBuffer;
            var syntaxTreeApplier = SyntaxTreeApplierProvider.GetSyntaxTreeApplier(textBuffer);
            var workspace = textBuffer.GetWorkspace();
            var fixProviders = CodeFixProviderService.Providers;
            var issueProviders = CodeIssueProviderService.Providers;
            var refactoringProviders = CodeRefactoringProviderService.Providers;
            var margin = new NQueryCodeActionsMargin(workspace, wpfTextViewHost, syntaxTreeApplier, fixProviders, issueProviders, refactoringProviders);
            wpfTextViewHost.TextView.Properties.AddProperty(margin.GetType(), margin);
            return margin;
        }
    }
}
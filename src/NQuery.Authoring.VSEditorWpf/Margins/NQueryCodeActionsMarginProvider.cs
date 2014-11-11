using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.Composition.CodeActions;

namespace NQuery.Authoring.VSEditorWpf.Margins
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name("NQueryCodeActionsMarginProvider")]
    [MarginContainer(PredefinedMarginNames.Left)]
    [Order(Before = PredefinedMarginNames.LeftSelection)]
    [ContentType("NQuery")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class NQueryCodeActionsMarginProvider : IWpfTextViewMarginProvider
    {
        [Import]
        public ICodeIssueProviderService CodeIssueProviderService { get; set; }

        [Import]
        public ICodeRefactoringProviderService CodeRefactoringProviderService { get; set; }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            var workspace = wpfTextViewHost.TextView.TextBuffer.GetWorkspace();
            var issueProviders = CodeIssueProviderService.Providers;
            var refactoringProviders = CodeRefactoringProviderService.Providers;
            var margin = new NQueryCodeActionsMargin(workspace, wpfTextViewHost, issueProviders, refactoringProviders);
            wpfTextViewHost.TextView.Properties.AddProperty(margin.GetType(), margin);
            return margin;
        }
    }
}
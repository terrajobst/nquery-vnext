using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.Composition.CodeActions;

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
        public ITextBufferUndoManagerProvider TextBufferUndoManagerProvider { get; set; }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
        {
            var textBuffer = wpfTextViewHost.TextView.TextBuffer;
            var textBufferUndoManager = TextBufferUndoManagerProvider.GetTextBufferUndoManager(textBuffer);
            var workspace = textBuffer.GetWorkspace();
            var fixProviders = CodeFixProviderService.Providers;
            var issueProviders = CodeIssueProviderService.Providers;
            var refactoringProviders = CodeRefactoringProviderService.Providers;
            var margin = new NQueryCodeActionsMargin(workspace, wpfTextViewHost, textBufferUndoManager, fixProviders, issueProviders, refactoringProviders);
            wpfTextViewHost.TextView.Properties.AddProperty(margin.GetType(), margin);
            return margin;
        }
    }
}
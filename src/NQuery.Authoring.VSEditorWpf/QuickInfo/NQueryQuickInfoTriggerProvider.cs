using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Authoring.VSEditorWpf.QuickInfo
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [Name(@"NQueryQuickInfoTriggerProvider")]
    [ContentType(@"NQuery")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class NQueryQuickInfoTriggerProvider : IWpfTextViewCreationListener
    {
        [Import]
        public IQuickInfoManagerProvider QuickInfoManagerProvider { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            var quickInfoManager = QuickInfoManagerProvider.GetQuickInfoManager(textView);
            new NQueryQuickInfoTrigger(textView, quickInfoManager);
        }
    }
}
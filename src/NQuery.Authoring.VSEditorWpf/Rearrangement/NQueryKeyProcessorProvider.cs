using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Authoring.VSEditorWpf.Rearrangement
{
    [Export(typeof(IKeyProcessorProvider))]
    [Name("NQueryRearrangementKeyProcessor")]
    [ContentType("NQuery")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class NQueryRearrangementKeyProcessorProvider : IKeyProcessorProvider
    {
        [Import]
        public IRearrangeModelManagerProvider RearrangeModelManagerProvider { get; set; }


        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView)
        {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() =>
            {
                var rearrangeModelManager = RearrangeModelManagerProvider.GetRearrangeModelManager(wpfTextView);
                return new NQueryRearrangementKeyProcessor(rearrangeModelManager);
            });
        }
    }
}
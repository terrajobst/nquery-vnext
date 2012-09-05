using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(INQuerySelectionProviderService))]
    internal sealed class NQuerySelectionProviderService : INQuerySelectionProviderService
    {
        [Import]
        public INQuerySyntaxTreeManagerService SyntaxTreeManagerService { get; set; }

        public INQuerySelectionProvider GetSelectionProvider(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(() =>
                                                                        {
                                                                            var syntaxTreeManager = SyntaxTreeManagerService.GetSyntaxTreeManager(textView.TextBuffer);
                                                                            return new NQuerySelectionProvider(textView, syntaxTreeManager);
                                                                        });
        }
    }
}
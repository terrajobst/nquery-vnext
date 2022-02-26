using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace NQuery.Authoring.VSEditorWpf.SignatureHelp
{
    [Export(typeof(ISignatureHelpSourceProvider))]
    [ContentType(@"NQuery")]
    [Name(@"NQuerySignatureHelpSourceProvider")]
    internal class NQuerySignatureHelpSourceProvider : ISignatureHelpSourceProvider
    {
        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new NQuerySignatureHelpSource(textBuffer));
        }
    }
}
using System.ComponentModel.Composition;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

namespace NQueryViewerActiproWpf
{
    [Export(typeof(ILanguageServiceRegistrar))]
    internal sealed class CollapsedRegionQuickInfoProviderRegistrar : ILanguageServiceRegistrar
    {
        public void RegisterServices(SyntaxLanguage syntaxLanguage)
        {
            syntaxLanguage.RegisterService(new CollapsedRegionQuickInfoProvider());
        }
    }
}
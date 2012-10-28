using System;
using System.ComponentModel.Composition;

using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

namespace NQuery.Language.ActiproWpf.Outlining
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
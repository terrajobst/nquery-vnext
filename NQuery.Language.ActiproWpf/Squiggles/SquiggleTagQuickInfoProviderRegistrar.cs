using System;
using System.ComponentModel.Composition;

using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

namespace NQuery.Language.ActiproWpf.Squiggles
{
    // TODO: We may want to replace this provider with our own -- it doens't seem to work very well.
    [Export(typeof(ILanguageServiceRegistrar))]
    internal sealed class SquiggleTagQuickInfoProviderRegistrar : ILanguageServiceRegistrar
    {
        public void RegisterServices(SyntaxLanguage syntaxLanguage)
        {
            syntaxLanguage.RegisterService(new SquiggleTagQuickInfoProvider());
        }
    }
}
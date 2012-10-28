using System;
using System.ComponentModel.Composition;

using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Tagging.Implementation;

namespace NQuery.Language.ActiproWpf.BraceMatching
{
    [Export(typeof(ILanguageServiceRegistrar))]
    internal sealed class DelimiterHighlightTaggerRegistrar : ILanguageServiceRegistrar
    {
        public void RegisterServices(SyntaxLanguage syntaxLanguage)
        {
            syntaxLanguage.RegisterService(new TextViewTaggerProvider<DelimiterHighlightTagger>(typeof(DelimiterHighlightTagger)));
        }
    }
}
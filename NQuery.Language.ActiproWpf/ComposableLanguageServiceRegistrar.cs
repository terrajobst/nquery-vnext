using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using ActiproSoftware.Text.Implementation;

namespace NQueryViewerActiproWpf
{
    [Export(typeof(ILanguageServiceRegistrar))]
    internal sealed class ComposableLanguageServiceRegistrar : ILanguageServiceRegistrar
    {
        [ImportMany]
        public IEnumerable<Lazy<object, ILanguageServiceMetadata>> LanguageServices { get; set; }

        public void RegisterServices(SyntaxLanguage syntaxLanguage)
        {
            foreach (var languageService in LanguageServices)
            {
                var type = languageService.Metadata.ServiceType;
                syntaxLanguage.RegisterService(type, languageService.Value);
            }
        }
    }
}
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
                var serviceType = languageService.Metadata.ServiceType;
                var service = languageService.Value;
                syntaxLanguage.RegisterService(serviceType, service);
            }
        }
    }
}
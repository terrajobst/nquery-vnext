using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using ActiproSoftware.Text.Implementation;

namespace NQuery.Language.ActiproWpf
{
    [Export(typeof(ILanguageServiceRegistrar))]
    internal sealed class ComposableLanguageServiceRegistrar : ILanguageServiceRegistrar
    {
        [ImportMany]
        public IEnumerable<Lazy<object, IDictionary<string,object>>> LanguageServices { get; set; }

        public void RegisterServices(SyntaxLanguage syntaxLanguage)
        {
            const string serviceTypeKey = "ServiceType";
            var languageServices = from ls in LanguageServices
                                   where ls.Metadata.ContainsKey(serviceTypeKey)
                                   let service = ls.Value
                                   let serviceType = ls.Metadata[serviceTypeKey] ?? service.GetType()
                                   select new {Service = ls.Value, ServiceType = serviceType};

            foreach (var languageService in languageServices)
                syntaxLanguage.RegisterService(languageService.ServiceType, languageService.Service);
        }
    }
}
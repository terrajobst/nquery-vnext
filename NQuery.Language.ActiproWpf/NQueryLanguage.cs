using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using ActiproSoftware.Text.Implementation;

using NQuery.Language.Services.BraceMatching;

namespace NQuery.Language.ActiproWpf
{
    public sealed class NQueryLanguage : SyntaxLanguage, IDisposable
    {
        private readonly CompositionContainer _compositionContainer;

        public NQueryLanguage()
            : this(GetDefaultCatalog())
        {
        }

        public NQueryLanguage(ComposablePartCatalog catalog)
            : base("NQuery")
        {
            _compositionContainer = new CompositionContainer(catalog);
            _compositionContainer.SatisfyImportsOnce(this);

            var serviceProviders = _compositionContainer.GetExportedValues<ILanguageServiceRegistrar>();
            foreach (var serviceProvider in serviceProviders)
                serviceProvider.RegisterServices(this);
        }

        private static ComposablePartCatalog GetDefaultCatalog()
        {
            var servicesAssembly = new AssemblyCatalog(typeof (IBraceMatchingService).Assembly);
            var thisAssembly = new AssemblyCatalog(typeof (NQueryLanguage).Assembly);
            return new AggregateCatalog(servicesAssembly, thisAssembly);
        }

        public void Dispose()
        {
            _compositionContainer.Dispose();
        }
    }
}
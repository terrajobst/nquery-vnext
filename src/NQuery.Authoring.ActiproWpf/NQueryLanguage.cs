using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using ActiproSoftware.Text.Implementation;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.Wpf;

namespace NQuery.Authoring.ActiproWpf
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
            var wpfAssembly = new AssemblyCatalog(typeof(INQueryGlyphService).Assembly);
            var thisAssembly = new AssemblyCatalog(typeof (NQueryLanguage).Assembly);
            return new AggregateCatalog(servicesAssembly, wpfAssembly, thisAssembly);
        }

        public void Dispose()
        {
            _compositionContainer.Dispose();
        }
    }
}
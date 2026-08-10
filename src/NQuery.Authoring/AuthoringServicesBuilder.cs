using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.Classifications;
using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Commenting;
using NQuery.Authoring.Completion;
using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Outlining;
using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.Selection;
using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SymbolSearch;

namespace NQuery.Authoring;

// Configures an AuthoringServices. Obtained from AuthoringServices.Create, never constructed
// directly, so there is no half-built instance to hand around.
//
// Providers keep insertion order per type, which first-wins features such as brace matching depend
// on; AddDefaultServices seeds the standard sets first so host providers append after the built-ins.
// Services are registered as factories and instantiated lazily against the completed bag, so the
// order of AddService and AddProvider calls doesn't matter.
public sealed class AuthoringServicesBuilder
{
    private readonly Dictionary<Type, ImmutableArray<object>.Builder> _providers = new();
    private readonly Dictionary<Type, Func<AuthoringServices, object>> _serviceFactories = new();

    internal AuthoringServicesBuilder()
    {
    }

    // The one place that knows the built-in feature set. Everything else here is feature-agnostic,
    // so a service living in another assembly is registered exactly the way these are.
    public AuthoringServicesBuilder AddDefaultServices()
    {
        this.AddBraceMatchingService();
        this.AddClassificationService();
        this.AddCodeFixService();
        this.AddCodeIssueService();
        this.AddCodeRefactoringService();
        this.AddCommentingService();
        this.AddCompletionService();
        this.AddHighlightingService();
        this.AddOutliningService();
        this.AddQuickInfoService();
        this.AddSelectionService();
        this.AddSignatureHelpService();
        this.AddSymbolSearchService();

        this.AddStandardBraceMatchers();
        this.AddStandardCodeFixProviders();
        this.AddStandardCodeIssueProviders();
        this.AddStandardCodeRefactoringProviders();
        this.AddStandardCompletionProviders();
        this.AddStandardHighlighters();
        this.AddStandardOutliners();
        this.AddStandardQuickInfoModelProviders();
        this.AddStandardSelectionSpanProviders();
        this.AddStandardSignatureHelpModelProviders();

        return this;
    }

    // Prefer the per-feature members (AddCompletionProvider and friends) over calling this with an
    // inferred type argument: builder.AddProvider(new MyCompletionProvider()) infers TProvider as
    // the concrete type and files the provider where no service will look for it.
    public AuthoringServicesBuilder AddProvider<TProvider>(TProvider provider)
        where TProvider : class
    {
        ThrowIfNull(provider);

        GetProviderBuilder<TProvider>().Add(provider);
        return this;
    }

    public AuthoringServicesBuilder AddProviders<TProvider>(IEnumerable<TProvider> providers)
        where TProvider : class
    {
        ThrowIfNull(providers);

        var builder = GetProviderBuilder<TProvider>();

        foreach (var provider in providers)
        {
            ThrowIfNull(provider);
            builder.Add(provider);
        }

        return this;
    }

    public AuthoringServicesBuilder RemoveProviders<TProvider>()
        where TProvider : class
    {
        _providers.Remove(typeof(TProvider));
        return this;
    }

    public AuthoringServicesBuilder AddService<TService>(Func<AuthoringServices, TService> factory)
        where TService : class
    {
        ThrowIfNull(factory);

        _serviceFactories[typeof(TService)] = services => factory(services);
        return this;
    }

    public AuthoringServicesBuilder RemoveService<TService>()
        where TService : class
    {
        _serviceFactories.Remove(typeof(TService));
        return this;
    }

    internal AuthoringServices Build()
    {
        var providers = _providers.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToImmutable());
        var serviceFactories = _serviceFactories.ToFrozenDictionary();
        return new AuthoringServices(providers, serviceFactories);
    }

    private ImmutableArray<object>.Builder GetProviderBuilder<TProvider>()
    {
        if (!_providers.TryGetValue(typeof(TProvider), out var builder))
        {
            builder = ImmutableArray.CreateBuilder<object>();
            _providers.Add(typeof(TProvider), builder);
        }

        return builder;
    }
}

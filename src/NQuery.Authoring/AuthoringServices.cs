using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace NQuery.Authoring;

// The composition root of the authoring layer: the language services a document is analyzed with,
// plus the providers those services are built from.
//
// Immutable and shared. One instance is built at startup and carried by every Document, which is
// why this is a value rather than a dependency injection container: documents are minted per
// keystroke, and a container would tie their lifetime to disposal and scoping that this layer has
// no use for. Everything here is a stateless singleton; all caching lives on Document.
//
// Keyed by type, so the root never enumerates features and a service defined in another assembly
// registers on equal footing. Only AuthoringServicesBuilder.AddDefaultServices knows the built-in
// set.
public sealed class AuthoringServices
{
    // Frozen rather than immutable: both are built once by the builder and then only read, which is
    // exactly what FrozenDictionary optimizes for. The service cache below stays concurrent -- it is
    // populated lazily as services are first resolved.
    private readonly FrozenDictionary<Type, ImmutableArray<object>> _providers;
    private readonly FrozenDictionary<Type, Func<AuthoringServices, object>> _serviceFactories;
    private readonly ConcurrentDictionary<Type, Lazy<object>> _services = new();

    internal AuthoringServices(FrozenDictionary<Type, ImmutableArray<object>> providers,
                               FrozenDictionary<Type, Func<AuthoringServices, object>> serviceFactories)
    {
        _providers = providers;
        _serviceFactories = serviceFactories;
    }

    public static AuthoringServices Create(Action<AuthoringServicesBuilder> configure)
    {
        ThrowIfNull(configure);

        var builder = new AuthoringServicesBuilder();
        configure(builder);
        return builder.Build();
    }

    // Throws rather than returning null: the service set is fixed at composition time, so a miss is
    // a bug in how the host was configured, not a condition a caller can meaningfully handle.
    public TService GetService<TService>()
        where TService : class
    {
        if (!TryGetService<TService>(out var service))
        {
            var message = $"There is no {typeof(TService).Name} registered. " +
                          $"Did you mean to call {nameof(AuthoringServicesBuilder.AddDefaultServices)}()?";
            throw new InvalidOperationException(message);
        }

        return service;
    }

    public bool TryGetService<TService>([NotNullWhen(true)] out TService? service)
        where TService : class
    {
        if (!_serviceFactories.TryGetValue(typeof(TService), out var factory))
        {
            service = null;
            return false;
        }

        // Lazy<T> rather than a bare GetOrAdd factory: GetOrAdd can run the factory more than once
        // under contention, and a service is meant to be a singleton even though it's stateless.
        var lazy = _services.GetOrAdd(typeof(TService), _ => new Lazy<object>(() => factory(this)));
        service = (TService)lazy.Value;
        return true;
    }

    // Public because a service defined outside this assembly has to be able to obtain its own
    // construction inputs. Note that services themselves never expose what they were built from:
    // whether a feature has providers behind it is not something its callers should be able to see.
    public ImmutableArray<TProvider> GetProviders<TProvider>()
        where TProvider : class
    {
        return _providers.TryGetValue(typeof(TProvider), out var providers)
                ? [.. providers.Cast<TProvider>()]
                : [];
    }
}

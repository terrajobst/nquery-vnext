using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NQuery.Authoring;

// The composition root of the authoring layer: the language services a document is analyzed with,
// including the extension points those services are built from.
//
// Immutable and shared. One instance is built at startup and carried by every Document, which is
// why this is a value rather than a dependency injection container: documents are minted per
// keystroke, and a container would tie their lifetime to disposal and scoping that this layer has
// no use for. Everything here is a stateless singleton; all caching lives on Document.
//
// There is no separate notion of a provider. A service type can simply be registered more than
// once: GetServices returns every registration in order, which is how a feature reaches its
// extension points, and GetService returns the single one. Registering several of something a
// caller expects one of is a composition bug rather than a silent last-one-wins, so GetService
// throws on it -- and because the whole graph is built eagerly, it throws from Create rather than
// from the keystroke that first touches the feature.
//
// Keyed by type, so the root never enumerates features and a service defined in another assembly
// registers on equal footing. Only AuthoringServicesBuilder.AddDefaultServices knows the built-in
// set.
public sealed class AuthoringServices
{
    private static readonly MethodInfo GetServicesMethod =
        typeof(AuthoringServices).GetMethod(nameof(GetServices), BindingFlags.Public | BindingFlags.Instance)!;

    // Frozen rather than immutable: built once and then only read, which is what FrozenDictionary
    // optimizes for.
    private readonly FrozenDictionary<Type, ImmutableArray<ServiceRegistration>> _registrations;

    // The chain of registrations currently being constructed, which is how a cycle is detected and
    // reported. Non-null only while Create is building the graph; afterwards every registration
    // holds an instance, so nothing ever re-enters resolution.
    private List<ServiceRegistration>? _resolving;

    internal AuthoringServices(ImmutableArray<ServiceRegistration> registrations)
    {
        _registrations = registrations.GroupBy(r => r.ServiceType)
                                      .ToFrozenDictionary(g => g.Key, g => g.ToImmutableArray());
    }

    public static AuthoringServices Create(Action<AuthoringServicesBuilder> configure)
    {
        ThrowIfNull(configure);

        var builder = new AuthoringServicesBuilder();
        configure(builder);
        return builder.Build();
    }

    // Everything is constructed up front, in registration order. Services here are stateless
    // singletons whose constructors do no work, so there is nothing worth deferring -- and building
    // eagerly is what turns a missing dependency, an ambiguous one, or a cycle into a failure at the
    // composition root instead of somewhere deep in an editor callback.
    internal void ResolveAll(ImmutableArray<ServiceRegistration> registrations)
    {
        _resolving = [];

        try
        {
            foreach (var registration in registrations)
                Resolve(registration);
        }
        finally
        {
            _resolving = null;
        }
    }

    // Throws rather than returning null: the service set is fixed at composition time, so a miss is
    // a bug in how the host was configured, not a condition a caller can meaningfully handle.
    public TService GetService<TService>()
        where TService : class
    {
        var registrations = GetRegistrations(typeof(TService));

        if (registrations.Length == 0)
        {
            var message = $"There is no {typeof(TService).Name} registered. " +
                          $"Did you mean to call {nameof(AuthoringServicesBuilder.AddDefaultServices)}()?";
            throw new InvalidOperationException(message);
        }

        ThrowIfAmbiguous<TService>(registrations);

        return (TService)Resolve(registrations[0]);
    }

    // Reports absence, not misuse: an ambiguous ask still throws, because "registered several times"
    // isn't an answer a caller asking for one of something can do anything sensible with.
    public bool TryGetService<TService>([NotNullWhen(true)] out TService? service)
        where TService : class
    {
        var registrations = GetRegistrations(typeof(TService));

        if (registrations.Length == 0)
        {
            service = null;
            return false;
        }

        ThrowIfAmbiguous<TService>(registrations);

        service = (TService)Resolve(registrations[0]);
        return true;
    }

    // In registration order, which features such as brace matching depend on: they take the first
    // result, and AddDefaultServices seeds the built-ins before a host appends its own.
    public ImmutableArray<TService> GetServices<TService>()
        where TService : class
    {
        var registrations = GetRegistrations(typeof(TService));
        var result = ImmutableArray.CreateBuilder<TService>(registrations.Length);

        foreach (var registration in registrations)
            result.Add((TService)Resolve(registration));

        return result.MoveToImmutable();
    }

    private ImmutableArray<ServiceRegistration> GetRegistrations(Type serviceType)
    {
        return _registrations.TryGetValue(serviceType, out var registrations)
                ? registrations
                : [];
    }

    private static void ThrowIfAmbiguous<TService>(ImmutableArray<ServiceRegistration> registrations)
    {
        if (registrations.Length < 2)
            return;

        var implementations = string.Join(@", ", registrations.Select(r => r.ImplementationType.Name));
        var message = $"There are {registrations.Length} registrations for {typeof(TService).Name} " +
                      $"({implementations}). Use {nameof(GetServices)}<{typeof(TService).Name}>() to get all of them.";
        throw new InvalidOperationException(message);
    }

    private object Resolve(ServiceRegistration registration)
    {
        if (registration.Instance is not null)
            return registration.Instance;

        // Only reachable while ResolveAll is running: once it returns, every registration has an
        // instance and the branch above takes over.
        var resolving = _resolving!;

        var start = resolving.IndexOf(registration);
        if (start >= 0)
        {
            var chain = string.Join(@" -> ", resolving.Skip(start).Append(registration));
            throw new InvalidOperationException($"The composition contains a cycle: {chain}.");
        }

        resolving.Add(registration);

        try
        {
            return registration.Resolve(this);
        }
        finally
        {
            resolving.RemoveAt(resolving.Count - 1);
        }
    }

    // Constructor injection. A parameter typed ImmutableArray<T> or IEnumerable<T> receives every
    // registration for T, anything else receives the single registration for its own type, and
    // AuthoringServices receives the composition itself -- which is the escape hatch for a service
    // that has to resolve something later rather than at construction.
    internal object Activate(ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        var arguments = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
            arguments[i] = ResolveParameter(constructor, parameters[i]);

        return constructor.Invoke(arguments);
    }

    private object ResolveParameter(ConstructorInfo constructor, ParameterInfo parameter)
    {
        var parameterType = parameter.ParameterType;

        if (parameterType == typeof(AuthoringServices))
            return this;

        if (TryGetElementType(parameterType, out var elementType))
            return GetServicesMethod.MakeGenericMethod(elementType).Invoke(this, null)!;

        var registrations = GetRegistrations(parameterType);
        var declaringType = constructor.DeclaringType!;

        if (registrations.Length == 0)
        {
            var message = $"{declaringType.Name} cannot be constructed: its parameter '{parameter.Name}' " +
                          $"needs a {parameterType.Name}, but none is registered.";
            throw new InvalidOperationException(message);
        }

        if (registrations.Length > 1)
        {
            var message = $"{declaringType.Name} cannot be constructed: its parameter '{parameter.Name}' " +
                          $"needs a single {parameterType.Name}, but {registrations.Length} are registered. " +
                          $"Declare it as ImmutableArray<{parameterType.Name}> to receive all of them.";
            throw new InvalidOperationException(message);
        }

        return Resolve(registrations[0]);
    }

    private static bool TryGetElementType(Type parameterType, [NotNullWhen(true)] out Type? elementType)
    {
        if (parameterType.IsGenericType)
        {
            var definition = parameterType.GetGenericTypeDefinition();
            if (definition == typeof(ImmutableArray<>) || definition == typeof(IEnumerable<>))
            {
                // A value-typed element can't be a service, so a parameter such as
                // IEnumerable<int> is an ordinary dependency rather than a set to fan out.
                var argument = parameterType.GetGenericArguments()[0];
                if (!argument.IsValueType)
                {
                    elementType = argument;
                    return true;
                }
            }
        }

        elementType = null;
        return false;
    }
}

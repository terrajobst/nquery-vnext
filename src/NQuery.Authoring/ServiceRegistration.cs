namespace NQuery.Authoring;

// A single entry in the composition. There is one of these per Add call, even when several name the
// same service type -- that's what makes a service registered more than once observable as a set
// rather than as a silent overwrite.
internal sealed class ServiceRegistration
{
    private readonly Func<AuthoringServices, object> _activator;

    public ServiceRegistration(Type serviceType, Type implementationType, Func<AuthoringServices, object> activator)
    {
        ServiceType = serviceType;
        ImplementationType = implementationType;
        _activator = activator;
    }

    public Type ServiceType { get; }

    // What the registration named: the implementation for AddService<TService, TImplementation>, the
    // concrete type for an instance, and the service type itself for a factory. Diagnostics only --
    // resolution never looks at it.
    public Type ImplementationType { get; }

    public object? Instance { get; private set; }

    public object Resolve(AuthoringServices services)
    {
        if (Instance is null)
        {
            var instance = _activator(services);
            if (instance is null)
            {
                var message = $"The factory registered for {ServiceType.Name} returned null.";
                throw new InvalidOperationException(message);
            }

            Instance = instance;
        }

        return Instance;
    }

    public override string ToString()
    {
        return ServiceType == ImplementationType
                ? ServiceType.Name
                : $"{ServiceType.Name} ({ImplementationType.Name})";
    }
}

namespace NQuery.Metadata;

public static class NullProviders
{
    private sealed class NullPropertyProvider : IPropertyProvider
    {
        public IEnumerable<PropertyDefinition> GetProperties(Type type)
        {
            ThrowIfNull(type);

            return Enumerable.Empty<PropertyDefinition>();
        }
    }

    private sealed class NullMethodProvider : IMethodProvider
    {
        public IEnumerable<MethodDefinition> GetMethods(Type type)
        {
            ThrowIfNull(type);

            return Enumerable.Empty<MethodDefinition>();
        }
    }

    public static IPropertyProvider PropertyProvider { get; } = new NullPropertyProvider();
    public static IMethodProvider MethodProvider { get; } = new NullMethodProvider();
}

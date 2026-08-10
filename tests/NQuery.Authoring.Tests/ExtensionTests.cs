using System.Collections.Immutable;

namespace NQuery.Authoring.Tests;

public abstract class ExtensionTests
{
    private static ImmutableArray<Type> GetAvailableProviderTypes<TProvider>()
    {
        var type = typeof(TProvider);
        return [.. type.Assembly.GetTypes().Where(t => !t.IsAbstract && type.IsAssignableFrom(t))];
    }

    // Asserts against what the default composition actually exposes rather than against the
    // standard array itself, so a provider that exists but is never registered still fails.
    protected static void AssertAllProvidersAreExposed<TProvider>()
        where TProvider : class
    {
        var availableTypes = GetAvailableProviderTypes<TProvider>();
        var exposed = DocumentFactory.DefaultServices.GetProviders<TProvider>();
        var exposedTypes = new HashSet<Type>(exposed.Select(p => p.GetType()));

        foreach (var type in availableTypes)
            Assert.True(exposedTypes.Contains(type), $"{type.Name} isn't exposed from the default AuthoringServices");

        Assert.Equal(exposedTypes.Count, availableTypes.Length);
    }
}

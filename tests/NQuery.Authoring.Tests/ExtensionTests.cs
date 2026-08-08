using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace NQuery.Authoring.Tests;

public abstract class ExtensionTests
{
    private static ImmutableArray<Type> GetAvailableProviderTypes<T>()
    {
        var type = typeof(T);
        return [.. type.Assembly.GetTypes().Where(t => !t.IsAbstract && type.IsAssignableFrom(t))];
    }

    protected static void AssertAllProvidersAreExposed<T>(IEnumerable<T> providers, [CallerArgumentExpression(nameof(providers))] string? source = null)
    {
        var availableTypes = GetAvailableProviderTypes<T>();
        var standardTypes = new HashSet<Type>(providers.Select(t => t!.GetType()));
        var message = $"{{0}} isn't exposed from {source}";

        foreach (var type in availableTypes)
            Assert.True(standardTypes.Contains(type), string.Format(message, type.Name));

        Assert.Equal(standardTypes.Count, availableTypes.Length);
    }
}

﻿using System.Collections.Immutable;

namespace NQuery.Authoring.Tests
{
    public abstract class ExtensionTests
    {
        private static ISet<Type> GetStandardProviderTypes<T>(Func<IEnumerable<T>> providers)
        {
            return new HashSet<Type>(providers().Select(t => t.GetType()));
        }

        private static ImmutableArray<Type> GetAvailableProviderTypes<T>()
        {
            var type = typeof(T);
            return type.Assembly.GetTypes()
                       .Where(t => !t.IsAbstract && type.IsAssignableFrom(t))
                       .ToImmutableArray();
        }

        protected static void AssertAllProvidersAreExposed<T>(Func<IEnumerable<T>> selector)
        {
            var availableTypes = GetAvailableProviderTypes<T>();
            var standardTypes = GetStandardProviderTypes(selector);
            var message = $"{{0}} isn't exposed from {selector.Method.DeclaringType.Name}.{selector.Method.Name}()";

            foreach (var type in availableTypes)
                Assert.True(standardTypes.Contains(type), string.Format(message, type.Name));

            Assert.Equal(standardTypes.Count, availableTypes.Length);
        }
    }
}
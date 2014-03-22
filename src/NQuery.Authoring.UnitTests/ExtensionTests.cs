using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Authoring.UnitTests
{
    public abstract class ExtensionTests
    {
        private static ISet<Type> GetStandardProviderTypes<T>(Func<IEnumerable<T>> providers)
        {
            return new HashSet<Type>(providers().Select(t => t.GetType()));
        }

        private static IReadOnlyCollection<Type> GetAvailableProviderTypes<T>()
        {
            var type = typeof(T);
            return type.Assembly.GetTypes()
                       .Where(t => !t.IsAbstract && type.IsAssignableFrom(t))
                       .ToArray();
        }

        protected static void AssertAllProvidersAreExposed<T>(Func<IEnumerable<T>> selector)
        {
            var availableTypes = GetAvailableProviderTypes<T>();
            var standardTypes = GetStandardProviderTypes(selector);
            var message = string.Format("{{0}} isn't exposed from {0}.{1}()", selector.Method.DeclaringType.Name, selector.Method.Name);

            foreach (var type in availableTypes)
                Assert.IsTrue(standardTypes.Contains(type), message, type.Name);

            Assert.AreEqual(standardTypes.Count, availableTypes.Count);
        }
    }
}
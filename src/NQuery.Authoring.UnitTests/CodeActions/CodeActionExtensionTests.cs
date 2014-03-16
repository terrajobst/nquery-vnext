using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.UnitTests.CodeActions
{
    [TestClass]
    public class CodeActionExtensionTests
    {
        private static IReadOnlyCollection<Type> GetProviderTypes<T>()
        {
            return typeof(T).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(T).IsAssignableFrom(t)).ToArray();
        }

        [TestMethod]
        public void CodeActionExtension_ReturnsAllIssueProviders()
        {
            var providers = CodeActionExtensions.GetStandardIssueProviders().Select(t => t.GetType()).ToArray();
            var actualTypes = new HashSet<Type>(providers);
            var types = GetProviderTypes<ICodeIssueProvider>();

            foreach (var type in types)
                Assert.IsTrue(actualTypes.Contains(type), "Issue provider {0} isn't exposed from GetStandardIssueProviders()", type);

            Assert.AreEqual(providers.Length, types.Count);
        }

        [TestMethod]
        public void CodeActionExtension_ReturnsAllRefactoringProviders()
        {
            var providers = CodeActionExtensions.GetStandardRefactoringProviders().Select(t => t.GetType()).ToArray();
            var actualTypes = new HashSet<Type>(providers);
            var types = GetProviderTypes<ICodeRefactoringProvider>();

            foreach (var type in types)
                Assert.IsTrue(actualTypes.Contains(type), "Refactoring provider {0} isn't exposed from GetStandardRefactoringProviders()", type);

            Assert.AreEqual(providers.Length, types.Count);
        }
    }
}
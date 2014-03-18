using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.UnitTests.SignatureHelp
{
    [TestClass]
    public class SignatureHelpExtensionsTests
    {
        [TestMethod]
        public void SignatureHelpExtensions_ReturnsAllProviders()
        {
            var allTypes = typeof(ISignatureHelpModelProvider).Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(ISignatureHelpModelProvider).IsAssignableFrom(t)).ToArray();
            var matchers = SignatureHelpExtensions.GetStandardSignatureHelpModelProviders().Select(t => t.GetType()).ToArray();
            var exposedTypes = new HashSet<Type>(matchers);

            foreach (var type in allTypes)
                Assert.IsTrue(exposedTypes.Contains(type), "Provider {0} isn't exposed from GetStandardSignatureHelpModelProviders()", type);

            Assert.AreEqual(matchers.Length, allTypes.Length);
        }
    }
}
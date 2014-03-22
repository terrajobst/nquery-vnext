using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.UnitTests.CodeActions
{
    [TestClass]
    public class CodeActionExtensionTests : ExtensionTests
    {
        [TestMethod]
        public void CodeActionExtension_ReturnsAllIssueProviders()
        {
            AssertAllProvidersAreExposed(CodeActionExtensions.GetStandardIssueProviders);
        }

        [TestMethod]
        public void CodeActionExtension_ReturnsAllRefactoringProviders()
        {
            AssertAllProvidersAreExposed(CodeActionExtensions.GetStandardRefactoringProviders);
        }
    }
}
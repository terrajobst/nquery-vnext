using System;

using Xunit;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.UnitTests.CodeActions
{
    public class CodeActionExtensionTests : ExtensionTests
    {
        [Fact]
        public void CodeActionExtension_ReturnsAllFixProviders()
        {
            AssertAllProvidersAreExposed(CodeActionExtensions.GetStandardFixProviders);
        }

        [Fact]
        public void CodeActionExtension_ReturnsAllIssueProviders()
        {
            AssertAllProvidersAreExposed(CodeActionExtensions.GetStandardIssueProviders);
        }

        [Fact]
        public void CodeActionExtension_ReturnsAllRefactoringProviders()
        {
            AssertAllProvidersAreExposed(CodeActionExtensions.GetStandardRefactoringProviders);
        }
    }
}
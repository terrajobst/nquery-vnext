using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Tests.CodeActions;

public class CodeActionExtensionTests : ExtensionTests
{
    [Fact]
    public void CodeActionExtension_ReturnsAllFixProviders()
    {
        AssertAllProvidersAreExposed(CodeActionExtensions.StandardFixProviders);
    }

    [Fact]
    public void CodeActionExtension_ReturnsAllIssueProviders()
    {
        AssertAllProvidersAreExposed(CodeActionExtensions.StandardIssueProviders);
    }

    [Fact]
    public void CodeActionExtension_ReturnsAllRefactoringProviders()
    {
        AssertAllProvidersAreExposed(CodeActionExtensions.StandardRefactoringProviders);
    }
}

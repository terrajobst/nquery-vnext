using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Tests.CodeActions;

public class CodeActionExtensionTests : ExtensionTests
{
    [Fact]
    public void CodeActionExtension_ReturnsAllFixProviders()
    {
        AssertAllProvidersAreExposed<ICodeFixProvider>();
    }

    [Fact]
    public void CodeActionExtension_ReturnsAllIssueProviders()
    {
        AssertAllProvidersAreExposed<ICodeIssueProvider>();
    }

    [Fact]
    public void CodeActionExtension_ReturnsAllRefactoringProviders()
    {
        AssertAllProvidersAreExposed<ICodeRefactoringProvider>();
    }
}

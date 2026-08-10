using System.Collections.Immutable;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Tests.CodeActions;

public abstract class CodeRefactoringTests : CodeActionTest
{
    protected override ImmutableArray<ICodeAction> GetActions(string query)
    {
        var services = DocumentFactory.ServicesWithOnly(CreateProvider());
        var document = DocumentFactory.CreateQuery(query, out int position, services);
        var view = DocumentView.Create(document, position);

        return document.Services.GetService<CodeRefactoringService>().GetRefactorings(view);
    }

    protected abstract ICodeRefactoringProvider CreateProvider();
}

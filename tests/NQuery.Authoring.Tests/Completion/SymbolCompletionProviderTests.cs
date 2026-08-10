using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;

namespace NQuery.Authoring.Tests.Completion;

public abstract class SymbolCompletionProviderTests
{
    protected static CompletionModel GetCompletionModel(string query)
    {
        var services = DocumentFactory.ServicesWithOnly<ICompletionProvider>(new SymbolCompletionProvider());
        var document = DocumentFactory.CreateQuery(query, out int position, services);

        return document.Services.GetService<CompletionService>().GetModel(DocumentView.Create(document, position));
    }
}

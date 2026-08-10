using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;
using NQuery.Authoring.Classifications;
using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Commenting;
using NQuery.Authoring.Completion;
using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Outlining;
using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.Selection;
using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SymbolSearch;

namespace NQuery.Authoring.Tests;

public class AuthoringServicesTests
{
    private static readonly Type[] AllServiceTypes =
    [
        typeof(BraceMatchingService),
        typeof(ClassificationService),
        typeof(CodeFixService),
        typeof(CodeIssueService),
        typeof(CodeRefactoringService),
        typeof(CommentingService),
        typeof(CompletionService),
        typeof(HighlightingService),
        typeof(OutliningService),
        typeof(QuickInfoService),
        typeof(SelectionService),
        typeof(SignatureHelpService),
        typeof(SymbolSearchService)
    ];

    [Fact]
    public void AuthoringServices_AddDefaultServices_RegistersEveryService()
    {
        var services = AuthoringServices.Create(b => b.AddDefaultServices());
        var resolve = typeof(AuthoringServices).GetMethod(nameof(AuthoringServices.GetService))!;

        foreach (var serviceType in AllServiceTypes)
        {
            var resolved = resolve.MakeGenericMethod(serviceType).Invoke(services, null);
            Assert.NotNull(resolved);
        }
    }

    [Fact]
    public void AuthoringServices_AddDefaultServices_ExposesEveryServiceInTheAssembly()
    {
        // Guards the same thing the per-feature provider tests guard: a service added to the
        // assembly but forgotten in AddDefaultServices would silently never be resolvable.
        var declared = typeof(AuthoringServices).Assembly
                                                .GetTypes()
                                                .Where(t => t.IsPublic && !t.IsAbstract && t.Name.EndsWith(@"Service", StringComparison.Ordinal))
                                                .ToArray();

        Assert.Equal(declared.OrderBy(t => t.Name), AllServiceTypes.OrderBy(t => t.Name));
    }

    [Fact]
    public void AuthoringServices_GetService_ThrowsWhenNotRegistered()
    {
        var services = AuthoringServices.Create(_ => { });

        Assert.Throws<InvalidOperationException>(() => services.GetService<CompletionService>());
    }

    [Fact]
    public void AuthoringServices_TryGetService_ReturnsFalseWhenNotRegistered()
    {
        var services = AuthoringServices.Create(_ => { });

        Assert.False(services.TryGetService<CompletionService>(out var service));
        Assert.Null(service);
    }

    [Fact]
    public void AuthoringServices_GetService_ReturnsSameInstance()
    {
        var services = AuthoringServices.Create(b => b.AddDefaultServices());

        Assert.Same(services.GetService<CompletionService>(), services.GetService<CompletionService>());
    }

    [Fact]
    public void AuthoringServices_GetProviders_PreservesRegistrationOrder()
    {
        var first = new ParenthesisBraceMatcher();
        var second = new CaseBraceMatcher();

        var services = AuthoringServices.Create(b => b.AddBraceMatcher(first).AddBraceMatcher(second));
        var providers = services.GetProviders<IBraceMatcher>();

        Assert.Equal([first, second], providers);
    }

    [Fact]
    public void AuthoringServices_GetProviders_ReturnsEmptyWhenNoneRegistered()
    {
        var services = AuthoringServices.Create(_ => { });

        Assert.Empty(services.GetProviders<IBraceMatcher>());
    }

    [Fact]
    public void AuthoringServices_RemoveProviders_DropsStandardSet()
    {
        var matcher = new ParenthesisBraceMatcher();

        var services = AuthoringServices.Create(b => b.AddDefaultServices()
                                                      .RemoveProviders<IBraceMatcher>()
                                                      .AddBraceMatcher(matcher));

        Assert.Equal([matcher], services.GetProviders<IBraceMatcher>());
    }

    [Fact]
    public void AuthoringServices_AddService_SeesProvidersRegisteredAfterwards()
    {
        // Services are materialized lazily against the completed bag, so composition order between
        // AddService and AddProvider must not matter.
        var services = AuthoringServices.Create(b => b.AddCompletionService()
                                                      .AddStandardCompletionProviders());

        Assert.NotEmpty(services.GetProviders<ICompletionProvider>());
        Assert.NotNull(services.GetService<CompletionService>());
    }

    [Fact]
    public void AuthoringServices_Document_CarriesServices()
    {
        var services = AuthoringServices.Create(b => b.AddDefaultServices());
        var document = DocumentFactory.CreateQuery(@"SELECT 1", services);

        Assert.Same(services, document.Services);
        Assert.Same(services, document.WithText(document.Text).Services);
    }
}

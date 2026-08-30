using System.Collections.Immutable;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;
using NQuery.Authoring.Classifications;
using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Commenting;
using NQuery.Authoring.Completion;
using NQuery.Authoring.Completion.Providers;
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

    // The service types a feature fans out over rather than resolving one of. Nothing in the
    // composition distinguishes them -- they are simply the ones registered more than once.
    private static readonly Type[] AllExtensionPointTypes =
    [
        typeof(IBraceMatcher),
        typeof(ICodeFixProvider),
        typeof(ICodeIssueProvider),
        typeof(ICodeRefactoringProvider),
        typeof(ICompletionProvider),
        typeof(IHighlighter),
        typeof(IOutliner),
        typeof(IQuickInfoModelProvider),
        typeof(ISelectionSpanProvider),
        typeof(ISignatureHelpModelProvider)
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
        // Guards the same thing the extension point test below guards: a service added to the
        // assembly but forgotten in AddDefaultServices would silently never be resolvable.
        var declared = typeof(AuthoringServices).Assembly
                                                .GetTypes()
                                                .Where(t => t.IsPublic && !t.IsAbstract && t.Name.EndsWith(@"Service", StringComparison.Ordinal))
                                                .ToArray();

        Assert.Equal(declared.OrderBy(t => t.Name), AllServiceTypes.OrderBy(t => t.Name));
    }

    [Fact]
    public void AuthoringServices_AddDefaultServices_ExposesEveryExtensionPointImplementation()
    {
        var assembly = typeof(AuthoringServices).Assembly;
        var getServices = typeof(AuthoringServices).GetMethod(nameof(AuthoringServices.GetServices))!;

        foreach (var extensionPointType in AllExtensionPointTypes)
        {
            var declared = assembly.GetTypes()
                                   .Where(t => !t.IsAbstract && extensionPointType.IsAssignableFrom(t));

            var registered = (IEnumerable<object>)getServices.MakeGenericMethod(extensionPointType)
                                                            .Invoke(DocumentFactory.DefaultServices, null)!;

            Assert.Equal(declared.OrderBy(t => t.Name),
                         registered.Select(s => s.GetType()).OrderBy(t => t.Name));
        }
    }

    [Fact]
    public void AuthoringServices_GetService_ThrowsWhenNotRegistered()
    {
        var services = AuthoringServices.Create(_ => { });

        Assert.Throws<InvalidOperationException>(() => services.GetService<CompletionService>());
    }

    [Fact]
    public void AuthoringServices_GetService_ThrowsWhenAmbiguous()
    {
        var services = AuthoringServices.Create(b => b.AddService<IBraceMatcher, ParenthesisBraceMatcher>()
                                                      .AddService<IBraceMatcher, CaseBraceMatcher>());

        Assert.Throws<InvalidOperationException>(() => services.GetService<IBraceMatcher>());
    }

    [Fact]
    public void AuthoringServices_TryGetService_ReturnsFalseWhenNotRegistered()
    {
        var services = AuthoringServices.Create(_ => { });

        Assert.False(services.TryGetService<CompletionService>(out var service));
        Assert.Null(service);
    }

    [Fact]
    public void AuthoringServices_TryGetService_ThrowsWhenAmbiguous()
    {
        // Absence is a condition a caller can handle; asking for one of something registered twice
        // is a composition bug, so it stays an exception even on the Try path.
        var services = AuthoringServices.Create(b => b.AddService<IBraceMatcher, ParenthesisBraceMatcher>()
                                                      .AddService<IBraceMatcher, CaseBraceMatcher>());

        Assert.Throws<InvalidOperationException>(() => services.TryGetService<IBraceMatcher>(out _));
    }

    [Fact]
    public void AuthoringServices_GetService_ReturnsSameInstance()
    {
        var services = AuthoringServices.Create(b => b.AddDefaultServices());

        Assert.Same(services.GetService<CompletionService>(), services.GetService<CompletionService>());
    }

    [Fact]
    public void AuthoringServices_GetServices_PreservesRegistrationOrder()
    {
        var first = new ParenthesisBraceMatcher();
        var second = new CaseBraceMatcher();

        var services = AuthoringServices.Create(b => b.AddService<IBraceMatcher>(first)
                                                      .AddService<IBraceMatcher>(second));

        Assert.Equal([first, second], services.GetServices<IBraceMatcher>());
    }

    [Fact]
    public void AuthoringServices_GetServices_ReturnsEmptyWhenNoneRegistered()
    {
        var services = AuthoringServices.Create(_ => { });

        Assert.Empty(services.GetServices<IBraceMatcher>());
    }

    [Fact]
    public void AuthoringServices_RemoveServices_DropsStandardSet()
    {
        var matcher = new ParenthesisBraceMatcher();

        var services = AuthoringServices.Create(b => b.AddDefaultServices()
                                                      .RemoveServices<IBraceMatcher>()
                                                      .AddService<IBraceMatcher>(matcher));

        Assert.Equal([matcher], services.GetServices<IBraceMatcher>());
    }

    [Fact]
    public void AuthoringServices_AddService_IgnoresRegistrationOrderOfDependencies()
    {
        // Nothing is constructed until Build, so a service may be registered before the extension
        // points it fans out over.
        var services = AuthoringServices.Create(b => b.AddService<CompletionService>()
                                                      .AddService<ICompletionProvider, KeywordCompletionProvider>());

        Assert.Single(services.GetServices<ICompletionProvider>());
        Assert.NotNull(services.GetService<CompletionService>());
    }

    [Fact]
    public void AuthoringServices_AddService_InjectsSingleDependency()
    {
        var services = AuthoringServices.Create(b => b.AddService<Dependency>()
                                                      .AddService<SingleConsumer>());

        Assert.Same(services.GetService<Dependency>(), services.GetService<SingleConsumer>().Dependency);
    }

    [Fact]
    public void AuthoringServices_AddService_InjectsEveryRegistrationAsCollection()
    {
        var services = AuthoringServices.Create(b => b.AddService<IThing, FirstThing>()
                                                      .AddService<IThing, SecondThing>()
                                                      .AddService<ArrayConsumer>()
                                                      .AddService<EnumerableConsumer>());

        var things = services.GetServices<IThing>();

        Assert.Equal(things, services.GetService<ArrayConsumer>().Things);
        Assert.Equal(things, services.GetService<EnumerableConsumer>().Things);
    }

    [Fact]
    public void AuthoringServices_AddService_ThrowsWhenDependencyIsMissing()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => AuthoringServices.Create(b => b.AddService<SingleConsumer>()));

        Assert.Contains(nameof(Dependency), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthoringServices_AddService_ThrowsWhenDependencyIsAmbiguous()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => AuthoringServices.Create(b => b.AddService<Dependency>()
                                                 .AddService<Dependency>()
                                                 .AddService<SingleConsumer>()));

        Assert.Contains(nameof(Dependency), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthoringServices_AddService_ThrowsOnCycle()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => AuthoringServices.Create(b => b.AddService<CycleStart>()
                                                 .AddService<CycleEnd>()));

        Assert.Contains(nameof(CycleStart), exception.Message, StringComparison.Ordinal);
        Assert.Contains(nameof(CycleEnd), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthoringServices_AddService_ThrowsWhenConstructorIsAmbiguous()
    {
        Assert.Throws<InvalidOperationException>(
            () => AuthoringServices.Create(b => b.AddService<TwoConstructors>()));
    }

    [Fact]
    public void AuthoringServices_Document_CarriesServices()
    {
        var services = AuthoringServices.Create(b => b.AddDefaultServices());
        var document = DocumentFactory.CreateQuery(@"SELECT 1", services);

        Assert.Same(services, document.Services);
        Assert.Same(services, document.WithText(document.Text).Services);
    }

    private sealed class Dependency;

    private sealed class SingleConsumer
    {
        public SingleConsumer(Dependency dependency)
        {
            Dependency = dependency;
        }

        public Dependency Dependency { get; }
    }

    private interface IThing;

    private sealed class FirstThing : IThing;

    private sealed class SecondThing : IThing;

    private sealed class ArrayConsumer
    {
        public ArrayConsumer(ImmutableArray<IThing> things)
        {
            Things = things;
        }

        public ImmutableArray<IThing> Things { get; }
    }

    private sealed class EnumerableConsumer
    {
        public EnumerableConsumer(IEnumerable<IThing> things)
        {
            Things = things;
        }

        public IEnumerable<IThing> Things { get; }
    }

    private sealed class CycleStart
    {
        public CycleStart(CycleEnd end)
        {
            End = end;
        }

        public CycleEnd End { get; }
    }

    private sealed class CycleEnd
    {
        public CycleEnd(CycleStart start)
        {
            Start = start;
        }

        public CycleStart Start { get; }
    }

    private sealed class TwoConstructors
    {
        public TwoConstructors()
        {
        }

        public TwoConstructors(Dependency dependency)
        {
            Dependency = dependency;
        }

        public Dependency? Dependency { get; }
    }
}

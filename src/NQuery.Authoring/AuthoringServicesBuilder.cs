using System.Collections.Immutable;
using System.Reflection;

using NQuery.Authoring.BraceMatching;
using NQuery.Authoring.BraceMatching.Matchers;
using NQuery.Authoring.Classifications;
using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Fixes;
using NQuery.Authoring.CodeActions.Issues;
using NQuery.Authoring.CodeActions.Refactorings;
using NQuery.Authoring.Commenting;
using NQuery.Authoring.Completion;
using NQuery.Authoring.Formatting;
using NQuery.Authoring.Completion.Providers;
using NQuery.Authoring.Highlighting;
using NQuery.Authoring.Highlighting.Highlighters;
using NQuery.Authoring.Outlining;
using NQuery.Authoring.Outlining.Outliners;
using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.QuickInfo.Providers;
using NQuery.Authoring.Selection;
using NQuery.Authoring.Selection.Providers;
using NQuery.Authoring.SignatureHelp;
using NQuery.Authoring.SignatureHelp.Providers;
using NQuery.Authoring.SymbolSearch;

namespace NQuery.Authoring;

// Configures an AuthoringServices. Obtained from AuthoringServices.Create, never constructed
// directly, so there is no half-built instance to hand around.
//
// The builder starts empty and is a flat, ordered list of registrations. Registration order is
// observable in two ways: GetServices hands a feature its extension points in this order, and
// AddDefaultServices seeds the built-ins first so that host registrations append after them.
// Nothing is constructed until Build, so a service may be registered before the things it depends
// on.
public sealed class AuthoringServicesBuilder
{
    private readonly List<ServiceRegistration> _registrations = [];

    internal AuthoringServicesBuilder()
    {
    }

    // The one place that knows the built-in feature set. Everything else here is feature-agnostic,
    // so a service living in another assembly is registered exactly the way these are.
    public AuthoringServicesBuilder AddDefaultServices()
    {
        AddService<BraceMatchingService>();
        AddService<ClassificationService>();
        AddService<CodeFixService>();
        AddService<CodeIssueService>();
        AddService<CodeRefactoringService>();
        AddService<CommentingService>();
        AddService<CompletionService>();
        AddService<FormattingOptionsResolver>();
        AddService<FormattingService>();
        AddService<HighlightingService>();
        AddService<OutliningService>();
        AddService<QuickInfoService>();
        AddService<SelectionService>();
        AddService<SignatureHelpService>();
        AddService<SymbolSearchService>();

        // BraceMatchingService takes the first match, so order is behavior here.
        AddService<IBraceMatcher, StringQuoteBraceMatcher>();
        AddService<IBraceMatcher, CaseBraceMatcher>();
        AddService<IBraceMatcher, DateBraceMatcher>();
        AddService<IBraceMatcher, IdentifierBraceMatcher>();
        AddService<IBraceMatcher, ParenthesisBraceMatcher>();

        AddService<ICodeFixProvider, AddOrderByToSelectDistinctCodeFixProvider>();
        AddService<ICodeFixProvider, AddParenthesesCodeFixProvider>();
        AddService<ICodeFixProvider, AddToGroupByCodeFixProvider>();

        AddService<ICodeIssueProvider, ColumnsInExistsCodeIssueProvider>();
        AddService<ICodeIssueProvider, ComparisonWithNullCodeIssueProvider>();
        AddService<ICodeIssueProvider, OrderByExpressionsCodeIssueProvider>();
        AddService<ICodeIssueProvider, OrderByOrdinalCodeIssueProvider>();
        AddService<ICodeIssueProvider, UnusedCommonTableExpressionCodeIssueProvider>();
        AddService<ICodeIssueProvider, RecursiveCodeIssueProvider>();

        AddService<ICodeRefactoringProvider, FlipBinaryOperatorSidesCodeRefactoringProvider>();
        AddService<ICodeRefactoringProvider, SortOrderCodeRefactoringProvider>();
        AddService<ICodeRefactoringProvider, AddAsAliasCodeRefactoringProvider>();
        AddService<ICodeRefactoringProvider, AddAsDerivedTableCodeRefactoringProvider>();
        AddService<ICodeRefactoringProvider, AddMissingKeywordCodeRefactoringProvider>();
        AddService<ICodeRefactoringProvider, ExpandWildcardCodeRefactoringProvider>();
        AddService<ICodeRefactoringProvider, QualifyColumnCodeRefactoringProvider>();
        AddService<ICodeRefactoringProvider, BetweenCodeRefactoringProvider>();
        AddService<ICodeRefactoringProvider, RemoveRedundantBracketsCodeRefactoringProvider>();
        AddService<ICodeRefactoringProvider, RemoveRedundantParenthesisCodeRefactoringProvider>();

        AddService<ICompletionProvider, AliasCompletionProvider>();
        AddService<ICompletionProvider, JoinCompletionProvider>();
        AddService<ICompletionProvider, KeywordCompletionProvider>();
        AddService<ICompletionProvider, SymbolCompletionProvider>();
        AddService<ICompletionProvider, TypeCompletionProvider>();
        AddService<ICompletionProvider, CommonTableExpressionCompletionProvider>();

        AddService<IHighlighter, CaseKeywordHighlighter>();
        AddService<IHighlighter, CastKeywordHighlighter>();
        AddService<IHighlighter, SelectQueryKeywordHighlighter>();
        AddService<IHighlighter, OrderedQueryKeywordHighlighter>();
        AddService<IHighlighter, InnerJoinKeywordHighlighter>();
        AddService<IHighlighter, OuterJoinKeywordHighlighter>();
        AddService<IHighlighter, SymbolReferenceHighlighter>();

        AddService<IOutliner, SelectQueryOutliner>();
        AddService<IOutliner, OrderedQueryOutliner>();
        AddService<IOutliner, MultiLineCommentOutliner>();
        AddService<IOutliner, SingleLineCommentOutliner>();

        AddService<IQuickInfoProvider, CastExpressionQuickInfoProvider>();
        AddService<IQuickInfoProvider, CoalesceExpressionQuickInfoProvider>();
        AddService<IQuickInfoProvider, CommonTableExpressionColumnNameQuickInfoProvider>();
        AddService<IQuickInfoProvider, CommonTableExpressionQuickInfoProvider>();
        AddService<IQuickInfoProvider, CountAllExpressionQuickInfoProvider>();
        AddService<IQuickInfoProvider, DerivedTableReferenceQuickInfoProvider>();
        AddService<IQuickInfoProvider, ExpressionSelectColumnQuickInfoProvider>();
        AddService<IQuickInfoProvider, FunctionInvocationExpressionQuickInfoProvider>();
        AddService<IQuickInfoProvider, MethodInvocationExpressionQuickInfoProvider>();
        AddService<IQuickInfoProvider, NamedTableReferenceQuickInfoProvider>();
        AddService<IQuickInfoProvider, NameExpressionQuickInfoProvider>();
        AddService<IQuickInfoProvider, NullIfQuickInfoProvider>();
        AddService<IQuickInfoProvider, PropertyAccessExpressionQuickInfoProvider>();
        AddService<IQuickInfoProvider, VariableExpressionQuickInfoProvider>();
        AddService<IQuickInfoProvider, WildcardSelectColumnQuickInfoProvider>();

        AddService<ISelectionSpanProvider, ArgumentListSelectionSpanProvider>();
        AddService<ISelectionSpanProvider, CommonTableExpressionColumnNameListSelectionSpanProvider>();
        AddService<ISelectionSpanProvider, CommonTableExpressionQuerySelectionSpanProvider>();
        AddService<ISelectionSpanProvider, FromClauseSelectionSpanProvider>();
        AddService<ISelectionSpanProvider, GroupByClauseSelectionSpanProvider>();
        AddService<ISelectionSpanProvider, OrderedQuerySelectionSpanProvider>();
        AddService<ISelectionSpanProvider, SelectClauseSelectionSpanProvider>();

        AddService<ISignatureHelpProvider, CastSignatureHelpProvider>();
        AddService<ISignatureHelpProvider, CoalesceSignatureHelpProvider>();
        AddService<ISignatureHelpProvider, CountAllSignatureHelpProvider>();
        AddService<ISignatureHelpProvider, FunctionSignatureHelpProvider>();
        AddService<ISignatureHelpProvider, MethodSignatureHelpProvider>();
        AddService<ISignatureHelpProvider, NullIfSignatureHelpProvider>();

        return this;
    }

    // Registers a type as itself, which is what a feature service such as CompletionService is.
    public AuthoringServicesBuilder AddService<TService>()
        where TService : class
    {
        return AddActivated(typeof(TService), typeof(TService));
    }

    public AuthoringServicesBuilder AddService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return AddActivated(typeof(TService), typeof(TImplementation));
    }

    public AuthoringServicesBuilder AddService<TService>(Func<AuthoringServices, TService> factory)
        where TService : class
    {
        ThrowIfNull(factory);

        _registrations.Add(new ServiceRegistration(typeof(TService), typeof(TService), s => factory(s)));
        return this;
    }

    public AuthoringServicesBuilder AddService<TService>(TService instance)
        where TService : class
    {
        ThrowIfNull(instance);

        _registrations.Add(new ServiceRegistration(typeof(TService), instance.GetType(), _ => instance));
        return this;
    }

    // Drops every registration for the service type, which is how a host replaces a built-in
    // extension point set rather than appending to it.
    public AuthoringServicesBuilder RemoveServices<TService>()
        where TService : class
    {
        _registrations.RemoveAll(r => r.ServiceType == typeof(TService));
        return this;
    }

    internal AuthoringServices Build()
    {
        var registrations = _registrations.ToImmutableArray();
        var services = new AuthoringServices(registrations);
        services.ResolveAll(registrations);
        return services;
    }

    private AuthoringServicesBuilder AddActivated(Type serviceType, Type implementationType)
    {
        var constructor = GetConstructor(implementationType);
        _registrations.Add(new ServiceRegistration(serviceType, implementationType, s => s.Activate(constructor)));
        return this;
    }

    // Deliberately public-only and exactly one: an activator that reaches for a constructor the type
    // didn't offer is doing something its author never sanctioned, and picking between overloads by
    // what happens to be registered makes the choice depend on composition order. A type that needs
    // either is registered with a factory instead.
    private static ConstructorInfo GetConstructor(Type implementationType)
    {
        if (implementationType.IsAbstract)
            throw new InvalidOperationException($"{implementationType.Name} is abstract and cannot be registered by type.");

        var constructors = implementationType.GetConstructors();
        if (constructors.Length != 1)
        {
            var message = $"{implementationType.Name} has {constructors.Length} public constructors, " +
                          $"but registering by type requires exactly one. Register it with a factory instead.";
            throw new InvalidOperationException(message);
        }

        return constructors[0];
    }
}

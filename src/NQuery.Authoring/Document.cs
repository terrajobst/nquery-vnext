using System.Diagnostics.CodeAnalysis;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring;

// An immutable snapshot of a query, plus everything needed to analyze it: the catalog it binds
// against and the language services it is analyzed with.
//
// This is the only asynchronous boundary in the authoring layer. Nothing below a document does I/O
// -- parsing and binding are CPU-bound throughout -- so whether that work is offloaded to another
// thread is the host's policy, not the library's. Every artifact is therefore available three ways:
// Get computes in place, GetAsync offloads, TryGet answers only if the value is already cached.
// All three yield the same instance; see AsyncLazy for why that matters.
//
// There is deliberately no back-reference to whatever mutable state a host keeps alongside a
// document. A document is a snapshot and that state moves on, so reaching from one to the other
// would observe a version that no longer matches -- and documents exist without any such state at
// all.
public sealed class Document
{
    private readonly AsyncLazy<SyntaxTree> _syntaxTree;
    private readonly AsyncLazy<Compilation> _compilation;
    private readonly AsyncLazy<SemanticModel> _semanticModel;

    private Document(DocumentKind kind, SourceText text, Catalog catalog, AuthoringServices services)
    {
        Kind = kind;
        Text = text;
        Catalog = catalog;
        Services = services;

        _syntaxTree = new AsyncLazy<SyntaxTree>(_ => ComputeSyntaxTree());
        _compilation = new AsyncLazy<Compilation>(c => Compilation.Create(Catalog, GetSyntaxTree(c)));
        _semanticModel = new AsyncLazy<SemanticModel>(c => GetCompilation(c).GetSemanticModel());
    }

    // services is required rather than defaulted: a document that silently fell back to a standard
    // set would drop whatever the host configured, which is the failure this layer exists to remove.
    public static Document Create(DocumentKind kind, SourceText text, Catalog catalog, AuthoringServices services)
    {
        ThrowIfNull(text);
        ThrowIfNull(catalog);
        ThrowIfNull(services);

        return new Document(kind, text, catalog, services);
    }

    public DocumentKind Kind { get; }

    public SourceText Text { get; }

    public Catalog Catalog { get; }

    public AuthoringServices Services { get; }

    public bool TryGetSyntaxTree([NotNullWhen(true)] out SyntaxTree? syntaxTree)
    {
        return _syntaxTree.TryGetValue(out syntaxTree);
    }

    public bool TryGetCompilation([NotNullWhen(true)] out Compilation? compilation)
    {
        return _compilation.TryGetValue(out compilation);
    }

    public bool TryGetSemanticModel([NotNullWhen(true)] out SemanticModel? semanticModel)
    {
        return _semanticModel.TryGetValue(out semanticModel);
    }

    public SyntaxTree GetSyntaxTree(CancellationToken cancellationToken = default)
    {
        return _syntaxTree.GetValue(cancellationToken);
    }

    public Compilation GetCompilation(CancellationToken cancellationToken = default)
    {
        return _compilation.GetValue(cancellationToken);
    }

    public SemanticModel GetSemanticModel(CancellationToken cancellationToken = default)
    {
        return _semanticModel.GetValue(cancellationToken);
    }

    public Task<SyntaxTree> GetSyntaxTreeAsync(CancellationToken cancellationToken = default)
    {
        return _syntaxTree.GetValueAsync(cancellationToken);
    }

    public Task<Compilation> GetCompilationAsync(CancellationToken cancellationToken = default)
    {
        return _compilation.GetValueAsync(cancellationToken);
    }

    public Task<SemanticModel> GetSemanticModelAsync(CancellationToken cancellationToken = default)
    {
        return _semanticModel.GetValueAsync(cancellationToken);
    }

    private SyntaxTree ComputeSyntaxTree()
    {
        switch (Kind)
        {
            case DocumentKind.Query:
                return SyntaxTree.ParseQuery(Text);
            case DocumentKind.Expression:
                return SyntaxTree.ParseExpression(Text);
            default:
                throw ExceptionBuilder.UnexpectedValue(Kind);
        }
    }

    public Document WithKind(DocumentKind kind)
    {
        return kind == Kind ? this : new Document(kind, Text, Catalog, Services);
    }

    public Document WithText(SourceText text)
    {
        ThrowIfNull(text);

        return text == Text ? this : new Document(Kind, text, Catalog, Services);
    }

    public Document WithCatalog(Catalog catalog)
    {
        ThrowIfNull(catalog);

        return catalog == Catalog ? this : new Document(Kind, Text, catalog, Services);
    }

    public Document WithServices(AuthoringServices services)
    {
        ThrowIfNull(services);

        return services == Services ? this : new Document(Kind, Text, Catalog, services);
    }
}

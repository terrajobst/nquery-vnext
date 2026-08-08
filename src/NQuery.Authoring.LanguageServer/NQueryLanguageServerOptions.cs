using System.Collections.Immutable;

using NQuery.Authoring.Classifications;
using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Completion;
using NQuery.Authoring.Highlighting;
using NQuery.Authoring.LanguageServer.Hosting;
using NQuery.Authoring.Outlining;
using NQuery.Authoring.QuickInfo;
using NQuery.Authoring.Selection;
using NQuery.Authoring.SignatureHelp;

namespace NQuery.Authoring.LanguageServer;

public sealed class NQueryLanguageServerOptions
{
    // Called once at initialize, after the project's settings blob has arrived -- which is why
    // this is a factory rather than an instance. Required.
    public Func<ProjectContext, ICatalogProvider>? CatalogProviderFactory { get; set; }

    public string ServerName { get; set; } = @"NQuery Language Server";

    public string? ServerVersion { get; set; }

    // How long to wait after the last keystroke before recomputing diagnostics.
    public TimeSpan DiagnosticsDelay { get; set; } = TimeSpan.FromMilliseconds(300);

    // Running a query executes against whatever the catalog is backed by. A host serving a
    // production system, or a catalog that carries schema without data, should turn this off;
    // the client hides the Run Query command when it is disabled.
    public bool AllowExecution { get; set; } = true;

    // Hard cap on rows returned to the client. A client may request fewer, never more.
    public int MaxRows { get; set; } = 1000;

    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    // Appended to the Standard* provider sets, mirroring how NQuery.Authoring.Composition
    // aggregates MEF-exported providers on top of the built-in ones.
    public IList<ICompletionProvider> AdditionalCompletionProviders { get; } = [];

    public IList<IQuickInfoModelProvider> AdditionalQuickInfoModelProviders { get; } = [];

    public IList<ISignatureHelpModelProvider> AdditionalSignatureHelpModelProviders { get; } = [];

    public IList<IOutliner> AdditionalOutliners { get; } = [];

    public IList<IHighlighter> AdditionalHighlighters { get; } = [];

    public IList<ISelectionSpanProvider> AdditionalSelectionSpanProviders { get; } = [];

    public IList<ICodeFixProvider> AdditionalCodeFixProviders { get; } = [];

    public IList<ICodeIssueProvider> AdditionalCodeIssueProviders { get; } = [];

    public IList<ICodeRefactoringProvider> AdditionalCodeRefactoringProviders { get; } = [];

    internal ImmutableArray<ICompletionProvider> CompletionProviders
        => CompletionExtensions.StandardCompletionProviders.AddRange(AdditionalCompletionProviders);

    internal ImmutableArray<IQuickInfoModelProvider> QuickInfoModelProviders
        => QuickInfoExtensions.StandardQuickInfoModelProviders.AddRange(AdditionalQuickInfoModelProviders);

    internal ImmutableArray<ISignatureHelpModelProvider> SignatureHelpModelProviders
        => SignatureHelpExtensions.StandardSignatureHelpModelProviders.AddRange(AdditionalSignatureHelpModelProviders);

    internal ImmutableArray<IOutliner> Outliners
        => OutliningExtensions.StandardOutliners.AddRange(AdditionalOutliners);

    internal ImmutableArray<IHighlighter> Highlighters
        => HighlightingExtensions.StandardHighlighters.AddRange(AdditionalHighlighters);

    internal ImmutableArray<ISelectionSpanProvider> SelectionSpanProviders
        => SelectionExtensions.StandardSelectionSpanProviders.AddRange(AdditionalSelectionSpanProviders);

    internal ImmutableArray<ICodeFixProvider> CodeFixProviders
        => CodeActionExtensions.StandardFixProviders.AddRange(AdditionalCodeFixProviders);

    internal ImmutableArray<ICodeIssueProvider> CodeIssueProviders
        => CodeActionExtensions.StandardIssueProviders.AddRange(AdditionalCodeIssueProviders);

    internal ImmutableArray<ICodeRefactoringProvider> CodeRefactoringProviders
        => CodeActionExtensions.StandardRefactoringProviders.AddRange(AdditionalCodeRefactoringProviders);
}

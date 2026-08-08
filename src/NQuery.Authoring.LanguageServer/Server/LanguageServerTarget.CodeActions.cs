using NQuery.Authoring.CodeActions;
using NQuery.Authoring.LanguageServer.Mapping;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

using StreamJsonRpc;

using LspCodeAction = NQuery.Authoring.LanguageServer.Protocol.CodeAction;
using LspCodeActionKind = NQuery.Authoring.LanguageServer.Protocol.CodeActionKind;

namespace NQuery.Authoring.LanguageServer.Server;

internal sealed partial class LanguageServerTarget
{
    [JsonRpcMethod(Methods.TextDocumentCodeAction, UseSingleObjectParameterDeserialization = true)]
    public async Task<LspCodeAction[]?> CodeActionAsync(CodeActionParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var snapshot = await TryGetSnapshotAsync(parameters.TextDocument.Uri, cancellationToken);
        var semanticModel = await TryGetSemanticModelAsync(snapshot, cancellationToken);
        if (snapshot is null || semanticModel is null)
            return null;

        // Without a catalog nothing binds, so every provider would either find nothing or reason
        // about a document it cannot resolve.
        if (CatalogError is not null)
            return [];

        var document = snapshot.Value.Document;
        var text = document.Text;
        var span = text.ToTextSpan(parameters.Range);
        var only = parameters.Context?.Only;

        // The client spells the URI; echo it back exactly so it can match the edit to the document.
        var uri = parameters.TextDocument.Uri.OriginalString;

        var result = new List<LspCodeAction>();

        if (Includes(only, LspCodeActionKind.QuickFix))
        {
            foreach (var action in semanticModel.GetFixes(span.Start, _options.CodeFixProviders))
                AddAction(result, action, LspCodeActionKind.QuickFix, document, uri);

            // A CodeIssue carries its own fixes, and they are separate from the fix providers:
            // issues are found by scanning the document, fixes by reacting to a diagnostic.
            foreach (var issue in semanticModel.GetIssues(_options.CodeIssueProviders))
            {
                if (!issue.Span.IntersectsWith(span))
                    continue;

                foreach (var action in issue.Actions)
                    AddAction(result, action, LspCodeActionKind.QuickFix, document, uri);
            }
        }

        if (Includes(only, LspCodeActionKind.Refactor))
        {
            foreach (var action in semanticModel.GetRefactorings(span.Start, _options.CodeRefactoringProviders))
                AddAction(result, action, LspCodeActionKind.Refactor, document, uri);
        }

        return result.ToArray();
    }

    // VS Code asks for a specific kind when the user picks "Refactor..." rather than the general
    // lightbulb. Prefix matching, because "refactor.extract" requests should match "refactor".
    private static bool Includes(IReadOnlyList<string>? only, string kind)
    {
        return only is null
            || only.Count == 0
            || only.Any(k => kind.StartsWith(k, StringComparison.Ordinal) || k.StartsWith(kind, StringComparison.Ordinal));
    }

    private static void AddAction(List<LspCodeAction> result,
                                  ICodeAction action,
                                  string kind,
                                  Document document,
                                  string uri)
    {
        var edit = TryGetEdit(action, document, uri);
        if (edit is null)
            return;

        // Providers can legitimately offer the same action twice -- an issue's fix and a fix
        // provider reaching the same conclusion -- and a duplicated lightbulb entry looks broken.
        if (result.Any(a => a.Title == action.Description && SameEdit(a.Edit, edit)))
            return;

        result.Add(new LspCodeAction
        {
            Title = action.Description,
            Kind = kind,
            Edit = edit
        });
    }

    /// <summary>
    /// Turns a code action into precise text edits rather than a whole-document replacement.
    /// </summary>
    /// <remarks>
    /// GetEdit() returns the rewritten SyntaxTree, whose text is a ChangedSourceText chained to
    /// the original, so GetChanges walks that chain and hands back the exact TextChangeSet the
    /// action built -- no diffing, and the user's cursor and folding survive.
    ///
    /// This reparses the document once per offered action, which is fine at the size of a query
    /// but is the thing to revisit if the lightbulb ever feels slow: the fix is codeAction/resolve,
    /// computing the edit only for the action the user actually picks.
    /// </remarks>
    private static WorkspaceEdit? TryGetEdit(ICodeAction action, Document document, string uri)
    {
        SyntaxTree edited;

        try
        {
            edited = action.GetEdit();
        }
        catch (Exception)
        {
            // A provider that throws should cost its own action, not the whole lightbulb.
            return null;
        }

        var text = document.Text;
        var changes = edited.Text.GetChanges(text).ToArray();

        if (changes.Length == 0)
            return null;

        // Every range is against the document as it is now, and TextChangeSet guarantees they do
        // not overlap -- which is exactly what LSP requires of a single document's edits.
        var edits = changes.Select(c => new TextEdit
        {
            Range = text.ToRange(c.Span),
            NewText = c.NewText
        }).ToArray();

        return new WorkspaceEdit
        {
            Changes = new Dictionary<string, TextEdit[]> { [uri] = edits }
        };
    }

    private static bool SameEdit(WorkspaceEdit? x, WorkspaceEdit? y)
    {
        if (x is null || y is null)
            return ReferenceEquals(x, y);

        foreach (var (uri, xEdits) in x.Changes)
        {
            if (!y.Changes.TryGetValue(uri, out var yEdits) || xEdits.Length != yEdits.Length)
                return false;

            for (var i = 0; i < xEdits.Length; i++)
            {
                if (xEdits[i].NewText != yEdits[i].NewText || !Equals(xEdits[i].Range, yEdits[i].Range))
                    return false;
            }
        }

        return x.Changes.Count == y.Changes.Count;
    }
}

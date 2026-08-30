using NQuery.CodeAnalysis;

namespace NQuery.Authoring.CodeActions;

public abstract class CodeFixProvider : ICodeFixProvider
{
    public abstract IEnumerable<DiagnosticId> GetFixableDiagnosticIds();

    public IEnumerable<ICodeAction> GetFixes(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);
        var position = view.Position;

        var applicableDiagnostics = semanticModel.GetDiagnostics()
                                                 .Where(d => d.Span.ContainsOrTouches(position))
                                                 .Where(d => GetFixableDiagnosticIds().Contains(d.DiagnosticId));

        return applicableDiagnostics.SelectMany(d => GetFixes(semanticModel, position, d));
    }

    protected abstract IEnumerable<ICodeAction> GetFixes(SemanticModel semanticModel, int position, Diagnostic diagnostic);
}

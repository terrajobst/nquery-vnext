using System.Collections.Immutable;

using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Classifications;

// Has no extension points, which is deliberately not visible from the outside: a caller asks this
// service for classifications exactly the way it asks CompletionService for a model.
public sealed class ClassificationService
{
    public ImmutableArray<SyntaxClassificationSpan> ClassifySyntax(Document document, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);

        var root = document.GetSyntaxTree(cancellationToken).Root;
        return ClassifySyntax(document, root.FullSpan, cancellationToken);
    }

    public ImmutableArray<SyntaxClassificationSpan> ClassifySyntax(Document document, TextSpan span, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);

        var root = document.GetSyntaxTree(cancellationToken).Root;

        var result = new List<SyntaxClassificationSpan>();
        var worker = new SyntaxClassificationWorker(result, span);
        worker.ClassifyNode(root);
        return [.. result];
    }

    public ImmutableArray<SemanticClassificationSpan> ClassifySemantics(Document document, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);

        var root = document.GetSyntaxTree(cancellationToken).Root;
        return ClassifySemantics(document, root.FullSpan, cancellationToken);
    }

    public ImmutableArray<SemanticClassificationSpan> ClassifySemantics(Document document, TextSpan span, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(document);

        var semanticModel = document.GetSemanticModel(cancellationToken);
        var root = semanticModel.SyntaxTree.Root;

        var result = new List<SemanticClassificationSpan>();
        var worker = new SemanticClassificationWorker(result, semanticModel, span);
        worker.ClassifyNode(root);
        return [.. result];
    }
}

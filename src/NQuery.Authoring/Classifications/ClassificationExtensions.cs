using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Classifications;

public static class ClassificationExtensions
{
    extension(SyntaxNode root)
    {
        public IReadOnlyList<SyntaxClassificationSpan> ClassifySyntax()
        {
            ThrowIfNull(root);

            return root.ClassifySyntax(root.FullSpan);
        }

        public IReadOnlyList<SyntaxClassificationSpan> ClassifySyntax(TextSpan span)
        {
            ThrowIfNull(root);

            var result = new List<SyntaxClassificationSpan>();
            var worker = new SyntaxClassificationWorker(result, span);
            worker.ClassifyNode(root);
            return result;
        }

        public IReadOnlyList<SemanticClassificationSpan> ClassifySemantics(SemanticModel semanticModel)
        {
            ThrowIfNull(root);

            return root.ClassifySemantics(semanticModel, root.FullSpan);
        }

        public IReadOnlyList<SemanticClassificationSpan> ClassifySemantics(SemanticModel semanticModel, TextSpan span)
        {
            ThrowIfNull(root);
            ThrowIfNull(semanticModel);

            var result = new List<SemanticClassificationSpan>();
            var worker = new SemanticClassificationWorker(result, semanticModel, span);
            worker.ClassifyNode(root);
            return result;
        }
    }
}

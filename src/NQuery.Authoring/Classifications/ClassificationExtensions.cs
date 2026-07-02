using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Classifications;

public static class ClassificationExtensions
{
    extension(SyntaxNode root)
    {
        public IReadOnlyList<SyntaxClassificationSpan> ClassifySyntax()
        {
            return root.ClassifySyntax(root.FullSpan);
        }

        public IReadOnlyList<SyntaxClassificationSpan> ClassifySyntax(TextSpan span)
        {
            var result = new List<SyntaxClassificationSpan>();
            var worker = new SyntaxClassificationWorker(result, span);
            worker.ClassifyNode(root);
            return result;
        }

        public IReadOnlyList<SemanticClassificationSpan> ClassifySemantics(SemanticModel semanticModel)
        {
            return root.ClassifySemantics(semanticModel, root.FullSpan);
        }

        public IReadOnlyList<SemanticClassificationSpan> ClassifySemantics(SemanticModel semanticModel, TextSpan span)
        {
            var result = new List<SemanticClassificationSpan>();
            var worker = new SemanticClassificationWorker(result, semanticModel, span);
            worker.ClassifyNode(root);
            return result;
        }
    }
}

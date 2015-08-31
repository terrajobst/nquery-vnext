using System;
using System.Collections.Generic;

using NQuery.Text;

namespace NQuery.Authoring.Classifications
{
    public static class ClassificationExtensions
    {
        public static IReadOnlyList<SyntaxClassificationSpan> ClassifyTokens(this SourceText sourceText)
        {
            var span = new TextSpan(0, sourceText.Length);
            return sourceText.ClassifyTokens(span);
        }

        public static IReadOnlyList<SyntaxClassificationSpan> ClassifyTokens(this SourceText sourceText, TextSpan span)
        {
            var tokens = SyntaxFacts.ParseTokens(sourceText);

            var result = new List<SyntaxClassificationSpan>();
            var worker = new SyntaxClassificationWorker(result, span);
            worker.ClassifyTokens(tokens);
            return result;
        }

        public static IReadOnlyList<SyntaxClassificationSpan> ClassifySyntax(this SyntaxNode root)
        {
            return root.ClassifySyntax(root.FullSpan);
        }

        public static IReadOnlyList<SyntaxClassificationSpan> ClassifySyntax(this SyntaxNode root, TextSpan span)
        {
            var result = new List<SyntaxClassificationSpan>();
            var worker = new SyntaxClassificationWorker(result, span);
            worker.ClassifyNode(root);
            return result;
        }

        public static IReadOnlyList<SemanticClassificationSpan> ClassifySemantics(this SyntaxNode root, SemanticModel semanticModel)
        {
            return root.ClassifySemantics(semanticModel, root.FullSpan);
        }

        public static IReadOnlyList<SemanticClassificationSpan> ClassifySemantics(this SyntaxNode root, SemanticModel semanticModel, TextSpan span)
        {
            var result = new List<SemanticClassificationSpan>();
            var worker = new SemanticClassificationWorker(result, semanticModel, span);
            worker.ClassifyNode(root);
            return result;
        }
    }
}
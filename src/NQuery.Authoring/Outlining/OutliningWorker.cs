using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.Outlining
{
    internal sealed class OutliningWorker
    {
        private readonly SourceText _sourceText;
        private readonly List<OutliningRegionSpan> _outlineRegions;
        private readonly TextSpan _span;
        private readonly ImmutableArray<IOutliner> _outliners;

        public OutliningWorker(SourceText sourceText, List<OutliningRegionSpan> outlineRegions, TextSpan span, IEnumerable<IOutliner> outliners)
        {
            _sourceText = sourceText;
            _outlineRegions = outlineRegions;
            _span = span;
            _outliners = outliners.ToImmutableArray();
        }

        public void Visit(SyntaxNode node)
        {
            VisitNodeOrToken(node);
        }

        private void VisitNodeOrToken(SyntaxNodeOrToken nodeOrToken)
        {
            AddOutliningRegions(nodeOrToken);

            if (nodeOrToken.IsNode)
            {
                var children = nodeOrToken.AsNode()
                                          .ChildNodesAndTokens()
                                          .SkipWhile(c => !c.FullSpan.IntersectsWith(_span))
                                          .TakeWhile(c => c.FullSpan.IntersectsWith(_span));

                foreach (var syntaxNodeOrToken in children)
                    VisitNodeOrToken(syntaxNodeOrToken);
            }
        }

        private void AddOutliningRegions(SyntaxNodeOrToken nodeOrToken)
        {
            var regions = _outliners.SelectMany(o => o.FindRegions(nodeOrToken).Where(r => IsMultipleLines(r.Span)));

            _outlineRegions.AddRange(regions);
        }

        private bool IsMultipleLines(TextSpan textSpan)
        {
            var start = _sourceText.GetLineFromPosition(textSpan.Start);
            var end = _sourceText.GetLineFromPosition(textSpan.End);
            return start.LineNumber != end.LineNumber;
        }
    }
}
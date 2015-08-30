using System;
using System.Collections.Immutable;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining.Implementation;

using NQuery.Authoring.Outlining;

using IOutliner = NQuery.Authoring.Outlining.IOutliner;

namespace NQuery.Authoring.ActiproWpf.Outlining
{
    internal sealed class NQueryOutliningSource : RangeOutliningSourceBase
    {
        public NQueryOutliningSource(ITextSnapshot snapshot, SyntaxTree syntaxTree, ImmutableArray<IOutliner> outliners)
            : base(snapshot)
        {
            var text = syntaxTree.Text;
            var result = syntaxTree.Root.FindRegions(outliners);

            foreach (var regionSpan in result)
            {
                var range = text.ToSnapshotRange(regionSpan.Span);

                IOutliningNodeDefinition nodeDefinition = new OutliningNodeDefinition("NQueryNode")
                {
                    DefaultCollapsedContent = regionSpan.Text,
                    IsImplementation = false
                };

                AddNode(range, nodeDefinition);
            }
        }
    }
}
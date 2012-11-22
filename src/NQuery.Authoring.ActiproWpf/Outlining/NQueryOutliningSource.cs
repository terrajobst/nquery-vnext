using System;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining.Implementation;

using NQuery.Authoring.Outlining;

namespace NQuery.Authoring.ActiproWpf.Outlining
{
    internal sealed class NQueryOutliningSource : RangeOutliningSourceBase
    {
        public NQueryOutliningSource(ITextSnapshot snapshot, SyntaxTree syntaxTree)
            : base(snapshot)
        {
            var textBuffer = syntaxTree.TextBuffer;
            var result = syntaxTree.Root.FindRegions();

            foreach (var regionSpan in result)
            {
                var range = textBuffer.ToSnapshotRange(snapshot, regionSpan.Span);

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
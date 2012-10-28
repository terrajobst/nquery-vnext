using System;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining.Implementation;

using NQuery.Language.Services.Outlining;

namespace NQuery.Language.ActiproWpf.Outlining
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
                var text = textBuffer.GetText(regionSpan.Span);

                IOutliningNodeDefinition nodeDefinition = new OutliningNodeDefinition("NQueryNode")
                {
                    DefaultCollapsedContent = text,
                    IsImplementation = false
                };

                AddNode(range, nodeDefinition);
            }
        }
    }
}
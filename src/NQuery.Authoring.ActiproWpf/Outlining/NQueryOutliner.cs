using System;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining;

namespace NQuery.Authoring.ActiproWpf.Outlining
{
    internal sealed class NQueryOutliner : IOutliner
    {
        public IOutliningSource GetOutliningSource(ITextSnapshot snapshot)
        {
            var document = snapshot.Document.GetNQueryDocument();
            if (document == null)
                return null;

            SyntaxTree syntaxTree;
            if (!document.TryGetSyntaxTree(out syntaxTree))
                return null;

            return new NQueryOutliningSource(snapshot, syntaxTree);
        }

        public AutomaticOutliningUpdateTrigger UpdateTrigger
        {
            get { return AutomaticOutliningUpdateTrigger.ParseDataChanged; }
        }
    }
}
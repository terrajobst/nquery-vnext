using System;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining;

namespace NQuery.Authoring.ActiproWpf.Outlining
{
    internal sealed class NQueryOutliner : IOutliner
    {
        public IOutliningSource GetOutliningSource(ITextSnapshot snapshot)
        {
            var parseData = snapshot.GetParseData();
            if (parseData == null)
                return null;

            return new NQueryOutliningSource(snapshot, parseData.SyntaxTree);
        }

        public AutomaticOutliningUpdateTrigger UpdateTrigger
        {
            get { return AutomaticOutliningUpdateTrigger.ParseDataChanged; }
        }
    }
}
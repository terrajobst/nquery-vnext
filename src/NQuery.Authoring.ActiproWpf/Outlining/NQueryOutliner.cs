using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining;

using IAuthoringOutliner = NQuery.Authoring.Outlining.IOutliner;

namespace NQuery.Authoring.ActiproWpf.Outlining
{
    internal sealed class NQueryOutliner : INQueryOutliner
    {
        private readonly Collection<IAuthoringOutliner> _outliners = new Collection<IAuthoringOutliner>();

        public Collection<IAuthoringOutliner> Outliners
        {
            get { return _outliners; }
        }

        public IOutliningSource GetOutliningSource(ITextSnapshot snapshot)
        {
            var document = snapshot.ToDocument();

            SyntaxTree syntaxTree;
            if (!document.TryGetSyntaxTree(out syntaxTree))
                return null;

            return new NQueryOutliningSource(snapshot, syntaxTree, _outliners.ToImmutableArray());
        }

        public AutomaticOutliningUpdateTrigger UpdateTrigger
        {
            get { return AutomaticOutliningUpdateTrigger.ParseDataChanged; }
        }
    }
}
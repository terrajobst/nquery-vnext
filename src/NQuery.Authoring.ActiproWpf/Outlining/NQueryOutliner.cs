using System.Collections.Immutable;
using System.Collections.ObjectModel;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining;

using IAuthoringOutliner = NQuery.Authoring.Outlining.IOutliner;

namespace NQuery.Authoring.ActiproWpf.Outlining
{
    internal sealed class NQueryOutliner : INQueryOutliner
    {
        public Collection<IAuthoringOutliner> Outliners { get; } = new();

        public IOutliningSource GetOutliningSource(ITextSnapshot snapshot)
        {
            var document = snapshot.ToDocument();

            if (!document.TryGetSyntaxTree(out var syntaxTree))
                return null;

            return new NQueryOutliningSource(snapshot, syntaxTree, Outliners.ToImmutableArray());
        }

        public AutomaticOutliningUpdateTrigger UpdateTrigger
        {
            get { return AutomaticOutliningUpdateTrigger.ParseDataChanged; }
        }
    }
}
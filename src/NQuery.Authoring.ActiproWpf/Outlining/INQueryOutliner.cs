using System.Collections.ObjectModel;

using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining;

namespace NQuery.Authoring.ActiproWpf.Outlining
{
    public interface INQueryOutliner : IOutliner
    {
        Collection<Authoring.Outlining.IOutliner> Outliners { get; }
    }
}
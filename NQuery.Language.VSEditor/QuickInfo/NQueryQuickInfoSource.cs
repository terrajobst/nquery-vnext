using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryQuickInfoSource : IQuickInfoSource
    {
        public void Dispose()
        {
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            IQuickInfoManager quickInfoManager;
            if (!session.Properties.TryGetProperty(typeof(IQuickInfoManager), out quickInfoManager))
                return;

            var model = quickInfoManager.Model;
            var textSpan = model.NodeOrToken.Span;
            var symbol = model.Symbol;
            var span = new Span(textSpan.Start, textSpan.Length);
            var currentSnapshot = session.TextView.TextBuffer.CurrentSnapshot;

            applicableToSpan = currentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeNegative);
            quickInfoContent.Add(symbol.Name + ": " + symbol.Kind.ToString().ToUpper());
        }
    }
}
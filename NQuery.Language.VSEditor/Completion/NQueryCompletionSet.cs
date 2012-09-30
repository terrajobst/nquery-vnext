using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace NQuery.Language.VSEditor.Completion
{
    internal sealed class NQueryCompletionSet : CompletionSet
    {
        private readonly ICompletionSession _session;
        private readonly INQueryGlyphService _glyphService;
        private readonly ICompletionModelManager _completionModelManager;

        public NQueryCompletionSet(ICompletionSession session, INQueryGlyphService glyphService, ICompletionModelManager completionModelManager)
        {
            _session = session;
            _glyphService = glyphService;
            _completionModelManager = completionModelManager;
            _completionModelManager.ModelChanged += CompletionModelManagerOnModelChanged;
            _session.Dismissed += SessionOnDismissed;
            Refresh();
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= SessionOnDismissed;
            _completionModelManager.ModelChanged -= CompletionModelManagerOnModelChanged;
        }

        private void CompletionModelManagerOnModelChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            Recalculate();
            Filter();
            SelectBestMatch();

            if (CompletionBuilders.Count + Completions.Count == 0)
                _session.Dismiss();
        }

        public override void Recalculate()
        {
            UpdateModel(_completionModelManager.Model);
        }

        private void UpdateModel(CompletionModel model)
        {
            ApplicableTo = ToTrackingSpan(model.ApplicableSpan);

            WritableCompletions.BeginBulkOperation();
            try
            {
                WritableCompletions.Clear();
                WritableCompletions.AddRange(ToCompletions(model.Items));
            }
            finally
            {
                WritableCompletions.EndBulkOperation();
            }
        }

        private ITrackingSpan ToTrackingSpan(TextSpan span)
        {
            var snapshot = _session.TextView.TextBuffer.CurrentSnapshot;
            return snapshot.CreateTrackingSpan(span.Start, span.Length, SpanTrackingMode.EdgeInclusive);
        }

        private IEnumerable<Microsoft.VisualStudio.Language.Intellisense.Completion> ToCompletions(IEnumerable<CompletionItem> completionItems)
        {
            return completionItems.Select(ToCompletion);
        }

        private Microsoft.VisualStudio.Language.Intellisense.Completion ToCompletion(CompletionItem completionItem)
        {
            var displayText = completionItem.DisplayText;
            var insertionText = completionItem.InsertionText;
            var description = completionItem.Description;
            var image = ToImage(completionItem.Glyph);
            return new Microsoft.VisualStudio.Language.Intellisense.Completion(displayText, insertionText, description, image, null);
        }

        private ImageSource ToImage(NQueryGlyph? glyp)
        {
            return glyp == null ? null : _glyphService.GetGlyph(glyp.Value);
        }
    }
}
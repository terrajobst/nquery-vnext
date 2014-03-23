using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

using NQuery.Authoring.Completion;
using NQuery.Authoring.Wpf;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Completion
{
    internal sealed class NQueryCompletionSet : CompletionSet
    {
        private readonly ICompletionSession _session;
        private readonly ICompletionModelManager _completionModelManager;

        public NQueryCompletionSet(ICompletionSession session, ICompletionModelManager completionModelManager)
        {
            _session = session;
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

            if (Completions.Count == 0)
                _session.Dismiss();
        }

        public override void SelectBestMatch()
        {
            var builderResult = MatchCompletionList(CompletionBuilders, CompletionMatchType.MatchDisplayText, false);
            var itemResult = MatchCompletionList(Completions, CompletionMatchType.MatchDisplayText, false);

            if (builderResult == null || itemResult == null)
            {
                base.SelectBestMatch();
            }
            else
            {
                var builderWeight = GetMatchWeight(builderResult);
                var itemWeight = GetMatchWeight(itemResult);
                SelectionStatus = builderWeight >= itemWeight
                    ? builderResult.SelectionStatus
                    : itemResult.SelectionStatus;
            }
        }

        private static int GetMatchWeight(CompletionMatchResult builderResult)
        {
            return (builderResult.CharsMatchedCount) +
                   (builderResult.SelectionStatus.IsSelected ? 1 : 0) +
                   (builderResult.SelectionStatus.IsUnique ? 1 : 0);
        }

        public override void Recalculate()
        {
            UpdateModel(_completionModelManager.Model);
        }

        private void UpdateModel(CompletionModel model)
        {
            ApplicableTo = ToTrackingSpan(model.ApplicableSpan);

            var builders = model.Items.Where(item => item.IsBuilder);
            UpdateBuilders(builders);

            var completions = model.Items.Where(item1 => !item1.IsBuilder);
            UpdateCompletions(completions);
        }

        private void UpdateBuilders(IEnumerable<CompletionItem> items)
        {
            WritableCompletionBuilders.BeginBulkOperation();
            try
            {
                WritableCompletionBuilders.Clear();
                WritableCompletionBuilders.AddRange(ToCompletions(items));
            }
            finally
            {
                WritableCompletionBuilders.EndBulkOperation();
            }
        }

        private void UpdateCompletions(IEnumerable<CompletionItem> items)
        {
            WritableCompletions.BeginBulkOperation();
            try
            {
                WritableCompletions.Clear();
                WritableCompletions.AddRange(ToCompletions(items));
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

        private static IEnumerable<Microsoft.VisualStudio.Language.Intellisense.Completion> ToCompletions(IEnumerable<CompletionItem> completionItems)
        {
            return completionItems.Select(ToCompletion);
        }

        private static Microsoft.VisualStudio.Language.Intellisense.Completion ToCompletion(CompletionItem completionItem)
        {
            var displayText = completionItem.DisplayText;
            var insertionText = completionItem.InsertionText;
            var description = completionItem.Description;
            var image = ToImage(completionItem.Glyph);

            return new Microsoft.VisualStudio.Language.Intellisense.Completion(displayText, insertionText, description, image, null);
        }

        private static ImageSource ToImage(NQueryGlyph? glyp)
        {
            return glyp == null ? null : NQueryGlyphImageSource.Get(glyp.Value);
        }
    }
}
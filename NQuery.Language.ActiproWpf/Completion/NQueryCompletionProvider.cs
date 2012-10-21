using System.ComponentModel.Composition;
using ActiproSoftware.Text;
using ActiproSoftware.Text.RegularExpressions;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Language.VSEditor;
using NQuery.Language.VSEditor.Completion;

using ActiproCompletionItem = ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation.CompletionItem;
using ActiproICompletionProvider = ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.ICompletionProvider;

using NQueryCompletionItem = NQuery.Language.VSEditor.Completion.CompletionItem;

namespace NQueryViewerActiproWpf
{
    [ExportLanguageService(typeof (ICompletionProvider))]
    internal sealed class NQueryCompletionProvider : CompletionProviderBase
    {
        [Import]
        public ICompletionModelProvider CompletionModelProvider { get; set; }

        [Import]
        public INQueryGlyphService GlyphService { get; set; }

        public override bool RequestSession(IEditorView view, bool canCommitWithoutPopup)
        {
            RequestSessionAsync(view, canCommitWithoutPopup);
            return true;
        }

        private async void RequestSessionAsync(IEditorView view, bool canCommitWithoutPopup)
        {
            var snapshot = view.CurrentSnapshot;
            var semanticModel = await snapshot.Document.GetSemanticModelAsync();
            if (semanticModel == null)
                return;

            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var textBuffer = syntaxTree.TextBuffer;
            var offset = view.SyntaxEditor.Caret.Offset;
            var position = new TextSnapshotOffset(snapshot, offset).ToOffset(textBuffer);

            var model = CompletionModelProvider.GetModel(semanticModel, position);

            var completionSession = new CompletionSession();
            completionSession.CanFilterUnmatchedItems = true;
            completionSession.CanCommitWithoutPopup = canCommitWithoutPopup;

            foreach (var completionItem in model.Items)
            {
                var item = GetActiproCompletionItem(completionItem);
                completionSession.Items.Add(item);
            }

            if (completionSession.Items.Count == 0)
                return;

            completionSession.Open(view);            
        }

        private ActiproCompletionItem GetActiproCompletionItem(NQueryCompletionItem completionItem)
        {
            var imageSourceProvider = completionItem.Glyph == null
                                          ? null
                                          : new NQueryGlyphImageProvider(GlyphService, completionItem.Glyph.Value);

            return new ActiproCompletionItem(completionItem.InsertionText, imageSourceProvider, null);
        }
    }
}
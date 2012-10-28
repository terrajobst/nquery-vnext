using System;
using System.ComponentModel.Composition;
using System.Linq;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Authoring.ActiproWpf.SymbolContent;
using NQuery.Authoring.Completion;

using CompletionItem = ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation.CompletionItem;

namespace NQuery.Authoring.ActiproWpf.Completion
{
    [ExportLanguageService(typeof (ICompletionProvider))]
    internal sealed class NQueryCompletionProvider : CompletionProviderBase
    {
        [Import]
        public ICompletionModelProvider CompletionModelProvider { get; set; }

        [Import]
        public ISymbolContentProvider SymbolContentProvider { get; set; }

        public override bool RequestSession(IEditorView view, bool canCommitWithoutPopup)
        {
            RequestSessionAsync(view, canCommitWithoutPopup);
            return true;
        }

        private async void RequestSessionAsync(IEditorView view, bool canCommitWithoutPopup)
        {
            var semanticData = await view.CurrentSnapshot.Document.GetSemanticDataAsync();
            if (semanticData == null)
                return;

            var snapshot = semanticData.ParseData.Snapshot;
            var semanticModel = semanticData.SemanticModel;
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var textBuffer = syntaxTree.TextBuffer;
            var offset = view.SyntaxEditor.Caret.Offset;
            var position = new TextSnapshotOffset(snapshot, offset).ToOffset(textBuffer);

            var model = CompletionModelProvider.GetModel(semanticModel, position);

            var existingSession = view.SyntaxEditor.IntelliPrompt.Sessions.OfType<CompletionSession>().FirstOrDefault();
            var completionSession = existingSession ?? new NQueryCompletionSession
                                                           {
                                                               CanFilterUnmatchedItems = true,
                                                               CanCommitWithoutPopup = canCommitWithoutPopup
                                                           };

            completionSession.Items.Clear();

            foreach (var completionItem in model.Items)
            {
                var item = GetActiproCompletionItem(completionItem);
                completionSession.Items.Add(item);
            }

            if (completionSession.Items.Count == 0)
            {
                completionSession.Cancel();
                return;
            }

            if (existingSession == null)
                completionSession.Open(view);            
        }

        private CompletionItem GetActiproCompletionItem(Authoring.Completion.CompletionItem completionItem)
        {
            var imageSourceProvider = completionItem.Glyph == null
                                          ? null
                                          : SymbolContentProvider.GetImageSourceProvider(completionItem.Glyph.Value);

            var contentProvider = completionItem.Symbol == null
                                      ? null
                                      : SymbolContentProvider.GetContentProvider(completionItem.Symbol);

            return new CompletionItem(completionItem.InsertionText, imageSourceProvider, contentProvider);
        }
    }
}
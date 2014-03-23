using System;
using System.Collections.ObjectModel;
using System.Linq;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Utility;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Authoring.ActiproWpf.SymbolContent;
using NQuery.Authoring.Completion;

using CompletionItem = ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation.CompletionItem;
using NQueryICompletionProvider = NQuery.Authoring.Completion.ICompletionProvider;

namespace NQuery.Authoring.ActiproWpf.Completion
{
    internal sealed class NQueryCompletionProvider : CompletionProviderBase, INQueryCompletionProvider
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly Collection<NQueryICompletionProvider> _providers = new Collection<NQueryICompletionProvider>();

        public NQueryCompletionProvider(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public Collection<NQueryICompletionProvider> Providers
        {
            get { return _providers; }
        }

        private INQuerySymbolContentProvider SymbolContentProvider
        {
            get { return _serviceLocator.GetService<INQuerySymbolContentProvider>(); }
        }

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

            var snapshot = semanticData.SemanticModel.Compilation.SyntaxTree.GetTextSnapshot();
            var semanticModel = semanticData.SemanticModel;
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var textBuffer = syntaxTree.TextBuffer;
            var offset = view.SyntaxEditor.Caret.Offset;
            var position = new TextSnapshotOffset(snapshot, offset).ToOffset(textBuffer);

            var model = semanticModel.GetCompletionModel(position, _providers);

            var existingSession = view.SyntaxEditor.IntelliPrompt.Sessions.OfType<CompletionSession>().FirstOrDefault();
            var completionSession = existingSession ?? new NQueryCompletionSession
                                                           {
                                                               MatchOptions = CompletionMatchOptions.TargetsDisplayText |
                                                                              CompletionMatchOptions.IsCaseInsensitive |
                                                                              CompletionMatchOptions.UseAcronyms,
                                                               CanFilterUnmatchedItems = true,
                                                               CanCommitWithoutPopup = canCommitWithoutPopup
                                                           };

            completionSession.Items.Clear();

            var hasBuilders = model.Items.Any(i => i.IsBuilder);
            if (!hasBuilders)
            {
                foreach (var item in model.Items.Select(GetActiproCompletionItem))
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

            return new CompletionItem(completionItem.DisplayText, imageSourceProvider, contentProvider, completionItem.InsertionText, null);
        }
    }
}
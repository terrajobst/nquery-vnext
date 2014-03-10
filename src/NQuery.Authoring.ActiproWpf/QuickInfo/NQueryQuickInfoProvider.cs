using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Utility;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Authoring.ActiproWpf.SymbolContent;
using NQuery.Authoring.QuickInfo;

namespace NQuery.Authoring.ActiproWpf.QuickInfo
{
    internal sealed class NQueryQuickInfoProvider : QuickInfoProviderBase, INQueryQuickInfoProvider
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly Collection<IQuickInfoModelProvider> _providers = new Collection<IQuickInfoModelProvider>();

        public NQueryQuickInfoProvider(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        private INQuerySymbolContentProvider SymbolContentProvider
        {
            get { return _serviceLocator.GetService<INQuerySymbolContentProvider>(); }
        }

        public Collection<IQuickInfoModelProvider> Providers
        {
            get { return _providers; }
        }

        public override object GetContext(IEditorView view, int offset)
        {
            var snapshot = view.CurrentSnapshot;
            var semanticData = snapshot.GetSemanticData();
            if (semanticData == null)
                return null;

            var syntaxTree = semanticData.SemanticModel.Compilation.SyntaxTree;
            var textBuffer = syntaxTree.TextBuffer;
            var position = new TextSnapshotOffset(snapshot, offset).ToOffset(textBuffer);

            var model = semanticData.SemanticModel.GetQuickInfoModel(position, _providers);
            return model;
        }

        protected override bool RequestSession(IEditorView view, object context)
        {
            var model = context as QuickInfoModel;
            if (model == null)
                return false;

            var textBuffer = model.SemanticModel.Compilation.SyntaxTree.TextBuffer;
            var textSnapshotRange = textBuffer.ToSnapshotRange(view.CurrentSnapshot, model.Span);
            var textRange = textSnapshotRange.TextRange;
            var content = SymbolContentProvider.GetContentProvider(model.Glyph, model.Markup).GetContent();

            var quickInfoSession = new QuickInfoSession();
            quickInfoSession.Context = context;
            quickInfoSession.Content = content;
            quickInfoSession.Open(view, textRange);
            return true;
        }

        protected override IEnumerable<Type> ContextTypes
        {
            get { return new[] {typeof (QuickInfoModel)}; }
        }
    }
}
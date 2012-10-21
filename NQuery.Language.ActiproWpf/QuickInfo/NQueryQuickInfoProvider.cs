using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;
using NQuery.Language.VSEditor;

namespace NQueryViewerActiproWpf
{
    [ExportLanguageService(typeof(IQuickInfoProvider))]
    internal sealed class NQueryQuickInfoProvider : QuickInfoProviderBase
    {
        [Import]
        public ISymbolContentProvider SymbolContentProvider { get; set; }

        [ImportMany]
        public IEnumerable<IQuickInfoModelProvider> QuickInfoModelProviders { get; set; }

        public override object GetContext(IEditorView view, int offset)
        {
            var snapshot = view.CurrentSnapshot;
            var semanticData = snapshot.GetSemanticData();
            if (semanticData == null)
                return null;

            var syntaxTree = semanticData.ParseData.SyntaxTree;
            var textBuffer = syntaxTree.TextBuffer;
            var position = new TextSnapshotOffset(snapshot, offset).ToOffset(textBuffer);

            var model = QuickInfoModelProviders
                .Select(p => p.GetModel(semanticData.SemanticModel, position))
                .FirstOrDefault(m => m != null);
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
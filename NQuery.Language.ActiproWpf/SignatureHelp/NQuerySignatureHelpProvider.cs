using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;
using NQuery.Language.VSEditor.Completion;
using NQuery.Language.VSEditor.SignatureHelp;

using ActiproSignatureItem = ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation.SignatureItem;

namespace NQueryViewerActiproWpf.SignatureHelp
{
    [ExportLanguageService(typeof(IParameterInfoProvider))]
    internal sealed class NQuerySignatureHelpProvider : ParameterInfoProviderBase
    {
        [ImportMany]
        public IEnumerable<ISignatureModelProvider> SignatureModelProviders { get; set; }

        public override bool RequestSession(IEditorView view)
        {
            var snapshot = view.CurrentSnapshot;
            var semanticData = snapshot.GetSemanticData();
            if (semanticData == null)
                return false;

            var syntaxTree = semanticData.ParseData.SyntaxTree;
            var textBuffer = syntaxTree.TextBuffer;
            var offset = view.SyntaxEditor.Caret.Offset;
            var position = new TextSnapshotOffset(snapshot, offset).ToOffset(textBuffer);

            var model = SignatureModelProviders
                .Select(p => p.GetModel(semanticData.SemanticModel, position))
                .FirstOrDefault(m => m != null);

            if (model == null || model.Signatures.Count == 0)
                return false;

            var session = new ParameterInfoSession();

            foreach (var signatureItem in model.Signatures)
            {
                var signatureContentProvider = new NQuerySignatureContentProvider(signatureItem);
                var item = new ActiproSignatureItem(signatureContentProvider, signatureItem);
                session.Items.Add(item);
            }

            var span = textBuffer.ToSnapshotRange(view.CurrentSnapshot, model.ApplicableSpan);
            session.Open(view, span);

            return true;
        }
    }
}
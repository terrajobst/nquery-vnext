using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;
using NQuery.Language.VSEditor.SignatureHelp;

using ActiproSignatureItem = ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation.SignatureItem;
using NQuerySignatureItem = NQuery.Language.VSEditor.SignatureHelp.SignatureItem;

namespace NQueryViewerActiproWpf.SignatureHelp
{
    [ExportLanguageService(typeof(IParameterInfoProvider))]
    internal sealed class NQuerySignatureHelpProvider : ParameterInfoProviderBase
    {
        [ImportMany]
        public IEnumerable<ISignatureModelProvider> SignatureModelProviders { get; set; }

        public override bool RequestSession(IEditorView view)
        {
            RequestSessionAsync(view);
            return true;
        }

        private async void RequestSessionAsync(IEditorView view)
        {
            var semanticData = await view.CurrentSnapshot.Document.GetSemanticDataAsync();
            if (semanticData == null)
                return;

            var parseData = semanticData.ParseData;
            var snapshot = parseData.Snapshot;
            var syntaxTree = parseData.SyntaxTree;
            var textBuffer = syntaxTree.TextBuffer;
            var offset = view.SyntaxEditor.Caret.Offset;
            var position = new TextSnapshotOffset(snapshot, offset).ToOffset(textBuffer);

            var model = SignatureModelProviders
                .Select(p => p.GetModel(semanticData.SemanticModel, position))
                .FirstOrDefault(m => m != null);

            var existingSession = view.SyntaxEditor.IntelliPrompt.Sessions.OfType<ParameterInfoSession>().FirstOrDefault();

            if (model == null || model.Signatures.Count == 0)
            {
                if (existingSession != null)
                    existingSession.Close(true);
                return;
            }

            var session = existingSession ?? new ParameterInfoSession();

            var previousSelectedItem = existingSession == null
                                           ? -1
                                           : existingSession.Items.IndexOf(existingSession.Selection);

            session.Items.Clear();

            var parameterIndex = model.SelectedParameter;

            foreach (var signatureItem in model.Signatures)
            {
                var signatureContentProvider = new NQuerySignatureContentProvider(signatureItem, parameterIndex);
                var item = new ActiproSignatureItem(signatureContentProvider, signatureItem);
                session.Items.Add(item);
            }

            var selectedSignature = previousSelectedItem >= 0 && previousSelectedItem < model.Signatures.Count
                                        ? model.Signatures[previousSelectedItem]
                                        : null;

            var bestSignature = selectedSignature == null || parameterIndex >= selectedSignature.Parameters.Count
                                    ? model.Signatures.FirstOrDefault(s => parameterIndex < s.Parameters.Count)
                                    : selectedSignature;

            session.Selection = session.Items.FirstOrDefault(i => i.Tag == bestSignature);

            if (existingSession == null)
            {
                var span = textBuffer.ToSnapshotRange(view.CurrentSnapshot, model.ApplicableSpan);
                session.Open(view, span);
            }
        }
    }
}
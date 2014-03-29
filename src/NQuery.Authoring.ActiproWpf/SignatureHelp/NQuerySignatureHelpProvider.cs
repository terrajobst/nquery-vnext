using System;
using System.Collections.ObjectModel;
using System.Linq;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Authoring.SignatureHelp;

using SignatureItem = ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation.SignatureItem;

namespace NQuery.Authoring.ActiproWpf.SignatureHelp
{
    internal sealed class NQuerySignatureHelpProvider : ParameterInfoProviderBase, INQuerySignatureHelpProvider
    {
        private readonly Collection<ISignatureHelpModelProvider> _providers = new Collection<ISignatureHelpModelProvider>();

        public Collection<ISignatureHelpModelProvider> Providers
        {
            get { return _providers; }
        }

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

            var syntaxTree = semanticData.SemanticModel.Compilation.SyntaxTree;
            var snapshot = syntaxTree.GetTextSnapshot();
            var textBuffer = syntaxTree.TextBuffer;
            var offset = view.SyntaxEditor.Caret.Offset;
            var position = new TextSnapshotOffset(snapshot, offset).ToOffset(textBuffer);

            var model = semanticData.SemanticModel.GetSignatureHelpModel(position, _providers);

            var existingSession = view.SyntaxEditor.IntelliPrompt.Sessions.OfType<ParameterInfoSession>().FirstOrDefault();

            if (model == null || model.Signatures.Length == 0)
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
                var item = new SignatureItem(signatureContentProvider, signatureItem);
                session.Items.Add(item);
            }

            var selectedSignature = previousSelectedItem >= 0 && previousSelectedItem < model.Signatures.Length
                                        ? model.Signatures[previousSelectedItem]
                                        : null;

            var bestSignature = selectedSignature == null || parameterIndex >= selectedSignature.Parameters.Length
                                    ? model.Signatures.FirstOrDefault(s => parameterIndex < s.Parameters.Length)
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
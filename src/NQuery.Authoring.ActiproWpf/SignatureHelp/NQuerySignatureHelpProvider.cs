using System.Collections.ObjectModel;

using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Authoring.SignatureHelp;

using SignatureItem = ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation.SignatureItem;

namespace NQuery.Authoring.ActiproWpf.SignatureHelp
{
    internal sealed class NQuerySignatureHelpProvider : ParameterInfoProviderBase, INQuerySignatureHelpProvider
    {
        public Collection<ISignatureHelpModelProvider> Providers { get; } = new Collection<ISignatureHelpModelProvider>();

        public override bool RequestSession(IEditorView view)
        {
            RequestSessionAsync(view);
            return true;
        }

        private async void RequestSessionAsync(IEditorView view)
        {
            var snapshot = view.SyntaxEditor.GetDocumentView();
            var semanticModel = await snapshot.Document.GetSemanticModelAsync();
            var text = snapshot.Text;
            var position = snapshot.Position;

            var model = semanticModel.GetSignatureHelpModel(position, Providers);

            var existingSession = view.SyntaxEditor.IntelliPrompt.Sessions.OfType<ParameterInfoSession>().FirstOrDefault();

            if (model is null || model.Signatures.Length == 0)
            {
                existingSession?.Close(true);
                return;
            }

            var session = existingSession ?? new ParameterInfoSession();

            var previousSelectedItem = existingSession is null
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

            var bestSignature = selectedSignature is null || parameterIndex >= selectedSignature.Parameters.Length
                                    ? model.Signatures.FirstOrDefault(s => parameterIndex < s.Parameters.Length)
                                    : selectedSignature;

            session.Selection = session.Items.FirstOrDefault(i => i.Tag == bestSignature);

            if (existingSession is null)
            {
                var span = text.ToSnapshotRange(model.ApplicableSpan);
                session.Open(view, span);
            }
        }
    }
}
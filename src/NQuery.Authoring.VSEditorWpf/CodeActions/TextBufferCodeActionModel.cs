using System;
using System.Security.Cryptography.X509Certificates;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Wpf.CodeActions;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    internal sealed class TextBufferCodeActionModel : CodeActionModel
    {
        private readonly ISyntaxTreeApplier _syntaxTreeApplier;

        public TextBufferCodeActionModel(CodeActionKind kind, ICodeAction codeAction, ISyntaxTreeApplier syntaxTreeApplier)
            : base(kind, codeAction)
        {
            _syntaxTreeApplier = syntaxTreeApplier;
        }

        protected override void Invoke(ICodeAction action)
        {
            _syntaxTreeApplier.Apply(action.GetEdit(), action.Description);
        }
    }
}
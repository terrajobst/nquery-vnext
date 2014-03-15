using System;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Wpf.CodeActions
{
    public abstract class CodeActionModel
    {
        private readonly CodeActionKind _kind;
        private readonly ICodeAction _codeAction;

        protected CodeActionModel(CodeActionKind kind, ICodeAction codeAction)
        {
            _kind = kind;
            _codeAction = codeAction;
        }

        public CodeActionKind Kind
        {
            get { return _kind; }
        }

        public string Description
        {
            get { return _codeAction.Description; }
        }

        public void Invoke()
        {
            Invoke(_codeAction);
        }

        protected abstract void Invoke(ICodeAction action);
    }
}
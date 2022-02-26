using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Wpf.CodeActions
{
    public abstract class CodeActionModel
    {
        private readonly ICodeAction _codeAction;

        protected CodeActionModel(CodeActionKind kind, ICodeAction codeAction)
        {
            Kind = kind;
            _codeAction = codeAction;
        }

        public CodeActionKind Kind { get; }

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
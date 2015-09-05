using System;

using NQuery.Text;

namespace NQuery.Authoring.CodeActions
{
    public abstract class CodeAction : ICodeAction
    {
        private readonly SyntaxTree _syntaxTree;

        protected CodeAction(SyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;
        }

        public abstract string Description { get; }

        public virtual SyntaxTree GetEdit()
        {
            var changeSet = new TextChangeSet();
            GetChanges(changeSet);

            return _syntaxTree.WithChanges(changeSet);
        }

        protected virtual void GetChanges(TextChangeSet changeSet)
        {
        }
    }
}
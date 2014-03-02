using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.VSEditorWpf.CodeActions
{
    internal sealed class CodeActionSet
    {
        private readonly int _position;
        private readonly ReadOnlyCollection<ICodeAction> _codeActions;

        public CodeActionSet(int position, IEnumerable<ICodeAction> codeActions)
        {
            _position = position;
            _codeActions = new ReadOnlyCollection<ICodeAction>(codeActions.ToArray());
        }

        public int Position
        {
            get { return _position; }
        }

        public ReadOnlyCollection<ICodeAction> CodeActions
        {
            get { return _codeActions; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQuerySyntaxErrorTagger : NQueryErrorTagger
    {
        private readonly INQuerySyntaxTreeManager _syntaxTreeManager;

        public NQuerySyntaxErrorTagger(ITextBuffer textBuffer, INQuerySyntaxTreeManager syntaxTreeManager)
            : base(textBuffer, PredefinedErrorTypeNames.SyntaxError)
        {
            _syntaxTreeManager = syntaxTreeManager;
            _syntaxTreeManager.SyntaxTreeChanged += SyntaxTreeManagerOnSyntaxTreeChanged;
        }

        private void SyntaxTreeManagerOnSyntaxTreeChanged(object sender, EventArgs eventArgs)
        {
            InvalidateTags();
        }

        protected override IEnumerable<Diagnostic> GetDiagnostics()
        {
            var syntaxTree = _syntaxTreeManager.SyntaxTree;
            return syntaxTree == null
                       ? Enumerable.Empty<Diagnostic>()
                       : syntaxTree.GetDiagnostics();
        }
    }
}
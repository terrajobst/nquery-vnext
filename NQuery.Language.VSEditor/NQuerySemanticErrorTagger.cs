using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQuerySemanticErrorTagger : NQueryErrorTagger
    {
        private readonly INQuerySemanticModelManager _semanticModelManager;

        public NQuerySemanticErrorTagger(ITextBuffer textBuffer, INQuerySemanticModelManager semanticModelManager)
            : base(textBuffer, PredefinedErrorTypeNames.CompilerError)
        {
            _semanticModelManager = semanticModelManager;
            _semanticModelManager.SemanticModelChanged += SemanticModelManagerOnSemanticModelChanged;
        }

        private void SemanticModelManagerOnSemanticModelChanged(object sender, EventArgs eventArgs)
        {
            InvalidateTags();
        }

        protected override IEnumerable<Diagnostic> GetDiagnostics()
        {
            var semanticModel = _semanticModelManager.SemanticModel;
            return semanticModel == null
                       ? Enumerable.Empty<Diagnostic>()
                       : semanticModel.GetDiagnostics();
        }
    }
}
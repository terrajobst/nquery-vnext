using System;

namespace NQuery.Authoring.ActiproWpf
{
    public sealed class NQuerySemanticData
    {
        private readonly SemanticModel _semanticModel;

        public NQuerySemanticData(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public SemanticModel SemanticModel
        {
            get { return _semanticModel; }
        }
    }
}
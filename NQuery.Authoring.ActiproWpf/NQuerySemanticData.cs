using System;

namespace NQuery.Language.ActiproWpf
{
    public sealed class NQuerySemanticData
    {
        private readonly NQueryParseData _parseData;
        private readonly SemanticModel _semanticModel;

        public NQuerySemanticData(NQueryParseData parseData, SemanticModel semanticModel)
        {
            _parseData = parseData;
            _semanticModel = semanticModel;
        }

        public NQueryParseData ParseData
        {
            get { return _parseData; }
        }

        public SemanticModel SemanticModel
        {
            get { return _semanticModel; }
        }
    }
}
using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;

namespace NQuery.Authoring.VSEditorWpf.Classification
{
    [Export(typeof(INQueryClassificationService))]
    internal sealed class NQueryClassificationService : INQueryClassificationService
    {
        private IClassificationType _punctuation;
        private IClassificationType _schemaTable;
        private IClassificationType _derivedTable;
        private IClassificationType _cteTable;
        private IClassificationType _column;
        private IClassificationType _method;
        private IClassificationType _property;
        private IClassificationType _function;
        private IClassificationType _aggregate;
        private IClassificationType _variable;

        [Import]
        public IClassificationTypeRegistryService ClassificationTypeRegistryService { get; set; }

        [Import]
        public IStandardClassificationService StandardClassificationService { get; set; }

        private IClassificationType GetOrRetreiveClassification(ref IClassificationType target, string name)
        {
            return target ?? (target = ClassificationTypeRegistryService.GetClassificationType(name));
        }

        public IClassificationType WhiteSpace
        {
            get { return StandardClassificationService.WhiteSpace; }
        }

        public IClassificationType Comment
        {
            get { return StandardClassificationService.Comment; }
        }

        public IClassificationType Identifier
        {
            get { return StandardClassificationService.Identifier; }
        }

        public IClassificationType Keyword
        {
            get { return StandardClassificationService.Keyword; }
        }

        public IClassificationType Punctuation
        {
            get { return GetOrRetreiveClassification(ref _punctuation, NQuerySemanticClassificationMetadata.PunctuationClassificationTypeName); }
        }

        public IClassificationType NumberLiteral
        {
            get { return StandardClassificationService.NumberLiteral; }
        }

        public IClassificationType StringLiteral
        {
            get { return StandardClassificationService.StringLiteral; }
        }

        public IClassificationType SchemaTable
        {
            get { return GetOrRetreiveClassification(ref _schemaTable, NQuerySemanticClassificationMetadata.SchemaTableClassificationTypeName); }
        }

        public IClassificationType DerivedTable
        {
            get { return GetOrRetreiveClassification(ref _derivedTable, NQuerySemanticClassificationMetadata.DerivedTableClassificationTypeName); }
        }

        public IClassificationType CommonTableExpression
        {
            get { return GetOrRetreiveClassification(ref _cteTable, NQuerySemanticClassificationMetadata.CommonTableExpressionClassificationTypeName); }
        }

        public IClassificationType Column
        {
            get { return GetOrRetreiveClassification(ref _column, NQuerySemanticClassificationMetadata.ColumnClassificationTypeName); }
        }

        public IClassificationType Method
        {
            get { return GetOrRetreiveClassification(ref _method, NQuerySemanticClassificationMetadata.MethodClassificationTypeName); }
        }

        public IClassificationType Property
        {
            get { return GetOrRetreiveClassification(ref _property, NQuerySemanticClassificationMetadata.PropertyClassificationTypeName); }
        }

        public IClassificationType Function
        {
            get { return GetOrRetreiveClassification(ref _function, NQuerySemanticClassificationMetadata.FunctionClassificationTypeName); }
        }

        public IClassificationType Aggregate
        {
            get { return GetOrRetreiveClassification(ref _aggregate, NQuerySemanticClassificationMetadata.AggregateClassificationTypeName); }
        }

        public IClassificationType Variable
        {
            get { return GetOrRetreiveClassification(ref _variable, NQuerySemanticClassificationMetadata.VariableClassificationTypeName); }
        }

        public IClassificationType Unnecessary
        {
            get { return GetOrRetreiveClassification(ref _variable, NQuerySemanticClassificationMetadata.UnnecessaryClassificationTypeName); }
        }
    }
}
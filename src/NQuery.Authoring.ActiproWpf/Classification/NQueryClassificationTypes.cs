using System;
using System.ComponentModel.Composition;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;

namespace NQuery.Authoring.ActiproWpf.Classification
{
    [Export(typeof(INQueryClassificationTypes))]
    [ExportLanguageService(typeof(INQueryClassificationTypes))]
    internal sealed class NQueryClassificationTypes : INQueryClassificationTypes
    {
        private readonly IClassificationType _whiteSpace = new ClassificationType("whiteSpace");
        private readonly IClassificationType _comment = new ClassificationType("comment");
        private readonly IClassificationType _keyword = new ClassificationType("keyword");
        private readonly IClassificationType _punctuation = new ClassificationType("punctuation");
        private readonly IClassificationType _identifier = new ClassificationType("identifier");
        private readonly IClassificationType _stringLiteral = new ClassificationType("stringLiteral");
        private readonly IClassificationType _numberLiteral = new ClassificationType("numberLiteral");
        private readonly IClassificationType _schemaTable = new ClassificationType("schemaTable");
        private readonly IClassificationType _derivedTable = new ClassificationType("derivedTable");
        private readonly IClassificationType _commonTableExpression = new ClassificationType("commonTableExpression");
        private readonly IClassificationType _column = new ClassificationType("column");
        private readonly IClassificationType _method = new ClassificationType("method");
        private readonly IClassificationType _property = new ClassificationType("property");
        private readonly IClassificationType _function = new ClassificationType("function");
        private readonly IClassificationType _aggregate = new ClassificationType("aggregate");
        private readonly IClassificationType _operator = new ClassificationType("operator");
        private readonly IClassificationType _variable = new ClassificationType("variable");

        public IClassificationType WhiteSpace
        {
            get { return _whiteSpace; }
        }

        public IClassificationType Comment
        {
            get { return _comment; }
        }

        public IClassificationType Keyword
        {
            get { return _keyword; }
        }

        public IClassificationType Punctuation
        {
            get { return _punctuation; }
        }

        public IClassificationType Identifier
        {
            get { return _identifier; }
        }

        public IClassificationType StringLiteral
        {
            get { return _stringLiteral; }
        }

        public IClassificationType NumberLiteral
        {
            get { return _numberLiteral; }
        }

        public IClassificationType SchemaTable
        {
            get { return _schemaTable; }
        }

        public IClassificationType DerivedTable
        {
            get { return _derivedTable; }
        }

        public IClassificationType CommonTableExpression
        {
            get { return _commonTableExpression; }
        }

        public IClassificationType Column
        {
            get { return _column; }
        }

        public IClassificationType Method
        {
            get { return _method; }
        }

        public IClassificationType Property
        {
            get { return _property; }
        }

        public IClassificationType Function
        {
            get { return _function; }
        }

        public IClassificationType Aggregate
        {
            get { return _aggregate; }
        }

        public IClassificationType Operator
        {
            get { return _operator; }
        }

        public IClassificationType Variable
        {
            get { return _variable; }
        }
    }
}
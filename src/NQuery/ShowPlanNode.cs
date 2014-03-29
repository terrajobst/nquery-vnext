using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery
{
    public sealed class ShowPlanNode
    {
        private readonly bool _isScalar;
        private readonly string _operatorName;
        private readonly ImmutableArray<KeyValuePair<string, string>> _properties;
        private readonly ImmutableArray<ShowPlanNode> _children;

        internal ShowPlanNode(string operatorName, IEnumerable<KeyValuePair<string,string>> properties, IEnumerable<ShowPlanNode> children, bool isScalar = false)
        {
            _isScalar = isScalar;
            _operatorName = operatorName;
            _properties = properties.ToImmutableArray();
            _children = children.ToImmutableArray();
        }

        public bool IsScalar
        {
            get { return _isScalar; }
        }

        public string OperatorName
        {
            get { return _operatorName; }
        }

        public ImmutableArray<KeyValuePair<string, string>> Properties
        {
            get { return _properties; }
        }

        public ImmutableArray<ShowPlanNode> Children
        {
            get { return _children; }
        }
    }
}
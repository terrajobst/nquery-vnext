#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery
{
    public sealed class ShowPlanNode
    {
        internal ShowPlanNode(string operatorName, IEnumerable<KeyValuePair<string,string>> properties, IEnumerable<ShowPlanNode> children, bool isScalar = false)
        {
            IsScalar = isScalar;
            OperatorName = operatorName;
            Properties = properties.ToImmutableArray();
            Children = children.ToImmutableArray();
        }

        public bool IsScalar { get; }

        public string OperatorName { get; }

        public ImmutableArray<KeyValuePair<string, string>> Properties { get; }

        public ImmutableArray<ShowPlanNode> Children { get; }
    }
}
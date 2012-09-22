using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery
{
    public sealed class ShowPlanNode
    {
        private readonly string _operatorName;
        private readonly ReadOnlyCollection<KeyValuePair<string, string>> _properties;
        private readonly ReadOnlyCollection<ShowPlanNode> _children;

        internal ShowPlanNode(string operatorName, IEnumerable<KeyValuePair<string,string>> properties, IEnumerable<ShowPlanNode> children)
        {
            _operatorName = operatorName;
            _properties = new ReadOnlyCollection<KeyValuePair<string, string>>(properties.ToArray());
            _children = new ReadOnlyCollection<ShowPlanNode>(children.ToArray());
        }

        public string OperatorName
        {
            get { return _operatorName; }
        }

        public ReadOnlyCollection<KeyValuePair<string, string>> Properties
        {
            get { return _properties; }
        }

        public ReadOnlyCollection<ShowPlanNode> Children
        {
            get { return _children; }
        }
    }
}
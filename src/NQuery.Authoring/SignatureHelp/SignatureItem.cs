using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Authoring.SignatureHelp
{
    public sealed class SignatureItem
    {
        private readonly string _content;
        private readonly string _documentation;
        private readonly ReadOnlyCollection<ParameterItem> _parameters;

        public SignatureItem(string content, string documentation, IList<ParameterItem> parameters)
        {
            _content = content;
            _documentation = documentation;
            _parameters = new ReadOnlyCollection<ParameterItem>(parameters);
        }

        public string Content
        {
            get { return _content; }
        }

        public string Documentation
        {
            get { return _documentation; }
        }

        public ReadOnlyCollection<ParameterItem> Parameters
        {
            get { return _parameters; }
        }
    }
}
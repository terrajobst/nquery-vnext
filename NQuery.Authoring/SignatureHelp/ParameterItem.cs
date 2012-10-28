using System;

namespace NQuery.Language.Services.SignatureHelp
{
    public sealed class ParameterItem
    {
        private readonly string _name;
        private readonly string _documentation;
        private readonly TextSpan _span;

        public ParameterItem(string name, string documentation, TextSpan span)
        {
            _name = name;
            _documentation = documentation;
            _span = span;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Documentation
        {
            get { return _documentation; }
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public override string ToString()
        {
            return string.Format("{0} '{1}'", _span, _name);
        }
    }
}
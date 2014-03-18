using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Authoring.SignatureHelp
{
    public sealed class SignatureItem : IEquatable<SignatureItem>
    {
        private readonly string _content;
        private readonly ReadOnlyCollection<ParameterItem> _parameters;

        public SignatureItem(string content, IList<ParameterItem> parameters)
        {
            _content = content;
            _parameters = new ReadOnlyCollection<ParameterItem>(parameters);
        }

        public string Content
        {
            get { return _content; }
        }

        public ReadOnlyCollection<ParameterItem> Parameters
        {
            get { return _parameters; }
        }

        public bool Equals(SignatureItem other)
        {
            return other != null &&
                   _content == other.Content &&
                   _parameters.SequenceEqual(other.Parameters);
        }

        public override bool Equals(object obj)
        {
            var other = obj as SignatureItem;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _content.GetHashCode();
                hashCode = (hashCode*397) ^ _parameters.GetHashCode();
                return hashCode;
            }
        }
    }
}
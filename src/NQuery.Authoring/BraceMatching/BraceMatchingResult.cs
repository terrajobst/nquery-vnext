using System;

using NQuery.Text;

namespace NQuery.Authoring.BraceMatching
{
    public struct BraceMatchingResult
    {
        public static readonly BraceMatchingResult None =  new BraceMatchingResult();

        private readonly bool _isValid;
        private readonly TextSpan _left;
        private readonly TextSpan _right;

        public BraceMatchingResult(TextSpan left, TextSpan right)
        {
            _isValid = true;
            _left = left;
            _right = right;
        }

        public bool IsValid
        {
            get { return _isValid; }
        }

        public TextSpan Left
        {
            get { return _left; }
        }

        public TextSpan Right
        {
            get { return _right; }
        }
    }
}
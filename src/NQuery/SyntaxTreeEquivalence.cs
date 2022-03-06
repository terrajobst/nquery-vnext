namespace NQuery
{
    internal static class SyntaxTreeEquivalence
    {
        public static bool AreEquivalent(SyntaxNode left, SyntaxNode right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            if (left.Kind != right.Kind)
                return false;

            if (left.IsMissing != right.IsMissing)
                return false;

            using var leftChildren = left.ChildNodesAndTokens().GetEnumerator();
            using var rightChildren = right.ChildNodesAndTokens().GetEnumerator();

            while (true)
            {
                var leftHasCurrent = leftChildren.MoveNext();
                var rightHasCurrent = rightChildren.MoveNext();
                var reachedEnd = !leftHasCurrent && !rightHasCurrent;

                if (reachedEnd)
                    break;

                if (leftHasCurrent != rightHasCurrent)
                    return false;

                if (!AreEquivalent(leftChildren.Current, rightChildren.Current))
                    return false;
            }

            return true;
        }

        public static bool AreEquivalent(SyntaxNodeOrToken left, SyntaxNodeOrToken right)
        {
            if (left.Kind != right.Kind)
                return false;

            return left.IsNode
                       ? AreEquivalent(left.AsNode(), right.AsNode())
                       : AreEquivalent(left.AsToken(), right.AsToken());
        }

        public static bool AreEquivalent(SyntaxToken left, SyntaxToken right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            if (left.Kind != right.Kind)
                return false;

            if (left.IsMissing != right.IsMissing)
                return false;

            if (left.Kind == SyntaxKind.IdentifierToken)
            {
                // Let's say left is [test] and right is "Test"
                // In this case, we don't want to match.
                return left.Matches(right.ValueText) &&
                       right.Matches(left.ValueText);
            }

            if (left.Kind.IsLiteral())
                return Equals(left.Value, right.Value);

            return true;
        }
    }
}
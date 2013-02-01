using System;

using NQuery.Binding;

namespace NQuery.Algebra
{
    internal sealed partial class Algebrizer
    {
        private Algebrizer()
        {
        }

        public static AlgebraNode Algebrize(BoundQuery node)
        {
            var algebrizer = new Algebrizer();
            return algebrizer.AlgebrizeQuery(node);
        }
    }
}
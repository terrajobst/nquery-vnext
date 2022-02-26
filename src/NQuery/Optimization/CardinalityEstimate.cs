namespace NQuery.Optimization
{
    internal struct CardinalityEstimate
    {
        public static CardinalityEstimate Unknown = new CardinalityEstimate(null, null);
        public static CardinalityEstimate Empty = new CardinalityEstimate(0, 0);
        public static CardinalityEstimate SingleRow = new CardinalityEstimate(1, 1);

        public CardinalityEstimate(long? minimum, long? maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public long? Minimum { get; }

        public long? Maximum { get; }
    }
}
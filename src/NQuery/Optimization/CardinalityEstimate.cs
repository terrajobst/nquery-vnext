namespace NQuery.Optimization
{
    internal struct CardinalityEstimate
    {
        public static CardinalityEstimate Unknown = new(null, null);
        public static CardinalityEstimate Empty = new(0, 0);
        public static CardinalityEstimate SingleRow = new(1, 1);

        public CardinalityEstimate(long? minimum, long? maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public long? Minimum { get; }

        public long? Maximum { get; }
    }
}
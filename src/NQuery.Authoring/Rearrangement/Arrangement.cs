namespace NQuery.Authoring.Rearrangement
{
    public sealed class Arrangement
    {
        private readonly ArrangementOperation _verticalOperation;
        private readonly ArrangementOperation _horizontalOperation;

        public Arrangement(ArrangementOperation verticalOperation, ArrangementOperation horizontalOperation)
        {
            _verticalOperation = verticalOperation;
            _horizontalOperation = horizontalOperation;
        }

        public static Arrangement CreateVertical(ArrangementOperation verticalOperation)
        {
            return new Arrangement(verticalOperation, null);
        }

        public static Arrangement CreateHorizontal(ArrangementOperation horizontalOperation)
        {
            return new Arrangement(null, horizontalOperation);
        }

        public ArrangementOperation VerticalOperation
        {
            get { return _verticalOperation; }
        }

        public ArrangementOperation HorizontalOperation
        {
            get { return _horizontalOperation; }
        }
    }
}
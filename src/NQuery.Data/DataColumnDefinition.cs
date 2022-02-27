using System.Data;
using System.Data.SqlTypes;
using System.Linq.Expressions;
using System.Reflection;

using NQuery.Symbols;

namespace NQuery.Data
{
    public sealed class DataColumnDefinition : ColumnDefinition
    {
        private static readonly PropertyInfo DataRowIndexer = typeof(DataRow).GetProperty("Item", new[] { typeof(DataColumn) });
        private static readonly PropertyInfo NullableIsNull = typeof(INullable).GetProperty("IsNull");

        public DataColumnDefinition(DataColumn dataColumn)
        {
            if (dataColumn is null)
                throw new ArgumentNullException(nameof(dataColumn));

            DataColumn = dataColumn;
        }

        public DataColumn DataColumn { get; }

        public override string Name
        {
            get { return DataColumn.ColumnName; }
        }

        public override Type DataType
        {
            get { return DataColumn.DataType; }
        }

        public override Expression CreateInvocation(Expression instance)
        {
            // var v = ((DataRow)row)[_dataColumn];
            // INullable n;
            // return
            //    v is DBNull
            //      ? null
            //      : (n = v as INullable) is not null && n.IsNull
            //          ? null
            //          : v;

            var v = Expression.Variable(typeof(object));
            var n = Expression.Variable(typeof(INullable));
            var objectNull = Expression.Constant(null, typeof(object));

            return
                Expression.Block(
                    typeof(object),
                    new[] { v, n },
                    Expression.Assign(
                        v,
                        Expression.MakeIndex(
                            Expression.Convert(
                                instance,
                                typeof(DataRow)
                            ),
                            DataRowIndexer,
                            new[] { Expression.Constant(DataColumn) }
                        )
                    ),
                    Expression.Condition(
                        Expression.TypeIs(
                            v,
                            typeof(DBNull)
                        ),
                        objectNull,
                        Expression.Condition(
                            Expression.AndAlso(
                                Expression.NotEqual(
                                    Expression.Assign(
                                        n,
                                        Expression.TypeAs(
                                            v,
                                            typeof(INullable)
                                        )
                                    ),
                                    objectNull
                                ),
                                Expression.Property(
                                    n,
                                    NullableIsNull
                                )
                            ),
                            objectNull,
                            v
                        )
                    )
                );
        }
    }
}
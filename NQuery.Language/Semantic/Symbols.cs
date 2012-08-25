using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language.Semantic
{
    public enum SymbolKind
    {
        BadSymbol,
        BadTable,
        Column,
        SchemaTable,
        DerivedTable,
        TableInstance,
        ColumnInstance,
    }

    public abstract class Symbol
    {
        private readonly string _name;

        protected Symbol(string name)
        {
            _name = name;
        }

        public abstract SymbolKind Kind { get; }

        public string Name
        {
            get { return _name; }
        }

        public abstract Type Type { get; }
    }

    public sealed class BadSymbol : Symbol
    {
        private readonly Type _type;

        public BadSymbol(string name)
            : this(name, WellKnownTypes.Unknown)
        {
        }

        public BadSymbol(string name, Type type)
            : base(name)
        {
            _type = type;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.BadSymbol; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        public override string ToString()
        {
            return string.Format("!{0}: {1}", Name, Type);
        }
    }

    public sealed class ColumnSymbol : Symbol
    {
        private readonly Type _type;

        public ColumnSymbol(string name, Type type)
            : base(name)
        {
            _type = type;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Column; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        public override string ToString()
        {
            return string.Format("COLUMN {0}: {1}", Name, Type);
        }
    }

    public abstract class TableSymbol : Symbol
    {
        private readonly ReadOnlyCollection<ColumnSymbol> _columns;

        protected TableSymbol(string name, IList<ColumnSymbol> columns)
            : base(name)
        {
            _columns = new ReadOnlyCollection<ColumnSymbol>(columns);
        }

        public ReadOnlyCollection<ColumnSymbol> Columns
        {
            get { return _columns; }
        }

        public override string ToString()
        {
            return string.Format("TABLE {0}: {1}", Name, Type);
        }
    }

    public sealed class BadTableSymbol : TableSymbol
    {
        public BadTableSymbol(string name)
            : base(name, new ColumnSymbol[0])
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.BadTable; }
        }

        public override Type Type
        {
            get { return WellKnownTypes.Unknown; }
        }
    }

    public sealed class SchemaTableSymbol : TableSymbol
    {
        private readonly Type _rowType;

        public SchemaTableSymbol(string name, IList<ColumnSymbol> columns, Type rowType)
            : base(name, columns)
        {
            _rowType = rowType;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.SchemaTable; }
        }

        public override Type Type
        {
            get { return _rowType; }
        }
    }

    public sealed class DerivedTableSymbol : TableSymbol
    {
        public DerivedTableSymbol(IList<ColumnSymbol> columns)
            : base(string.Empty, columns)
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.DerivedTable; }
        }

        public override Type Type
        {
            get { return WellKnownTypes.Missing; }
        }
    }

    public sealed class TableInstanceSymbol : Symbol
    {
        private readonly TableSymbol _table;

        public TableInstanceSymbol(string name, TableSymbol table)
            : base(name)
        {
            _table = table;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.TableInstance; }
        }

        public TableSymbol Table
        {
            get { return _table; }
        }

        public override Type Type
        {
            get { return _table.Type; }
        }

        public override string ToString()
        {
            return string.Format("ROW {0} ({1})", Name, _table);
        }
    }

    public sealed class ColumnInstanceSymbol : Symbol
    {
        private readonly TableInstanceSymbol _tableInstance;
        private readonly ColumnSymbol _column;

        public ColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column)
            : base(column.Name)
        {
            _tableInstance = tableInstance;
            _column = column;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.ColumnInstance; }
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public ColumnSymbol Column
        {
            get { return _column; }
        }

        public override Type Type
        {
            get { return _column.Type; }
        }

        public override string ToString()
        {
            return string.Format("ROW COLUMN {0}.{1}: {2}", TableInstance.Table.Name, Column.Name, Column.Type);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQueryDesigner.ViewModels
{
    internal sealed class TableViewModel : ViewModel
    {
        private readonly DesignerViewModel _parent;
        private readonly string _name;
        private readonly IReadOnlyCollection<TableColumnViewModel> _columns;

        public TableViewModel(DesignerViewModel parent, string name, IEnumerable<TableColumnViewModel> columns)
        {
            _parent = parent;
            _name = name;
            _columns = new ReadOnlyCollection<TableColumnViewModel>(columns.ToArray());
        }

        public string Name
        {
            get { return _name; }
        }

        public IReadOnlyCollection<TableColumnViewModel> Columns
        {
            get { return _columns; }
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using NQuery;

using NQueryDesigner.Authoring;

namespace NQueryDesigner.ViewModels
{
    internal sealed class DesignerViewModel : ViewModel
    {
        private readonly RelayCommand _addSelectionRowCommand;
        private readonly ObservableCollection<SelectionRowViewModel> _selectionRows = new ObservableCollection<SelectionRowViewModel>();
        private readonly ObservableCollection<TableViewModel> _tables = new ObservableCollection<TableViewModel>();

        private string _text;
        private DataContext _dataContext;

        public DesignerViewModel()
        {
            _addSelectionRowCommand = new RelayCommand(AddSelectionRow);
        }

        private void AddSelectionRow()
        {
            _selectionRows.Add(new SelectionRowViewModel(this));
        }

        public ICommand AddSelectionRowCommand
        {
            get { return _addSelectionRowCommand; }
        }

        public DataContext DataContext
        {
            get { return _dataContext; }
            set
            {
                if (_dataContext != value)
                {
                    _dataContext = value;
                    OnPropertyChanged();
                    UpdateForModel();
                }
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                    UpdateFromText();
                }
            }
        }

        public ObservableCollection<SelectionRowViewModel> SelectionRows
        {
            get { return _selectionRows; }
        }

        public ObservableCollection<TableViewModel> Tables
        {
            get { return _tables; }
        }

        private void UpdateFromText()
        {
            var syntaxTree = SyntaxTree.ParseQuery(_text);
            var compilation = Compilation.Create(_dataContext, syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            _selectionRows.Clear();

            var queryInfo = semanticModel.AnalyzeQuery();
            foreach (var selectionInfo in queryInfo.SelectionInfos)
            {
                var viewModel = new SelectionRowViewModel(this);
                viewModel.Column = selectionInfo.Column;
                viewModel.Alias = selectionInfo.Alias;
                viewModel.Table = selectionInfo.Table;
                viewModel.Aggregate = selectionInfo.Aggregate;
                viewModel.Sort = selectionInfo.SortOrder == QuerySortOrder.Unsorted
                                ? string.Empty
                                : selectionInfo.SortOrder.ToString();
                _selectionRows.Add(viewModel);
            }

            _tables.Clear();

            foreach (var table in queryInfo.Tables)
            {
                var columns = table.ColumnInstances.Select(c => new TableColumnViewModel(this, c.Name));
                var viewModel = new TableViewModel(this, table.Name, columns);
                _tables.Add(viewModel);
            }
        }

        public void UpdateForModel()
        {
        }
    }
}
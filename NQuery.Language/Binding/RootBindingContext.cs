using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal sealed class RootBindingContext : BindingContext
    {
        private readonly DataContext _dataContext;

        public RootBindingContext(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public override IEnumerable<Symbol> LookupSymbols()
        {
            return _dataContext.Tables.Cast<Symbol>()
                                      .Concat(_dataContext.Functions)
                                      .Concat(_dataContext.Variables);
        }
    }
}
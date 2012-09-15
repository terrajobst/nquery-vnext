using System.Collections.Generic;
using System.Collections.ObjectModel;
using NQuery.Language.Symbols;

namespace NQuery.Language
{
    public sealed class DataContext
    {
        internal DataContext(IList<TableSymbol> tables, IList<FunctionSymbol> functions, IList<VariableSymbol> variables, TypeRegistry<IPropertyProvider> propertyProviders, TypeRegistry<IMethodProvider> methodProviders)
        {
            Tables = new ReadOnlyCollection<TableSymbol>(tables);
            Functions = new ReadOnlyCollection<FunctionSymbol>(functions);
            Variables = new ReadOnlyCollection<VariableSymbol>(variables);
            PropertyProviders = new ReadOnlyTypeRegistry<IPropertyProvider>(propertyProviders);
            MethodProviders = new ReadOnlyTypeRegistry<IMethodProvider>(methodProviders);
        }

        public static readonly DataContext Empty = new DataContext(new TableSymbol[0],
                                                                   new FunctionSymbol[0],
                                                                   new VariableSymbol[0],
                                                                   new TypeRegistry<IPropertyProvider>(),
                                                                   new TypeRegistry<IMethodProvider>());

        public ReadOnlyCollection<TableSymbol> Tables { get; private set; }
        public ReadOnlyCollection<FunctionSymbol> Functions { get; private set; }
        public ReadOnlyCollection<VariableSymbol> Variables { get; private set; }

        public ReadOnlyTypeRegistry<IPropertyProvider> PropertyProviders { get; private set; }
        public ReadOnlyTypeRegistry<IMethodProvider> MethodProviders { get; private set; }
    }
}
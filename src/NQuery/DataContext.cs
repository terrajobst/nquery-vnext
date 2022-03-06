using System.Collections;
using System.Collections.Immutable;

using NQuery.Hosting;
using NQuery.Symbols;
using NQuery.Symbols.Aggregation;

namespace NQuery
{
    public sealed class DataContext
    {
        private DataContext(IImmutableList<TableSymbol> tables,
                            IImmutableList<TableRelation> relations,
                            IImmutableList<FunctionSymbol> functions,
                            IImmutableList<AggregateSymbol> aggregates,
                            IImmutableList<VariableSymbol> variables,
                            IImmutableDictionary<Type, IPropertyProvider> propertyProviders,
                            IImmutableDictionary<Type, IMethodProvider> methodProviders,
                            IImmutableDictionary<Type, IComparer> comparers)
        {
            Tables = tables;
            Relations = relations;
            Functions = functions;
            Aggregates = aggregates;
            Variables = variables;
            PropertyProviders = propertyProviders;
            MethodProviders = methodProviders;
            Comparers = comparers;
        }

        public static readonly DataContext Empty = CreateEmpty();
        public static readonly DataContext Default = CreateDefault();

        public IImmutableList<TableSymbol> Tables { get; }

        public IImmutableList<TableRelation> Relations { get; }

        public IImmutableList<FunctionSymbol> Functions { get; }

        public IImmutableList<AggregateSymbol> Aggregates { get; }

        public IImmutableList<VariableSymbol> Variables { get; }

        public IImmutableDictionary<Type, IPropertyProvider> PropertyProviders { get; }

        public IImmutableDictionary<Type, IMethodProvider> MethodProviders { get; }

        public IImmutableDictionary<Type, IComparer> Comparers { get; }

        private static DataContext CreateEmpty()
        {
            return new DataContext(ImmutableList.Create<TableSymbol>(),
                                   ImmutableList.Create<TableRelation>(),
                                   ImmutableList.Create<FunctionSymbol>(),
                                   ImmutableList.Create<AggregateSymbol>(),
                                   ImmutableList.Create<VariableSymbol>(),
                                   ImmutableDictionary.Create<Type, IPropertyProvider>(),
                                   ImmutableDictionary.Create<Type, IMethodProvider>(),
                                   ImmutableDictionary.Create<Type, IComparer>());
        }

        private static DataContext CreateDefault()
        {
            var functions = BuiltInFunctions.GetFunctions().ToImmutableList();
            var aggregates = BuiltInAggregates.GetAggregates().ToImmutableList();
            var reflectionProvider = new ReflectionProvider();
            var propertyProviders = ImmutableDictionary.Create<Type, IPropertyProvider>()
                                                       .Add(typeof(object), reflectionProvider);
            var methodProviders = ImmutableDictionary.Create<Type, IMethodProvider>()
                                                     .Add(typeof(object), reflectionProvider);
            var comparers = ImmutableDictionary.Create<Type, IComparer>();
            return new DataContext(ImmutableList.Create<TableSymbol>(),
                                   ImmutableList.Create<TableRelation>(),
                                   functions,
                                   aggregates,
                                   ImmutableList.Create<VariableSymbol>(),
                                   propertyProviders,
                                   methodProviders,
                                   comparers);
        }

        // Tables

        public DataContext AddTables(params TableSymbol[] tables)
        {
            if (tables is null || tables.Length == 0)
                return this;

            return AddTables(tables.AsEnumerable());
        }

        public DataContext AddTables(IEnumerable<TableSymbol> tables)
        {
            ArgumentNullException.ThrowIfNull(tables);

            var newTables = Tables.AddRange(tables);
            return WithTables(newTables);
        }

        public DataContext RemoveTables(params TableSymbol[] tables)
        {
            if (tables is null || tables.Length == 0)
                return this;

            return RemoveTables(tables.AsEnumerable());
        }

        public DataContext RemoveTables(IEnumerable<TableSymbol> tables)
        {
            ArgumentNullException.ThrowIfNull(tables);

            var newTables = Tables.RemoveRange(tables);
            return WithTables(newTables);
        }

        public DataContext RemoveAllTables()
        {
            var newTables = Tables.Clear();
            return WithTables(newTables);
        }

        public DataContext WithTables(IEnumerable<TableSymbol> tables)
        {
            ArgumentNullException.ThrowIfNull(tables);

            if (ReferenceEquals(tables, Tables))
                return this;

            var newTables = tables.ToImmutableList();
            return new DataContext(newTables, Relations, Functions, Aggregates, Variables, PropertyProviders, MethodProviders, Comparers);
        }

        // Relations

        public DataContext AddRelations(params TableRelation[] relations)
        {
            if (relations is null || relations.Length == 0)
                return this;

            return AddRelations(relations.AsEnumerable());
        }

        public DataContext AddRelations(IEnumerable<TableRelation> relations)
        {
            ArgumentNullException.ThrowIfNull(relations);

            var newRelations = Relations.AddRange(relations);
            return WithRelations(newRelations);
        }

        public DataContext RemoveRelations(params TableRelation[] relations)
        {
            if (relations is null || relations.Length == 0)
                return this;

            return RemoveRelations(relations.AsEnumerable());
        }

        public DataContext RemoveRelations(IEnumerable<TableRelation> relations)
        {
            ArgumentNullException.ThrowIfNull(relations);

            var newRelations = Relations.RemoveRange(relations);
            return WithRelations(newRelations);
        }

        public DataContext RemoveAllRelations()
        {
            var newRelations = Relations.Clear();
            return WithRelations(newRelations);
        }

        public DataContext WithRelations(IEnumerable<TableRelation> relations)
        {
            ArgumentNullException.ThrowIfNull(relations);

            if (ReferenceEquals(relations, Relations))
                return this;

            var newRelations = relations.ToImmutableList();
            return new DataContext(Tables, newRelations, Functions, Aggregates, Variables, PropertyProviders, MethodProviders, Comparers);
        }

        // Functions

        public DataContext AddFunctions(params FunctionSymbol[] functions)
        {
            if (functions is null || functions.Length == 0)
                return this;

            return AddFunctions(functions.AsEnumerable());
        }

        public DataContext AddFunctions(IEnumerable<FunctionSymbol> functions)
        {
            ArgumentNullException.ThrowIfNull(functions);

            var newFunctions = Functions.AddRange(functions);
            return WithFunctions(newFunctions);
        }

        public DataContext RemoveFunctions(params FunctionSymbol[] functions)
        {
            if (functions is null || functions.Length == 0)
                return this;

            return RemoveFunctions(functions.AsEnumerable());
        }

        public DataContext RemoveFunctions(IEnumerable<FunctionSymbol> functions)
        {
            ArgumentNullException.ThrowIfNull(functions);

            var newFunctions = Functions.RemoveRange(functions);
            return WithFunctions(newFunctions);
        }

        public DataContext RemoveAllFunctions()
        {
            var newFunctions = Functions.Clear();
            return WithFunctions(newFunctions);
        }

        public DataContext WithFunctions(IEnumerable<FunctionSymbol> functions)
        {
            ArgumentNullException.ThrowIfNull(functions);

            if (ReferenceEquals(functions, Functions))
                return this;

            var newFunctions = functions.ToImmutableList();
            return new DataContext(Tables, Relations, newFunctions, Aggregates, Variables, PropertyProviders, MethodProviders, Comparers);
        }

        // Aggregates

        public DataContext AddAggregates(params AggregateSymbol[] aggregates)
        {
            if (aggregates is null || aggregates.Length == 0)
                return this;

            return AddAggregates(aggregates.AsEnumerable());
        }

        public DataContext AddAggregates(IEnumerable<AggregateSymbol> aggregates)
        {
            ArgumentNullException.ThrowIfNull(aggregates);

            var newAggregates = Aggregates.AddRange(aggregates);
            return WithAggregates(newAggregates);
        }

        public DataContext RemoveAggregates(params AggregateSymbol[] aggregates)
        {
            if (aggregates is null || aggregates.Length == 0)
                return this;

            return RemoveAggregates(aggregates.AsEnumerable());
        }

        public DataContext RemoveAggregates(IEnumerable<AggregateSymbol> aggregates)
        {
            ArgumentNullException.ThrowIfNull(aggregates);

            var newAggregates = Aggregates.RemoveRange(aggregates);
            return WithAggregates(newAggregates);
        }

        public DataContext RemoveAllAggregates()
        {
            var newAggregates = Aggregates.Clear();
            return WithAggregates(newAggregates);
        }

        public DataContext WithAggregates(IEnumerable<AggregateSymbol> aggregates)
        {
            ArgumentNullException.ThrowIfNull(aggregates);

            if (ReferenceEquals(aggregates, Aggregates))
                return this;

            var newAggregates = aggregates.ToImmutableList();
            return new DataContext(Tables, Relations, Functions, newAggregates, Variables, PropertyProviders, MethodProviders, Comparers);
        }

        // Variables

        public DataContext AddVariables(params VariableSymbol[] variables)
        {
            if (variables is null || variables.Length == 0)
                return this;

            return AddVariables(variables.AsEnumerable());
        }

        public DataContext AddVariables(IEnumerable<VariableSymbol> variables)
        {
            ArgumentNullException.ThrowIfNull(variables);

            var newVariables = Variables.AddRange(variables);
            return WithVariables(newVariables);
        }

        public DataContext RemoveVariables(params VariableSymbol[] variables)
        {
            if (variables is null || variables.Length == 0)
                return this;

            return RemoveVariables(variables.AsEnumerable());
        }

        public DataContext RemoveVariables(IEnumerable<VariableSymbol> variables)
        {
            ArgumentNullException.ThrowIfNull(variables);

            var newVariables = Variables.RemoveRange(variables);
            return WithVariables(newVariables);
        }

        public DataContext RemoveAllVariables()
        {
            var newVariables = Variables.Clear();
            return WithVariables(newVariables);
        }

        public DataContext WithVariables(IEnumerable<VariableSymbol> variables)
        {
            ArgumentNullException.ThrowIfNull(variables);

            if (ReferenceEquals(variables, Variables))
                return this;

            var newVariables = variables.ToImmutableList();
            return new DataContext(Tables, Relations, Functions, Aggregates, newVariables, PropertyProviders, MethodProviders, Comparers);
        }

        // Property Providers

        public DataContext AddPropertyProvider(Type type, IPropertyProvider provider)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(provider);

            var newProviders = PropertyProviders.Add(type, provider);
            return WithPropertyProviders(newProviders);
        }

        public DataContext AddPropertyProviders(IEnumerable<KeyValuePair<Type, IPropertyProvider>> providers)
        {
            ArgumentNullException.ThrowIfNull(providers);

            var newProviders = PropertyProviders.AddRange(providers);
            return WithPropertyProviders(newProviders);
        }

        public DataContext RemovePropertyProviders(params Type[] types)
        {
            if (types is null || types.Length == 0)
                return this;

            return RemovePropertyProviders(types.AsEnumerable());
        }

        public DataContext RemovePropertyProviders(IEnumerable<Type> types)
        {
            ArgumentNullException.ThrowIfNull(types);

            var newProviders = PropertyProviders.RemoveRange(types);
            return WithPropertyProviders(newProviders);
        }

        public DataContext RemoveAllPropertyProviders()
        {
            var newProviders = PropertyProviders.Clear();
            return WithPropertyProviders(newProviders);
        }

        public DataContext WithPropertyProviders(IImmutableDictionary<Type, IPropertyProvider> providers)
        {
            ArgumentNullException.ThrowIfNull(providers);

            if (ReferenceEquals(PropertyProviders, providers))
                return this;

            return new DataContext(Tables, Relations, Functions, Aggregates, Variables, providers, MethodProviders, Comparers);
        }

        // Method Providers

        public DataContext AddMethodProvider(Type type, IMethodProvider provider)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(provider);

            var newProviders = MethodProviders.Add(type, provider);
            return WithMethodProviders(newProviders);
        }

        public DataContext AddMethodProviders(IEnumerable<KeyValuePair<Type, IMethodProvider>> providers)
        {
            ArgumentNullException.ThrowIfNull(providers);

            var newProviders = MethodProviders.AddRange(providers);
            return WithMethodProviders(newProviders);
        }

        public DataContext RemoveMethodProviders(params Type[] types)
        {
            if (types is null || types.Length == 0)
                return this;

            return RemoveMethodProviders(types.AsEnumerable());
        }

        public DataContext RemoveMethodProviders(IEnumerable<Type> types)
        {
            ArgumentNullException.ThrowIfNull(types);

            var newProviders = MethodProviders.RemoveRange(types);
            return WithMethodProviders(newProviders);
        }

        public DataContext RemoveAllMethodProviders()
        {
            var newProviders = MethodProviders.Clear();
            return WithMethodProviders(newProviders);
        }

        public DataContext WithMethodProviders(IImmutableDictionary<Type, IMethodProvider> providers)
        {
            ArgumentNullException.ThrowIfNull(providers);

            if (ReferenceEquals(MethodProviders, providers))
                return this;

            return new DataContext(Tables, Relations, Functions, Aggregates, Variables, PropertyProviders, providers, Comparers);
        }

        // Comparers

        public DataContext AddComparer(Type type, IComparer comparer)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(comparer);

            var newProviders = Comparers.Add(type, comparer);
            return WithComparers(newProviders);
        }

        public DataContext AddComparers(IEnumerable<KeyValuePair<Type, IComparer>> comparer)
        {
            ArgumentNullException.ThrowIfNull(comparer);

            var newProviders = Comparers.AddRange(comparer);
            return WithComparers(newProviders);
        }

        public DataContext RemoveComparers(params Type[] types)
        {
            if (types is null || types.Length == 0)
                return this;

            return RemoveComparers(types.AsEnumerable());
        }

        public DataContext RemoveComparers(IEnumerable<Type> types)
        {
            ArgumentNullException.ThrowIfNull(types);

            var newProviders = Comparers.RemoveRange(types);
            return WithComparers(newProviders);
        }

        public DataContext RemoveAllComparers()
        {
            var newProviders = Comparers.Clear();
            return WithComparers(newProviders);
        }

        public DataContext WithComparers(IImmutableDictionary<Type, IComparer> comparers)
        {
            ArgumentNullException.ThrowIfNull(comparers);

            if (ReferenceEquals(Comparers, comparers))
                return this;

            return new DataContext(Tables, Relations, Functions, Aggregates, Variables, PropertyProviders, MethodProviders, comparers);
        }
    }
}
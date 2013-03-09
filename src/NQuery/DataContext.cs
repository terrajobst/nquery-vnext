using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Hosting;
using NQuery.Symbols;

namespace NQuery
{
    public sealed class DataContext
    {
        private readonly IImmutableList<TableSymbol> _tables;
        private readonly IImmutableList<TableRelation> _relations;
        private readonly IImmutableList<FunctionSymbol> _functions;
        private readonly IImmutableList<AggregateSymbol> _aggregates;
        private readonly IImmutableList<VariableSymbol> _variables;
        private readonly IImmutableDictionary<Type, IPropertyProvider> _propertyProviders;
        private readonly IImmutableDictionary<Type, IMethodProvider> _methodProviders;
        private readonly IImmutableDictionary<Type, IComparer> _comparers;

        private DataContext(IImmutableList<TableSymbol> tables,
                            IImmutableList<TableRelation> relations,
                            IImmutableList<FunctionSymbol> functions,
                            IImmutableList<AggregateSymbol> aggregates,
                            IImmutableList<VariableSymbol> variables,
                            IImmutableDictionary<Type, IPropertyProvider> propertyProviders,
                            IImmutableDictionary<Type, IMethodProvider> methodProviders,
                            IImmutableDictionary<Type, IComparer> comparers)
        {
            _tables = tables;
            _relations = relations;
            _functions = functions;
            _aggregates = aggregates;
            _variables = variables;
            _propertyProviders = propertyProviders;
            _methodProviders = methodProviders;
            _comparers = comparers;
        }

        public static readonly DataContext Empty = CreateEmpty();
        public static readonly DataContext Default = CreateDefault();

        public IImmutableList<TableSymbol> Tables
        {
            get { return _tables; }
        }

        public IImmutableList<TableRelation> Relations
        {
            get { return _relations; }
        }

        public IImmutableList<FunctionSymbol> Functions
        {
            get { return _functions; }
        }

        public IImmutableList<AggregateSymbol> Aggregates
        {
            get { return _aggregates; }
        }

        public IImmutableList<VariableSymbol> Variables
        {
            get { return _variables; }
        }

        public IImmutableDictionary<Type, IPropertyProvider> PropertyProviders
        {
            get { return _propertyProviders; }
        }

        public IImmutableDictionary<Type, IMethodProvider> MethodProviders
        {
            get { return _methodProviders; }
        }

        public IImmutableDictionary<Type, IComparer> Comparers
        {
            get { return _comparers; }
        }

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
                                                       .Add(typeof (object), reflectionProvider);
            var methodProviders = ImmutableDictionary.Create<Type, IMethodProvider>()
                                                     .Add(typeof (object), reflectionProvider);
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
            if (tables == null || tables.Length == 0)
                return this;

            return AddTables(tables.AsEnumerable());
        }

        public DataContext AddTables(IEnumerable<TableSymbol> tables)
        {
            var newTables = _tables.AddRange(tables);
            return WithTables(newTables);
        }

        public DataContext RemoveTables(params TableSymbol[] tables)
        {
            if (tables == null || tables.Length == 0)
                return this;

            return RemoveTables(tables.AsEnumerable());
        }

        public DataContext RemoveTables(IEnumerable<TableSymbol> tables)
        {
            var newTables = _tables.RemoveRange(tables);
            return WithTables(newTables);
        }

        public DataContext RemoveAllTables()
        {
            var newTables = _tables.Clear();
            return WithTables(newTables);
        }

        public DataContext WithTables(IEnumerable<TableSymbol> tables)
        {
            if (ReferenceEquals(tables, _tables))
                return this;

            var newTables = tables.ToImmutableList();
            return new DataContext(newTables, _relations, _functions, _aggregates, _variables, _propertyProviders, _methodProviders, _comparers);
        }

        // Relations

        public DataContext AddRelations(params TableRelation[] relations)
        {
            if (relations == null || relations.Length == 0)
                return this;

            return AddRelations(relations.AsEnumerable());
        }

        public DataContext AddRelations(IEnumerable<TableRelation> relations)
        {
            var newRelations = _relations.AddRange(relations);
            return WithRelations(newRelations);
        }

        public DataContext RemoveRelations(params TableRelation[] relations)
        {
            if (relations == null || relations.Length == 0)
                return this;

            return RemoveRelations(relations.AsEnumerable());
        }

        public DataContext RemoveRelations(IEnumerable<TableRelation> relations)
        {
            var newRelations = _relations.RemoveRange(relations);
            return WithRelations(newRelations);
        }

        public DataContext RemoveAllRelations()
        {
            var newRelations = _relations.Clear();
            return WithRelations(newRelations);
        }

        public DataContext WithRelations(IEnumerable<TableRelation> relations)
        {
            if (ReferenceEquals(relations, _relations))
                return this;

            var newRelations = relations.ToImmutableList();
            return new DataContext(_tables, newRelations, _functions, _aggregates, _variables, _propertyProviders, _methodProviders, _comparers);
        }

        // Functions

        public DataContext AddFunctions(params FunctionSymbol[] functions)
        {
            if (functions == null || functions.Length == 0)
                return this;

            return AddFunctions(functions.AsEnumerable());
        }

        public DataContext AddFunctions(IEnumerable<FunctionSymbol> functions)
        {
            var newFunctions = _functions.AddRange(functions);
            return WithFunctions(newFunctions);
        }

        public DataContext RemoveFunctions(params FunctionSymbol[] functions)
        {
            if (functions == null || functions.Length == 0)
                return this;

            return RemoveFunctions(functions.AsEnumerable());
        }

        public DataContext RemoveFunctions(IEnumerable<FunctionSymbol> functions)
        {
            var newFunctions = _functions.RemoveRange(functions);
            return WithFunctions(newFunctions);
        }

        public DataContext RemoveAllFunctions()
        {
            var newFunctions = _functions.Clear();
            return WithFunctions(newFunctions);
        }

        public DataContext WithFunctions(IEnumerable<FunctionSymbol> functions)
        {
            if (ReferenceEquals(functions, _functions))
                return this;

            var newFunctions = functions.ToImmutableList();
            return new DataContext(_tables, _relations, newFunctions, _aggregates, _variables, _propertyProviders, _methodProviders, _comparers);
        }

        // Aggregates

        public DataContext AddAggregates(params AggregateSymbol[] aggregates)
        {
            if (aggregates == null || aggregates.Length == 0)
                return this;

            return AddAggregates(aggregates.AsEnumerable());
        }

        public DataContext AddAggregates(IEnumerable<AggregateSymbol> aggregates)
        {
            var newAggregates = _aggregates.AddRange(aggregates);
            return WithAggregates(newAggregates);
        }

        public DataContext RemoveAggregates(params AggregateSymbol[] aggregates)
        {
            if (aggregates == null || aggregates.Length == 0)
                return this;

            return RemoveAggregates(aggregates.AsEnumerable());
        }

        public DataContext RemoveAggregates(IEnumerable<AggregateSymbol> aggregates)
        {
            var newAggregates = _aggregates.RemoveRange(aggregates);
            return WithAggregates(newAggregates);
        }

        public DataContext RemoveAllAggregates()
        {
            var newAggregates = _aggregates.Clear();
            return WithAggregates(newAggregates);
        }

        public DataContext WithAggregates(IEnumerable<AggregateSymbol> aggregates)
        {
            if (ReferenceEquals(aggregates, _aggregates))
                return this;

            var newAggregates = aggregates.ToImmutableList();
            return new DataContext(_tables, _relations, _functions, newAggregates, _variables, _propertyProviders, _methodProviders, _comparers);
        }

        // Variables

        public DataContext AddVariables(params VariableSymbol[] variables)
        {
            if (variables == null || variables.Length == 0)
                return this;

            return AddVariables(variables.AsEnumerable());
        }

        public DataContext AddVariables(IEnumerable<VariableSymbol> variables)
        {
            var newVariables = _variables.AddRange(variables);
            return WithVariables(newVariables);
        }

        public DataContext RemoveVariables(params VariableSymbol[] variables)
        {
            if (variables == null || variables.Length == 0)
                return this;

            return RemoveVariables(variables.AsEnumerable());
        }

        public DataContext RemoveVariables(IEnumerable<VariableSymbol> variables)
        {
            var newVariables = _variables.RemoveRange(variables);
            return WithVariables(newVariables);
        }

        public DataContext RemoveAllVariables()
        {
            var newVariables = _variables.Clear();
            return WithVariables(newVariables);
        }

        public DataContext WithVariables(IEnumerable<VariableSymbol> variables)
        {
            if (ReferenceEquals(variables, _variables))
                return this;

            var newVariables = variables.ToImmutableList();
            return new DataContext(_tables, _relations, _functions, _aggregates, newVariables, _propertyProviders, _methodProviders, _comparers);
        }

        // Property Providers

        public DataContext AddPropertyProvider(Type type, IPropertyProvider provider)
        {
            var newProviders = _propertyProviders.Add(type, provider);
            return WithPropertyProviders(newProviders);
        }

        public DataContext AddPropertyProviders(IEnumerable<KeyValuePair<Type, IPropertyProvider>> providers)
        {
            var newProviders = _propertyProviders.AddRange(providers);
            return WithPropertyProviders(newProviders);
        }

        public DataContext RemovePropertyProviders(params Type[] types)
        {
            if (types == null || types.Length == 0)
                return this;

            return RemovePropertyProviders(types.AsEnumerable());
        }

        public DataContext RemovePropertyProviders(IEnumerable<Type> types)
        {
            var newProviders = _propertyProviders.RemoveRange(types);
            return WithPropertyProviders(newProviders);
        }

        public DataContext RemoveAllPropertyProviders()
        {
            var newProviders = _propertyProviders.Clear();
            return WithPropertyProviders(newProviders);
        }

        public DataContext WithPropertyProviders(IImmutableDictionary<Type, IPropertyProvider> providers)
        {
            if (ReferenceEquals(_propertyProviders, providers))
                return this;

            return new DataContext(_tables, _relations, _functions, _aggregates, _variables, providers, _methodProviders, _comparers);
        }

        // Method Providers

        public DataContext AddMethodProvider(Type type, IMethodProvider provider)
        {
            var newProviders = _methodProviders.Add(type, provider);
            return WithMethodProviders(newProviders);
        }

        public DataContext AddMethodProviders(IEnumerable<KeyValuePair<Type, IMethodProvider>> providers)
        {
            var newProviders = _methodProviders.AddRange(providers);
            return WithMethodProviders(newProviders);
        }

        public DataContext RemoveMethodProviders(params Type[] types)
        {
            if (types == null || types.Length == 0)
                return this;

            return RemoveMethodProviders(types.AsEnumerable());
        }

        public DataContext RemoveMethodProviders(IEnumerable<Type> types)
        {
            var newProviders = _methodProviders.RemoveRange(types);
            return WithMethodProviders(newProviders);
        }

        public DataContext RemoveAllMethodProviders()
        {
            var newProviders = _methodProviders.Clear();
            return WithMethodProviders(newProviders);
        }

        public DataContext WithMethodProviders(IImmutableDictionary<Type, IMethodProvider> providers)
        {
            if (ReferenceEquals(_methodProviders, providers))
                return this;

            return new DataContext(_tables, _relations, _functions, _aggregates, _variables, _propertyProviders, providers, _comparers);
        }

        // Comparers

        public DataContext AddComparer(Type type, IComparer comparer)
        {
            var newProviders = _comparers.Add(type, comparer);
            return WithComparers(newProviders);
        }

        public DataContext AddComparers(IEnumerable<KeyValuePair<Type, IComparer>> comparer)
        {
            var newProviders = _comparers.AddRange(comparer);
            return WithComparers(newProviders);
        }

        public DataContext RemoveComparers(params Type[] types)
        {
            if (types == null || types.Length == 0)
                return this;

            return RemoveComparers(types.AsEnumerable());
        }

        public DataContext RemoveComparers(IEnumerable<Type> types)
        {
            var newProviders = _comparers.RemoveRange(types);
            return WithComparers(newProviders);
        }

        public DataContext RemoveAllComparers()
        {
            var newProviders = _comparers.Clear();
            return WithComparers(newProviders);
        }

        public DataContext WithComparers(IImmutableDictionary<Type, IComparer> comparers)
        {
            if (ReferenceEquals(_comparers, comparers))
                return this;

            return new DataContext(_tables, _relations, _functions, _aggregates, _variables, _propertyProviders, _methodProviders, comparers);
        }
    }
}
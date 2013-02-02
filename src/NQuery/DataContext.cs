using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Runtime;
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

        private DataContext(IImmutableList<TableSymbol> tables,
                            IImmutableList<TableRelation> relations,
                            IImmutableList<FunctionSymbol> functions,
                            IImmutableList<AggregateSymbol> aggregates,
                            IImmutableList<VariableSymbol> variables,
                            IImmutableDictionary<Type, IPropertyProvider> propertyProviders,
                            IImmutableDictionary<Type, IMethodProvider> methodProviders)
        {
            _tables = tables;
            _relations = relations;
            _functions = functions;
            _aggregates = aggregates;
            _variables = variables;
            _propertyProviders = propertyProviders;
            _methodProviders = methodProviders;
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

        private static DataContext CreateEmpty()
        {
            return new DataContext(ImmutableList<TableSymbol>.Empty,
                                   ImmutableList<TableRelation>.Empty,
                                   ImmutableList<FunctionSymbol>.Empty,
                                   ImmutableList<AggregateSymbol>.Empty,
                                   ImmutableList<VariableSymbol>.Empty,
                                   ImmutableDictionary<Type, IPropertyProvider>.Empty,
                                   ImmutableDictionary<Type, IMethodProvider>.Empty);
        }

        private static DataContext CreateDefault()
        {
            var functions = BuiltInFunctions.GetFunctions().ToImmutableList();
            var aggregates = BuiltInAggregates.GetAggregates().ToImmutableList();
            var reflectionProvider = new ReflectionProvider();
            var propertyProviders = ImmutableDictionary<Type, IPropertyProvider>.Empty.Add(typeof(object), reflectionProvider);
            var methodProviders = ImmutableDictionary<Type, IMethodProvider>.Empty.Add(typeof(object), reflectionProvider);
            return new DataContext(ImmutableList<TableSymbol>.Empty,
                                   ImmutableList<TableRelation>.Empty,
                                   functions,
                                   aggregates,
                                   ImmutableList<VariableSymbol>.Empty,
                                   propertyProviders,
                                   methodProviders);
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
            var newTables = _tables;
            foreach (var tableSymbol in tables)
                newTables = newTables.Remove(tableSymbol);

            return WithTables(newTables);
        }

        public DataContext RemoveAllTables()
        {
            var newTables = ImmutableList<TableSymbol>.Empty;
            return WithTables(newTables);
        }

        public DataContext WithTables(IEnumerable<TableSymbol> tables)
        {
            if (ReferenceEquals(tables, _tables))
                return this;

            var newTables = tables.ToImmutableList();
            return new DataContext(newTables, _relations, _functions, _aggregates, _variables, _propertyProviders, _methodProviders);
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
            var newRelations = _relations;
            foreach (var relationSymbol in relations)
                newRelations = newRelations.Remove(relationSymbol);

            return WithRelations(newRelations);
        }

        public DataContext RemoveAllRelations()
        {
            var newRelations = ImmutableList<TableRelation>.Empty;
            return WithRelations(newRelations);
        }

        public DataContext WithRelations(IEnumerable<TableRelation> relations)
        {
            if (ReferenceEquals(relations, _relations))
                return this;

            var newRelations = relations.ToImmutableList();
            return new DataContext(_tables, newRelations, _functions, _aggregates, _variables, _propertyProviders, _methodProviders);
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
            var newFunctions = _functions;
            foreach (var functionSymbol in functions)
                newFunctions = newFunctions.Remove(functionSymbol);

            return WithFunctions(newFunctions);
        }

        public DataContext RemoveAllFunctions()
        {
            var newFunctions = ImmutableList<FunctionSymbol>.Empty;
            return WithFunctions(newFunctions);
        }

        public DataContext WithFunctions(IEnumerable<FunctionSymbol> functions)
        {
            if (ReferenceEquals(functions, _functions))
                return this;

            var newFunctions = functions.ToImmutableList();
            return new DataContext(_tables, _relations, newFunctions, _aggregates, _variables, _propertyProviders, _methodProviders);
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
            var newAggregates = _aggregates;
            foreach (var aggregateSymbol in aggregates)
                newAggregates = newAggregates.Remove(aggregateSymbol);

            return WithAggregates(newAggregates);
        }

        public DataContext RemoveAllAggregates()
        {
            var newAggregates = ImmutableList<AggregateSymbol>.Empty;
            return WithAggregates(newAggregates);
        }

        public DataContext WithAggregates(IEnumerable<AggregateSymbol> aggregates)
        {
            if (ReferenceEquals(aggregates, _aggregates))
                return this;

            var newAggregates = aggregates.ToImmutableList();
            return new DataContext(_tables, _relations, _functions, newAggregates, _variables, _propertyProviders, _methodProviders);
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
            var newVariables = _variables;
            foreach (var variableSymbol in variables)
                newVariables = newVariables.Remove(variableSymbol);

            return WithVariables(newVariables);
        }

        public DataContext RemoveAllVariables()
        {
            var newVariables = ImmutableList<VariableSymbol>.Empty;
            return WithVariables(newVariables);
        }

        public DataContext WithVariables(IEnumerable<VariableSymbol> variables)
        {
            if (ReferenceEquals(variables, _variables))
                return this;

            var newVariables = variables.ToImmutableList();
            return new DataContext(_tables, _relations, _functions, _aggregates, newVariables, _propertyProviders, _methodProviders);
        }

        // Property Providers

        public DataContext WithPropertyProviders(IImmutableDictionary<Type, IPropertyProvider> providers)
        {
            if (ReferenceEquals(_propertyProviders, providers))
                return this;

            return new DataContext(_tables, _relations, _functions, _aggregates, _variables, providers, _methodProviders);
        }

        // Method Providers

        public DataContext WithMethodProviders(IImmutableDictionary<Type, IMethodProvider> providers)
        {
            if (ReferenceEquals(_methodProviders, providers))
                return this;

            return new DataContext(_tables, _relations, _functions, _aggregates, _variables, _propertyProviders, providers);
        }
    }
}
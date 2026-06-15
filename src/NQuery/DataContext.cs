using System.Collections;
using System.Collections.Immutable;

using NQuery.Metadata;
using NQuery.Metadata.Aggregation;

namespace NQuery;

public sealed class DataContext
{
    private DataContext(IImmutableList<TableDefinition> tables,
                        IImmutableList<RelationshipDefinition> relationships,
                        IImmutableList<FunctionDefinition> functions,
                        IImmutableList<AggregateDefinition> aggregates,
                        IImmutableList<VariableDefinition> variables,
                        IImmutableDictionary<Type, IPropertyProvider> propertyProviders,
                        IImmutableDictionary<Type, IMethodProvider> methodProviders,
                        IImmutableDictionary<Type, IComparer> comparers)
    {
        Tables = tables;
        Relationships = relationships;
        Functions = functions;
        Aggregates = aggregates;
        Variables = variables;
        PropertyProviders = propertyProviders;
        MethodProviders = methodProviders;
        Comparers = comparers;
    }

    public static readonly DataContext Empty = CreateEmpty();
    public static readonly DataContext Default = CreateDefault();

    public IImmutableList<TableDefinition> Tables { get; }

    public IImmutableList<RelationshipDefinition> Relationships { get; }

    public IImmutableList<FunctionDefinition> Functions { get; }

    public IImmutableList<AggregateDefinition> Aggregates { get; }

    public IImmutableList<VariableDefinition> Variables { get; }

    public IImmutableDictionary<Type, IPropertyProvider> PropertyProviders { get; }

    public IImmutableDictionary<Type, IMethodProvider> MethodProviders { get; }

    public IImmutableDictionary<Type, IComparer> Comparers { get; }

    private static DataContext CreateEmpty()
    {
        return new DataContext(ImmutableList.Create<TableDefinition>(),
                               ImmutableList.Create<RelationshipDefinition>(),
                               ImmutableList.Create<FunctionDefinition>(),
                               ImmutableList.Create<AggregateDefinition>(),
                               ImmutableList.Create<VariableDefinition>(),
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
        return new DataContext(ImmutableList.Create<TableDefinition>(),
                               ImmutableList.Create<RelationshipDefinition>(),
                               functions,
                               aggregates,
                               ImmutableList.Create<VariableDefinition>(),
                               propertyProviders,
                               methodProviders,
                               comparers);
    }

    // Tables

    public DataContext AddTables(params IEnumerable<TableDefinition> tables)
    {
        if (tables is null)
            return this;

        var newTables = Tables.AddRange(tables);
        return WithTables(newTables);
    }

    public DataContext RemoveTables(params IEnumerable<TableDefinition> tables)
    {
        if (tables is null)
            return this;

        var newTables = Tables.RemoveRange(tables);
        return WithTables(newTables);
    }

    public DataContext RemoveAllTables()
    {
        var newTables = Tables.Clear();
        return WithTables(newTables);
    }

    public DataContext WithTables(IEnumerable<TableDefinition> tables)
    {
        ThrowIfNull(tables);

        if (ReferenceEquals(tables, Tables))
            return this;

        var newTables = tables.ToImmutableList();
        return new DataContext(newTables, Relationships, Functions, Aggregates, Variables, PropertyProviders, MethodProviders, Comparers);
    }

    // Relationships

    public DataContext AddRelationships(params IEnumerable<RelationshipDefinition> relationships)
    {
        if (relationships is null)
            return this;

        var newRelationships = Relationships.AddRange(relationships);
        return WithRelationships(newRelationships);
    }

    public DataContext RemoveRelationships(params IEnumerable<RelationshipDefinition> relationships)
    {
        if (relationships is null)
            return this;

        var newRelationships = Relationships.RemoveRange(relationships);
        return WithRelationships(newRelationships);
    }

    public DataContext RemoveAllRelationships()
    {
        var newRelationships = Relationships.Clear();
        return WithRelationships(newRelationships);
    }

    public DataContext WithRelationships(IEnumerable<RelationshipDefinition> relationships)
    {
        ThrowIfNull(relationships);

        if (ReferenceEquals(relationships, Relationships))
            return this;

        var newRelationships = relationships.ToImmutableList();
        return new DataContext(Tables, newRelationships, Functions, Aggregates, Variables, PropertyProviders, MethodProviders, Comparers);
    }

    // Functions

    public DataContext AddFunctions(params IEnumerable<FunctionDefinition> functions)
    {
        if (functions is null)
            return this;

        var newFunctions = Functions.AddRange(functions);
        return WithFunctions(newFunctions);
    }

    public DataContext RemoveFunctions(params IEnumerable<FunctionDefinition> functions)
    {
        if (functions is null)
            return this;

        var newFunctions = Functions.RemoveRange(functions);
        return WithFunctions(newFunctions);
    }

    public DataContext RemoveAllFunctions()
    {
        var newFunctions = Functions.Clear();
        return WithFunctions(newFunctions);
    }

    public DataContext WithFunctions(IEnumerable<FunctionDefinition> functions)
    {
        ThrowIfNull(functions);

        if (ReferenceEquals(functions, Functions))
            return this;

        var newFunctions = functions.ToImmutableList();
        return new DataContext(Tables, Relationships, newFunctions, Aggregates, Variables, PropertyProviders, MethodProviders, Comparers);
    }

    // Aggregates

    public DataContext AddAggregates(params IEnumerable<AggregateDefinition> aggregates)
    {
        if (aggregates is null)
            return this;

        var newAggregates = Aggregates.AddRange(aggregates);
        return WithAggregates(newAggregates);
    }

    public DataContext RemoveAggregates(params IEnumerable<AggregateDefinition> aggregates)
    {
        if (aggregates is null)
            return this;

        var newAggregates = Aggregates.RemoveRange(aggregates);
        return WithAggregates(newAggregates);
    }

    public DataContext RemoveAllAggregates()
    {
        var newAggregates = Aggregates.Clear();
        return WithAggregates(newAggregates);
    }

    public DataContext WithAggregates(IEnumerable<AggregateDefinition> aggregates)
    {
        ThrowIfNull(aggregates);

        if (ReferenceEquals(aggregates, Aggregates))
            return this;

        var newAggregates = aggregates.ToImmutableList();
        return new DataContext(Tables, Relationships, Functions, newAggregates, Variables, PropertyProviders, MethodProviders, Comparers);
    }

    // Variables

    public DataContext AddVariables(params IEnumerable<VariableDefinition> variables)
    {
        if (variables is null)
            return this;

        var newVariables = Variables.AddRange(variables);
        return WithVariables(newVariables);
    }

    public DataContext RemoveVariables(params IEnumerable<VariableDefinition> variables)
    {
        if (variables is null)
            return this;

        var newVariables = Variables.RemoveRange(variables);
        return WithVariables(newVariables);
    }

    public DataContext RemoveAllVariables()
    {
        var newVariables = Variables.Clear();
        return WithVariables(newVariables);
    }

    public DataContext WithVariables(IEnumerable<VariableDefinition> variables)
    {
        ThrowIfNull(variables);

        if (ReferenceEquals(variables, Variables))
            return this;

        var newVariables = variables.ToImmutableList();
        return new DataContext(Tables, Relationships, Functions, Aggregates, newVariables, PropertyProviders, MethodProviders, Comparers);
    }

    // Property Providers

    public DataContext AddPropertyProvider(Type type, IPropertyProvider provider)
    {
        ThrowIfNull(type);
        ThrowIfNull(provider);

        var newProviders = PropertyProviders.Add(type, provider);
        return WithPropertyProviders(newProviders);
    }

    public DataContext AddPropertyProviders(IEnumerable<KeyValuePair<Type, IPropertyProvider>> providers)
    {
        ThrowIfNull(providers);

        var newProviders = PropertyProviders.AddRange(providers);
        return WithPropertyProviders(newProviders);
    }

    public DataContext RemovePropertyProviders(params IEnumerable<Type> types)
    {
        if (types is null)
            return this;

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
        ThrowIfNull(providers);

        if (ReferenceEquals(PropertyProviders, providers))
            return this;

        return new DataContext(Tables, Relationships, Functions, Aggregates, Variables, providers, MethodProviders, Comparers);
    }

    // Method Providers

    public DataContext AddMethodProvider(Type type, IMethodProvider provider)
    {
        ThrowIfNull(type);
        ThrowIfNull(provider);

        var newProviders = MethodProviders.Add(type, provider);
        return WithMethodProviders(newProviders);
    }

    public DataContext AddMethodProviders(IEnumerable<KeyValuePair<Type, IMethodProvider>> providers)
    {
        ThrowIfNull(providers);

        var newProviders = MethodProviders.AddRange(providers);
        return WithMethodProviders(newProviders);
    }

    public DataContext RemoveMethodProviders(params IEnumerable<Type> types)
    {
        if (types is null)
            return this;

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
        ThrowIfNull(providers);

        if (ReferenceEquals(MethodProviders, providers))
            return this;

        return new DataContext(Tables, Relationships, Functions, Aggregates, Variables, PropertyProviders, providers, Comparers);
    }

    // Comparers

    public DataContext AddComparer(Type type, IComparer comparer)
    {
        ThrowIfNull(type);
        ThrowIfNull(comparer);

        var newProviders = Comparers.Add(type, comparer);
        return WithComparers(newProviders);
    }

    public DataContext AddComparers(IEnumerable<KeyValuePair<Type, IComparer>> comparer)
    {
        ThrowIfNull(comparer);

        var newProviders = Comparers.AddRange(comparer);
        return WithComparers(newProviders);
    }

    public DataContext RemoveComparers(params IEnumerable<Type> types)
    {
        if (types is null)
            return this;

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
        ThrowIfNull(comparers);

        if (ReferenceEquals(Comparers, comparers))
            return this;

        return new DataContext(Tables, Relationships, Functions, Aggregates, Variables, PropertyProviders, MethodProviders, comparers);
    }
}

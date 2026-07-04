using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Metadata;

namespace NQuery;

public sealed class Catalog
{
    private Catalog(ImmutableArray<TableDefinition> tables,
                    ImmutableArray<RelationshipDefinition> relationships,
                    ImmutableArray<FunctionDefinition> functions,
                    ImmutableArray<AggregateDefinition> aggregates,
                    ImmutableArray<VariableDefinition> variables,
                    FrozenDictionary<Type, IPropertyProvider> propertyProviders,
                    FrozenDictionary<Type, IMethodProvider> methodProviders,
                    FrozenDictionary<Type, IComparer> comparers)
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

    public static Catalog Empty { get; } = CreateEmpty();
    public static Catalog Default { get; } = CreateDefault();

    public ImmutableArray<TableDefinition> Tables { get; }

    public ImmutableArray<RelationshipDefinition> Relationships { get; }

    public ImmutableArray<FunctionDefinition> Functions { get; }

    public ImmutableArray<AggregateDefinition> Aggregates { get; }

    public ImmutableArray<VariableDefinition> Variables { get; }

    public FrozenDictionary<Type, IPropertyProvider> PropertyProviders { get; }

    public FrozenDictionary<Type, IMethodProvider> MethodProviders { get; }

    public FrozenDictionary<Type, IComparer> Comparers { get; }

    private static Catalog CreateEmpty()
    {
        return new Catalog([], [], [], [], [], [], [], []);
    }

    private static Catalog CreateDefault()
    {
        var reflectionProvider = new ReflectionProvider();
        var propertyProviders = new Dictionary<Type, IPropertyProvider> { [typeof(object)] = reflectionProvider }.ToFrozenDictionary();
        var methodProviders = new Dictionary<Type, IMethodProvider> { [typeof(object)] = reflectionProvider }.ToFrozenDictionary();
        return new Catalog([],
                           [],
                           BuiltInFunctions.Functions,
                           BuiltInAggregates.Aggregates,
                           [],
                           propertyProviders,
                           methodProviders,
                           []);
    }

    // Tables

    public Catalog AddTables(params IEnumerable<TableDefinition> tables)
    {
        if (tables is null)
            return this;

        var newTables = Tables.AddRange(tables);
        return WithTables(newTables);
    }

    public Catalog RemoveTables(params IEnumerable<TableDefinition> tables)
    {
        if (tables is null)
            return this;

        var newTables = Tables.RemoveRange(tables);
        return WithTables(newTables);
    }

    public Catalog RemoveAllTables()
    {
        return WithTables(Tables.Clear());
    }

    public Catalog WithTables(IEnumerable<TableDefinition> tables)
    {
        ThrowIfNull(tables);

        var newTables = tables.ToImmutableArray();
        if (newTables == Tables)
            return this;

        return new Catalog(newTables, Relationships, Functions, Aggregates, Variables, PropertyProviders, MethodProviders, Comparers);
    }

    // Relationships

    public Catalog AddRelationships(params IEnumerable<RelationshipDefinition> relationships)
    {
        if (relationships is null)
            return this;

        var newRelationships = Relationships.AddRange(relationships);
        return WithRelationships(newRelationships);
    }

    public Catalog RemoveRelationships(params IEnumerable<RelationshipDefinition> relationships)
    {
        if (relationships is null)
            return this;

        var newRelationships = Relationships.RemoveRange(relationships);
        return WithRelationships(newRelationships);
    }

    public Catalog RemoveAllRelationships()
    {
        return WithRelationships(Relationships.Clear());
    }

    public Catalog WithRelationships(IEnumerable<RelationshipDefinition> relationships)
    {
        ThrowIfNull(relationships);

        var newRelationships = relationships.ToImmutableArray();
        if (newRelationships == Relationships)
            return this;

        return new Catalog(Tables, newRelationships, Functions, Aggregates, Variables, PropertyProviders, MethodProviders, Comparers);
    }

    // Functions

    public Catalog AddFunctions(params IEnumerable<FunctionDefinition> functions)
    {
        if (functions is null)
            return this;

        var newFunctions = Functions.AddRange(functions);
        return WithFunctions(newFunctions);
    }

    public Catalog RemoveFunctions(params IEnumerable<FunctionDefinition> functions)
    {
        if (functions is null)
            return this;

        var newFunctions = Functions.RemoveRange(functions);
        return WithFunctions(newFunctions);
    }

    public Catalog RemoveAllFunctions()
    {
        return WithFunctions(Functions.Clear());
    }

    public Catalog WithFunctions(IEnumerable<FunctionDefinition> functions)
    {
        ThrowIfNull(functions);

        var newFunctions = functions.ToImmutableArray();
        if (newFunctions == Functions)
            return this;

        EnsureNoCollidingFunctions(newFunctions);
        return new Catalog(Tables, Relationships, newFunctions, Aggregates, Variables, PropertyProviders, MethodProviders, Comparers);
    }

    // Unlike methods (which a reflection provider collapses during import), functions reach the
    // binder as separate symbols and are picked by overload resolution. Once Nullable<T> is erased,
    // two functions whose parameters differ only by nullability -- or that are outright identical --
    // become indistinguishable signatures, so resolving a call to either would be ambiguous. Reject
    // them eagerly here (the parameter types are already erased) rather than failing at the call site.
    private static void EnsureNoCollidingFunctions(IEnumerable<FunctionDefinition> functions)
    {
        var seen = new HashSet<FunctionDefinition>(SignatureEqualityComparer.Instance);
        foreach (var function in functions)
        {
            if (!seen.Add(function))
            {
                var parameters = string.Join(", ", function.GetParameterTypes().Select(t => t.FullName ?? t.Name));
                throw new ArgumentException(string.Format(Resources.DuplicateFunctionSignature, $"{function.Name}({parameters})"), nameof(functions));
            }
        }
    }

    // Aggregates

    public Catalog AddAggregates(params IEnumerable<AggregateDefinition> aggregates)
    {
        if (aggregates is null)
            return this;

        var newAggregates = Aggregates.AddRange(aggregates);
        return WithAggregates(newAggregates);
    }

    public Catalog RemoveAggregates(params IEnumerable<AggregateDefinition> aggregates)
    {
        if (aggregates is null)
            return this;

        var newAggregates = Aggregates.RemoveRange(aggregates);
        return WithAggregates(newAggregates);
    }

    public Catalog RemoveAllAggregates()
    {
        return WithAggregates(Aggregates.Clear());
    }

    public Catalog WithAggregates(IEnumerable<AggregateDefinition> aggregates)
    {
        ThrowIfNull(aggregates);

        var newAggregates = aggregates.ToImmutableArray();
        if (newAggregates == Aggregates)
            return this;

        return new Catalog(Tables, Relationships, Functions, newAggregates, Variables, PropertyProviders, MethodProviders, Comparers);
    }

    // Variables

    public Catalog AddVariables(params IEnumerable<VariableDefinition> variables)
    {
        if (variables is null)
            return this;

        var newVariables = Variables.AddRange(variables);
        return WithVariables(newVariables);
    }

    public Catalog RemoveVariables(params IEnumerable<VariableDefinition> variables)
    {
        if (variables is null)
            return this;

        var newVariables = Variables.RemoveRange(variables);
        return WithVariables(newVariables);
    }

    public Catalog RemoveAllVariables()
    {
        return WithVariables(Variables.Clear());
    }

    public Catalog WithVariables(IEnumerable<VariableDefinition> variables)
    {
        ThrowIfNull(variables);

        var newVariables = variables.ToImmutableArray();
        if (newVariables == Variables)
            return this;

        return new Catalog(Tables, Relationships, Functions, Aggregates, newVariables, PropertyProviders, MethodProviders, Comparers);
    }

    // Property Providers

    public Catalog AddPropertyProvider(Type type, IPropertyProvider provider)
    {
        ThrowIfNull(type);
        ThrowIfNull(provider);

        var newProviders = AddEntry(PropertyProviders, type, provider);
        return WithPropertyProviders(newProviders);
    }

    public Catalog AddPropertyProviders(IEnumerable<KeyValuePair<Type, IPropertyProvider>> providers)
    {
        ThrowIfNull(providers);

        var newProviders = AddEntries(PropertyProviders, providers);
        return WithPropertyProviders(newProviders);
    }

    public Catalog RemovePropertyProviders(params IEnumerable<Type> types)
    {
        if (types is null)
            return this;

        var newProviders = RemoveEntries(PropertyProviders, types);
        return WithPropertyProviders(newProviders);
    }

    public Catalog RemoveAllPropertyProviders()
    {
        return WithPropertyProviders([]);
    }

    public Catalog WithPropertyProviders(FrozenDictionary<Type, IPropertyProvider> providers)
    {
        ThrowIfNull(providers);

        if (ReferenceEquals(PropertyProviders, providers))
            return this;

        return new Catalog(Tables, Relationships, Functions, Aggregates, Variables, providers, MethodProviders, Comparers);
    }

    // Method Providers

    public Catalog AddMethodProvider(Type type, IMethodProvider provider)
    {
        ThrowIfNull(type);
        ThrowIfNull(provider);

        var newProviders = AddEntry(MethodProviders, type, provider);
        return WithMethodProviders(newProviders);
    }

    public Catalog AddMethodProviders(IEnumerable<KeyValuePair<Type, IMethodProvider>> providers)
    {
        ThrowIfNull(providers);

        var newProviders = AddEntries(MethodProviders, providers);
        return WithMethodProviders(newProviders);
    }

    public Catalog RemoveMethodProviders(params IEnumerable<Type> types)
    {
        if (types is null)
            return this;

        var newProviders = RemoveEntries(MethodProviders, types);
        return WithMethodProviders(newProviders);
    }

    public Catalog RemoveAllMethodProviders()
    {
        return WithMethodProviders([]);
    }

    public Catalog WithMethodProviders(FrozenDictionary<Type, IMethodProvider> providers)
    {
        ThrowIfNull(providers);

        if (ReferenceEquals(MethodProviders, providers))
            return this;

        return new Catalog(Tables, Relationships, Functions, Aggregates, Variables, PropertyProviders, providers, Comparers);
    }

    // Comparers

    public Catalog AddComparer(Type type, IComparer comparer)
    {
        ThrowIfNull(type);
        ThrowIfNull(comparer);

        var newProviders = AddEntry(Comparers, type, comparer);
        return WithComparers(newProviders);
    }

    public Catalog AddComparer<T>(IComparer<T> comparer)
    {
        ThrowIfNull(comparer);

        // The dictionary is keyed by the non-generic IComparer, so a comparer that only implements
        // IComparer<T> is wrapped to expose the non-generic side the rest of the pipeline still uses.
        // The typed side is recovered without boxing when the comparer reaches a typed sort key.
        var stored = comparer as IComparer ?? new UnboxingComparer<T>(comparer);
        var newProviders = AddEntry(Comparers, typeof(T), stored);
        return WithComparers(newProviders);
    }

    public Catalog AddComparers(IEnumerable<KeyValuePair<Type, IComparer>> comparer)
    {
        ThrowIfNull(comparer);

        var newProviders = AddEntries(Comparers, comparer);
        return WithComparers(newProviders);
    }

    public Catalog RemoveComparers(params IEnumerable<Type> types)
    {
        if (types is null)
            return this;

        var newProviders = RemoveEntries(Comparers, types);
        return WithComparers(newProviders);
    }

    public Catalog RemoveAllComparers()
    {
        return WithComparers([]);
    }

    public Catalog WithComparers(FrozenDictionary<Type, IComparer> comparers)
    {
        ThrowIfNull(comparers);

        if (ReferenceEquals(Comparers, comparers))
            return this;

        return new Catalog(Tables, Relationships, Functions, Aggregates, Variables, PropertyProviders, MethodProviders, comparers);
    }

    // FrozenDictionary is optimized for fast lookups but cannot be mutated in place, so every add
    // or remove rebuilds the dictionary. That is acceptable because the catalog is built rarely and
    // read often. These helpers preserve the semantics the catalog previously inherited from
    // IImmutableDictionary: adding an entry that already maps the key to an equal value is a no-op,
    // while re-mapping a key to a different value is rejected.

    private static FrozenDictionary<TKey, TValue> AddEntry<TKey, TValue>(FrozenDictionary<TKey, TValue> source, TKey key, TValue value)
        where TKey : Type
    {
        if (source.TryGetValue(key, out var existing))
        {
            if (EqualityComparer<TValue>.Default.Equals(existing, value))
                return source;

            throw new ArgumentException(string.Format(Resources.DuplicateCatalogKey, key));
        }

        var builder = ToBuilder(source);
        builder.Add(key, value);
        return builder.ToFrozenDictionary();
    }

    private static FrozenDictionary<TKey, TValue> AddEntries<TKey, TValue>(FrozenDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> items)
        where TKey : Type
    {
        Dictionary<TKey, TValue>? builder = null;
        foreach (var item in items)
        {
            IReadOnlyDictionary<TKey, TValue> current = builder ?? (IReadOnlyDictionary<TKey, TValue>)source;
            if (current.TryGetValue(item.Key, out var existing))
            {
                if (EqualityComparer<TValue>.Default.Equals(existing, item.Value))
                    continue;

                throw new ArgumentException(string.Format(Resources.DuplicateCatalogKey, item.Key));
            }

            builder ??= ToBuilder(source);
            builder.Add(item.Key, item.Value);
        }

        return builder is null ? source : builder.ToFrozenDictionary();
    }

    private static FrozenDictionary<TKey, TValue> RemoveEntries<TKey, TValue>(FrozenDictionary<TKey, TValue> source, IEnumerable<TKey> keys)
        where TKey : Type
    {
        Dictionary<TKey, TValue>? builder = null;
        foreach (var key in keys)
        {
            IReadOnlyDictionary<TKey, TValue> current = builder ?? (IReadOnlyDictionary<TKey, TValue>)source;
            if (!current.ContainsKey(key))
                continue;

            builder ??= ToBuilder(source);
            builder.Remove(key);
        }

        return builder is null ? source : builder.ToFrozenDictionary();
    }

    private static Dictionary<TKey, TValue> ToBuilder<TKey, TValue>(FrozenDictionary<TKey, TValue> source)
        where TKey : Type
    {
        var builder = new Dictionary<TKey, TValue>(source.Count);
        foreach (var item in source)
            builder.Add(item.Key, item.Value);
        return builder;
    }
}

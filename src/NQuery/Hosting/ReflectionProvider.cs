using System.Collections.Immutable;
using System.Reflection;
using System.Text;

using NQuery.Symbols;

namespace NQuery.Hosting
{
    public class ReflectionProvider : IPropertyProvider, IMethodProvider
    {
        public ReflectionProvider()
            : this(BindingFlags.Instance | BindingFlags.Public)
        {
        }

        public ReflectionProvider(BindingFlags bindingFlags)
        {
            BindingFlags = bindingFlags;
        }

        public BindingFlags BindingFlags { get; }

        private static bool ExistingMemberIsMoreSpecific(Type type, MemberInfo existingMember, MemberInfo newMember)
        {
            var existingMemberDeclaringType = existingMember.DeclaringType;
            var newMemberDeclaringType = newMember.DeclaringType;

            var existingMemberDistance = 0;
            var newMemberDistance = 0;

            while (existingMemberDeclaringType is not null && existingMemberDeclaringType != type)
            {
                existingMemberDistance++;
                existingMemberDeclaringType = existingMemberDeclaringType.BaseType;
            }

            while (newMemberDeclaringType is not null && newMemberDeclaringType != type)
            {
                newMemberDistance++;
                newMemberDeclaringType = newMemberDeclaringType.BaseType;
            }

            return existingMemberDistance > newMemberDistance;
        }

        private sealed class PropertyTable
        {
            private readonly Dictionary<string, Entry> _table = new();

            public class Entry
            {
                public Entry(PropertySymbol propertySymbol, MemberInfo memberInfo)
                {
                    PropertySymbol = propertySymbol;
                    MemberInfo = memberInfo;
                }

                public PropertySymbol PropertySymbol { get; }

                public MemberInfo MemberInfo { get; }
            }

            public void Add(PropertySymbol propertySymbol, MemberInfo memberInfo)
            {
                var entry = new Entry(propertySymbol, memberInfo);
                _table.Add(propertySymbol.Name, entry);
            }

            public void Remove(Entry entry)
            {
                _table.Remove(entry.PropertySymbol.Name);
            }

            public Entry this[string propertyName]
            {
                get
                {
                    _table.TryGetValue(propertyName, out var result);
                    return result;
                }
            }
        }

        private sealed class MethodTable
        {
            private readonly Dictionary<string, Entry> _table = new();

            public class Entry
            {
                public Entry(string key, MethodSymbol methodSymbol, MethodInfo methodInfo)
                {
                    MethodSymbol = methodSymbol;
                    MethodInfo = methodInfo;
                    Key = key;
                }

                public string Key { get; }

                public MethodSymbol MethodSymbol { get; }

                public MethodInfo MethodInfo { get; }
            }

            private static string GenerateKey(string methodName, IEnumerable<Type> parameterTypes)
            {
                var sb = new StringBuilder();
                sb.Append(methodName);
                sb.Append(@"(");

                var isFirst = true;

                foreach (var t in parameterTypes)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append(',');

                    sb.Append(t.Name);
                }

                sb.Append(@")");
                return sb.ToString();
            }

            public void Add(MethodSymbol methodSymbol, MethodInfo methodInfo)
            {
                var key = GenerateKey(methodSymbol.Name, methodSymbol.GetParameterTypes());
                var entry = new Entry(key, methodSymbol, methodInfo);
                _table.Add(entry.Key, entry);
            }

            public void Remove(Entry entry)
            {
                _table.Remove(entry.Key);
            }

            public Entry this[string methodName, IEnumerable<Type> parameterTypes]
            {
                get
                {
                    var key = GenerateKey(methodName, parameterTypes);
                    _table.TryGetValue(key, out var result);
                    return result;
                }
            }
        }

        private static void AddProperty(PropertyTable propertyTable, ICollection<PropertySymbol> memberList, Type declaringType, PropertySymbol memberBinding, MemberInfo memberInfo)
        {
            // Check if we already have a member with the same name declared.
            var existingMemberEntry = propertyTable[memberBinding.Name];

            if (existingMemberEntry is not null)
            {
                // OK we have one. Check if the existing member is not more specific.
                if (ExistingMemberIsMoreSpecific(declaringType, existingMemberEntry.MemberInfo, memberInfo))
                {
                    // The existing member is more specific. So we don't add the new one.
                    return;
                }

                // The new member is more specific. Remove the old one.
                propertyTable.Remove(existingMemberEntry);
                memberList.Remove(existingMemberEntry.PropertySymbol);
            }

            // Either the new member is more specific or we didn't have
            // a member with same name.
            propertyTable.Add(memberBinding, memberInfo);
            memberList.Add(memberBinding);
        }

        private static void AddMethod(MethodTable methodTable, ICollection<MethodSymbol> methodList, Type declaringType, MethodSymbol methodSymbol, MethodInfo methodInfo)
        {
            // Check if we already have a method with the same name and parameters declared.
            var existingMethodEntry = methodTable[methodSymbol.Name, methodSymbol.GetParameterTypes()];

            if (existingMethodEntry is not null)
            {
                // OK we have one. Check if the existing member is not more specific.
                if (ExistingMemberIsMoreSpecific(declaringType, existingMethodEntry.MethodInfo, methodInfo))
                {
                    // The existing member is more specific. So we don't add the new one.
                    return;
                }

                // The new member is more specific. Remove the old one.
                methodTable.Remove(existingMethodEntry);
                methodList.Remove(existingMethodEntry.MethodSymbol);
            }

            // Either the new member is more specific or we didn't have
            // a member with same name.
            methodTable.Add(methodSymbol, methodInfo);
            methodList.Add(methodSymbol);
        }

        public IEnumerable<PropertySymbol> GetProperties(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var propertyTable = new PropertyTable();
            var propertyList = new List<PropertySymbol>();

            // Convert CLR Properties

            var propertyInfos = type.GetProperties(BindingFlags);

            foreach (var currentPropertyInfo in propertyInfos)
            {
                // Ignore indexer
                var indexParameters = currentPropertyInfo.GetIndexParameters();
                if (indexParameters.Length > 0)
                    continue;

                var property = CreateProperty(currentPropertyInfo);
                if (property is not null)
                    AddProperty(propertyTable, propertyList, type, property, currentPropertyInfo);
            }

            // Convert CLR Fields

            var fieldInfos = type.GetFields(BindingFlags);

            foreach (var currentFieldInfo in fieldInfos)
            {
                var property = CreateProperty(currentFieldInfo);
                if (property is not null)
                    AddProperty(propertyTable, propertyList, type, property, currentFieldInfo);
            }

            return propertyList.ToImmutableArray();
        }

        /// <summary>
        /// Checks whether the given <see cref="MethodInfo"/> is invocable by the query engine, i.e. it can be used
        /// as <see cref="InvocableSymbol"/>.
        /// </summary>
        /// <remarks>
        /// A method cannot be invoked if any of the following is true:
        /// <ul>
        ///		<li><paramref name="methodInfo"/> has a special name (e.g. it is getter, setter, indexer or operator method)</li>
        ///		<li><paramref name="methodInfo"/> has abstract modifier</li>
        ///		<li><paramref name="methodInfo"/> has return type <see cref="Void"/></li>
        ///		<li><paramref name="methodInfo"/> has unsafe parameter types</li>
        ///		<li><paramref name="methodInfo"/> has dynamical argument lists (e.g. params modifier)</li>
        ///		<li><paramref name="methodInfo"/> has out or ref parameters</li>
        /// </ul>
        /// </remarks>
        /// <param name="methodInfo">The method info to check.</param>
        public static bool IsInvocable(MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo);

            if (methodInfo.IsSpecialName ||
                methodInfo.IsAbstract ||
                methodInfo.ReturnType == typeof(void) ||
                methodInfo.ReturnType.IsPointer ||
                (methodInfo.CallingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
                return false;

            var hasInvalidParameterTypes = (from parameterInfo in methodInfo.GetParameters()
                                            let hasParamsModifier = parameterInfo.GetCustomAttributes(typeof(ParamArrayAttribute), false).Any()
                                            where hasParamsModifier ||
                                                  parameterInfo.IsOut ||
                                                  parameterInfo.ParameterType.IsByRef ||
                                                  parameterInfo.ParameterType.IsPointer
                                            select parameterInfo).Any();

            return !hasInvalidParameterTypes;
        }

        public IEnumerable<MethodSymbol> GetMethods(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var methodTable = new MethodTable();
            var methodList = new List<MethodSymbol>();

            var methodInfos = type.GetMethods(BindingFlags);
            Array.Sort(methodInfos, (x, y) => string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));

            foreach (var currentMethodInfo in methodInfos)
            {
                if (!IsInvocable(currentMethodInfo))
                    continue;

                var methodSymbol = CreateMethod(currentMethodInfo);
                if (methodSymbol is not null)
                    AddMethod(methodTable, methodList, type, methodSymbol, currentMethodInfo);
            }

            return methodList.ToImmutableArray();
        }

        /// <summary>
        /// Creates a method binding for the given <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="methodInfo">The .NET method info.</param>
        /// <returns>If the method should not be visible this method returns <see langword="null"/>.</returns>
        protected virtual MethodSymbol CreateMethod(MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo);

            return new ReflectionMethodSymbol(methodInfo, methodInfo.Name);
        }

        /// <summary>
        /// Creates a property binding for the given <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="propertyInfo">The .NET property info.</param>
        /// <returns>If the property should not be visible this method returns <see langword="null"/>.</returns>
        protected virtual PropertySymbol CreateProperty(PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);

            return new ReflectionPropertySymbol(propertyInfo, propertyInfo.Name);
        }

        /// <summary>
        /// Creates a property binding for the given <see cref="FieldInfo"/>.
        /// </summary>
        /// <param name="fieldInfo">The .NET field info.</param>
        /// <returns>If the field should not be visible this method returns <see langword="null"/>.</returns>
        protected virtual PropertySymbol CreateProperty(FieldInfo fieldInfo)
        {
            ArgumentNullException.ThrowIfNull(fieldInfo);

            return new ReflectionFieldSymbol(fieldInfo, fieldInfo.Name);
        }
    }
}
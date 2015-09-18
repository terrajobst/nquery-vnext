using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public ReflectionProvider(BindingFlags bindingFlags)
        {
            BindingFlags = bindingFlags;
        }

        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public BindingFlags BindingFlags { get; private set; }

        private static bool ExistingMemberIsMoreSpecific(Type type, MemberInfo existingMember, MemberInfo newMember)
        {
            var existingMemberDeclaringType = existingMember.DeclaringType;
            var newMemberDeclaringType = newMember.DeclaringType;

            var existingMemberDistance = 0;
            var newMemberDistance = 0;

            while (existingMemberDeclaringType != null && existingMemberDeclaringType != type)
            {
                existingMemberDistance++;
                existingMemberDeclaringType = existingMemberDeclaringType.BaseType;
            }

            while (newMemberDeclaringType != null && newMemberDeclaringType != type)
            {
                newMemberDistance++;
                newMemberDeclaringType = newMemberDeclaringType.BaseType;
            }

            return existingMemberDistance > newMemberDistance;
        }

        private class PropertyTable
        {
            private readonly Dictionary<string, Entry> _table = new Dictionary<string, Entry>();

            public class Entry
            {
                public Entry(PropertySymbol propertySymbol, MemberInfo memberInfo)
                {
                    PropertySymbol = propertySymbol;
                    MemberInfo = memberInfo;
                }

                public PropertySymbol PropertySymbol { get; private set; }

                public MemberInfo MemberInfo { get; private set; }
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
                    Entry result;
                    _table.TryGetValue(propertyName, out result);
                    return result;
                }
            }
        }

        private class MethodTable
        {
            private readonly Dictionary<string, Entry> _table = new Dictionary<string, Entry>();

            public class Entry
            {
                public Entry(string key, MethodSymbol methodSymbol, MethodInfo methodInfo)
                {
                    MethodSymbol = methodSymbol;
                    MethodInfo = methodInfo;
                    Key = key;
                }

                public string Key { get; private set; }

                public MethodSymbol MethodSymbol { get; private set; }

                public MethodInfo MethodInfo { get; private set; }
            }

            private static string GenerateKey(string methodName, IEnumerable<Type> parameterTypes)
            {
                var sb = new StringBuilder();
                sb.Append(methodName);
                sb.Append("(");

                var isFirst = true;

                foreach (var t in parameterTypes)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append(',');

                    sb.Append(t.Name);
                }

                sb.Append(")");
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
                    Entry result;
                    _table.TryGetValue(key, out result);
                    return result;
                }
            }
        }

        private static void AddProperty(PropertyTable propertyTable, ICollection<PropertySymbol> memberList, Type declaringType, PropertySymbol memberBinding, MemberInfo memberInfo)
        {
            // Check if we already have a member with the same name declared.
            var exisitingMemberEntry = propertyTable[memberBinding.Name];

            if (exisitingMemberEntry != null)
            {
                // Ok we have one. Check if the existing member is not more specific.
                if (ExistingMemberIsMoreSpecific(declaringType, exisitingMemberEntry.MemberInfo, memberInfo))
                {
                    // The existing member is more specific. So we don't add the new one.
                    return;
                }

                // The new member is more specific. Remove the old one.
                propertyTable.Remove(exisitingMemberEntry);
                memberList.Remove(exisitingMemberEntry.PropertySymbol);
            }

            // Either the new member is more specific or we don't had
            // a member with same name.
            propertyTable.Add(memberBinding, memberInfo);
            memberList.Add(memberBinding);
        }

        private static void AddMethod(MethodTable methodTable, ICollection<MethodSymbol> methodList, Type declaringType, MethodSymbol methodSymbol, MethodInfo methodInfo)
        {
            // Check if we already have a method with the same name and parameters declared.
            var exisitingMethodEntry = methodTable[methodSymbol.Name, methodSymbol.GetParameterTypes()];

            if (exisitingMethodEntry != null)
            {
                // Ok we have one. Check if the existing member is not more specific.
                if (ExistingMemberIsMoreSpecific(declaringType, exisitingMethodEntry.MethodInfo, methodInfo))
                {
                    // The existing member is more specific. So we don't add the new one.
                    return;
                }

                // The new member is more specific. Remove the old one.
                methodTable.Remove(exisitingMethodEntry);
                methodList.Remove(exisitingMethodEntry.MethodSymbol);
            }

            // Either the new member is more specific or we don't had
            // a member with same name.
            methodTable.Add(methodSymbol, methodInfo);
            methodList.Add(methodSymbol);
        }

        public IEnumerable<PropertySymbol> GetProperties(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

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
                if (property != null)
                    AddProperty(propertyTable, propertyList, type, property, currentPropertyInfo);
            }

            // Convert CLR Fields

            var fieldInfos = type.GetFields(BindingFlags);

            foreach (var currentFieldInfo in fieldInfos)
            {
                var property = CreateProperty(currentFieldInfo);
                if (property != null)
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
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");

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
            if (type == null)
                throw new ArgumentNullException("type");

            var methodTable = new MethodTable();
            var methodList = new List<MethodSymbol>();

            var methodInfos = type.GetMethods(BindingFlags);
            Array.Sort(methodInfos, (x, y) => string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));

            foreach (var currentMethodInfo in methodInfos)
            {
                if (!IsInvocable(currentMethodInfo))
                    continue;

                var methodSymbol = CreateMethod(currentMethodInfo);
                if (methodSymbol != null)
                    AddMethod(methodTable, methodList, type, methodSymbol, currentMethodInfo);
            }

            return methodList.ToImmutableArray();
        }

        /// <summary>
        /// Creates a method binding for the given <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="methodInfo">The .NET framework property info.</param>
        /// <returns>If the property should not be visible this method returns <see langword="null"/>.</returns>
        protected virtual MethodSymbol CreateMethod(MethodInfo methodInfo)
        {
            return new ReflectionMethodSymbol(methodInfo, methodInfo.Name);
        }

        /// <summary>
        /// Creates a property binding for the given <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="propertyInfo">The .NET framework property info.</param>
        /// <returns>If the property should not be visible this method returns <see langword="null"/>.</returns>
        protected virtual PropertySymbol CreateProperty(PropertyInfo propertyInfo)
        {
            return new ReflectionPropertySymbol(propertyInfo, propertyInfo.Name);
        }

        /// <summary>
        /// Creates a property binding for the given <see cref="FieldInfo"/>.
        /// </summary>
        /// <param name="fieldInfo">The .NET framework field info.</param>
        /// <returns>If the field should not be visible this method returns <see langword="null"/>.</returns>
        protected virtual PropertySymbol CreateProperty(FieldInfo fieldInfo)
        {
            return new ReflectionFieldSymbol(fieldInfo, fieldInfo.Name);
        }
    }
}
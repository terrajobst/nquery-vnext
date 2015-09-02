using System;
using System.Linq;
using System.Reflection;

using NQuery;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main()
        {
            var assembly = typeof (SyntaxNode).Assembly;
            var nodes = assembly.GetExportedTypes().Where(t => typeof (SyntaxNode).IsAssignableFrom(t)).ToArray();

            var typesByBaseType = nodes.ToLookup(n => n.BaseType);
            var roots = nodes.Where(n => n != typeof(SyntaxNode) && (n.IsAbstract || n.BaseType == typeof(SyntaxNode))).OrderBy(n => n.Name);

            foreach (var node in roots)
            {
                PrintProductionName(node);

                if (!node.IsAbstract)
                    PrintProduction(node);

                var derivedTypes = typesByBaseType[node];

                foreach (var derivedType in derivedTypes)
                {
                    if (derivedType.IsAbstract)
                    {
                        PrintProductionNameReference(derivedType);
                    }
                    else
                    {
                        PrintProduction(derivedType);
                    }
                }

                Console.WriteLine();
            }
        }

        private static string GetProductionName(Type type)
        {
            return type.Name.Replace("Syntax", "");
        }

        private static void PrintProductionName(Type type)
        {
            Console.WriteLine("{0}:", GetProductionName(type));
        }

        private static void PrintProductionNameReference(Type type)
        {
            Console.WriteLine("\t{0}", GetProductionName(type));
        }

        private static void PrintProduction(Type type)
        {
            var constructor = type.GetConstructors().Single();
            var names = constructor.GetParameters().Skip(1).Select(GetElementName);
            var nameList = string.Join(" ", names);
            Console.WriteLine("\t{0}", nameList);
        }

        private static string GetElementName(ParameterInfo p)
        {
            if (p.ParameterType == typeof (SyntaxToken))
                return p.Name.Replace("Keyword", "").Replace("Token", "").ToUpper();

            if (typeof (SyntaxNode).IsAssignableFrom(p.ParameterType))
                return GetProductionName(p.ParameterType);

            return p.Name;
        }
    }
}

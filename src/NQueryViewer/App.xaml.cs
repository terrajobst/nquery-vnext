using System.Collections.Immutable;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Reflection;
using System.Windows;

namespace NQueryViewer
{
    internal sealed partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var paths = new[]
            {
                GetApplicationPath(),
                GetApplicationDirectoryPath(),
            };

            var aggregateCatalog = GetCatalog(paths);
            var compositionContainer = new CompositionContainer(aggregateCatalog);
            var mainWindowProvider = compositionContainer.GetExportedValue<IMainWindowProvider>();
            var mainWindow = mainWindowProvider.Window;
            mainWindow.Show();
        }

        public static ComposablePartCatalog GetCatalog(IEnumerable<string> paths)
        {
            var directoryPaths = from p in paths
                                 where Directory.Exists(p)
                                 select p;

            var filePaths = from p in paths
                            where File.Exists(p)
                            select p;

            var expandedDirectoryFilePaths = from dp in directoryPaths
                                             from p in Directory.GetFiles(dp, "*.dll")
                                             select p;

            var pathSet = new SortedSet<string>(filePaths.Concat(expandedDirectoryFilePaths),
                                                StringComparer.OrdinalIgnoreCase);

            var uniqueAssemblyPaths = (from p in pathSet
                                       let a = TryGetAssemblyName(p)
                                       where a is not null && !a.Name.StartsWith("xunit", StringComparison.OrdinalIgnoreCase)
                                       let t = Tuple.Create(a.FullName, p)
                                       group t by t.Item1
                                           into g
                                           let p = g.First().Item2
                                           orderby p
                                           select p).ToImmutableArray();

            var assemblyCatalogs = from a in uniqueAssemblyPaths
                                   select new AssemblyCatalog(a);

            return new AggregateCatalog(assemblyCatalogs);
        }

        private static AssemblyName TryGetAssemblyName(string path)
        {
            try
            {
                return AssemblyName.GetAssemblyName(path);
            }
            catch
            {
                return null;
            }
        }

        private static string GetApplicationPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        private static string GetApplicationDirectoryPath()
        {
            return Path.GetDirectoryName(GetApplicationPath());
        }
    }
}
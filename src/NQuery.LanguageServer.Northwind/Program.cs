using NQuery.Authoring.LanguageServer;
using NQuery.LanguageServer.Northwind;

// stdout is the LSP transport: a stray Console.Write anywhere in the process would corrupt the
// message stream, so the ambient Console.Out is repointed at stderr before the server starts.
var transport = Console.OpenStandardOutput();
Console.SetOut(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });

await NQueryLanguageServer.Create(options =>
    {
        options.ServerName = @"NQuery Northwind Language Server";
        options.CatalogProviderFactory = context => new NorthwindCatalogProvider(context);
    })
    .RunAsync(Console.OpenStandardInput(), transport);

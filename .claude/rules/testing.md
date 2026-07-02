# Running tests

Test projects use the **xUnit v3 in-process runner** (Microsoft.Testing.Platform),
not classic `dotnet test`/VSTest. This changes how filtering and flags work — the
gotchas below are the ones worth remembering.

Test projects:
- `src/NQuery.Tests`
- `src/NQuery.Authoring.Tests`

Each multi-targets `net8.0` and `net481`, built to:
`artifacts/bin/<Project>/debug_<tfm>/<Project>.exe`

### Run the whole suite

```
dotnet test --project src/NQuery.Tests/NQuery.Tests.csproj
```

- Use `--project`; a positional project path errors ("Specifying a project ...
  should be via '--project'").
- Do **not** append classic flags like `--nologo` or `--filter` here. `dotnet
  test` forwards unknown args to the test app, which rejects them ("Unknown
  option '--nologo'") and reports `Zero tests ran` / exit code 5 — which looks
  like a filter miss but isn't.

### Filter tests (the reliable way)

Run the built test **exe directly** and use xUnit's native single-dash filters.
Build first (`dotnet build src/NQuery.Tests/NQuery.Tests.csproj`), then:

```
artifacts/bin/NQuery.Tests/debug_net8.0/NQuery.Tests.exe -method "*Subquery*"
```

Native filter options (wildcard `*` allowed at start and/or end; repeating a
positive filter is OR, a negative `-` filter is AND):

- `-method "*Subquery*"`        — fully-qualified `Namespace.Class.Method`
- `-class  "*.AlgebrizerTests"` — fully-qualified type name
- `-namespace "NQuery.*"`
- `-trait "name=value"`
- `-method-` / `-class-` / `-namespace-` — exclusions
- `-filter "/asm/namespace/class/method[trait=value]"` — combined query-filter
  language

`-` prefixes are native xUnit options (not `--`). Run the exe in the **Bash tool**
and append `2>&1 | tail -n N` to trim the run banner.

# NQuery for VS Code

Language support for NQuery documents:

| Extension | Document kind |
| --------- | ------------- |
| `.nql`    | Query         |
| `.nqe`    | Expression    |

To use `.sql` files as well, map them with VS Code's built-in setting:

```jsonc
"files.associations": { "queries/**/*.sql": "nquery" }
```

## File icons

The extension contributes a database glyph for `.nql` and `.nqe`, so they get a real icon in the
explorer instead of the blank default page. It is [VS Code's own `database` codicon](https://github.com/microsoft/vscode-codicons)
rather than custom artwork, painted in Seti's SQL colors so it sits with its neighbours under the
default theme — see `icons/README.md`.

It is a **fallback**, and deliberately the lowest-priority one. VS Code gives a theme's
file-extension rules an extra CSS class over a language icon, so any icon theme that has an icon
for `.nql` wins and this one never appears.

Where a theme *can* be taught, teach it rather than relying on the fallback: NQuery files are
SQL-shaped, so pointing a theme at its own `.sql` icon gets a glyph drawn in that theme's style
instead of an outsider. This needs no files from you — both common configurable themes are set up
in this repository's `.vscode/settings.json` already:

```jsonc
// Material Icon Theme -- "database" is the icon it gives .sql
"material-icon-theme.files.associations": { "*.nql": "database", "*.nqe": "database" },

// vscode-icons -- "sql" is file_type_sql.svg; then run "Icons: Apply Icons Customization"
"vsicons.associations.files": [{ "icon": "sql", "extensions": ["nql", "nqe"], "format": "svg" }]
```

The same trick does **not** work for Seti, the default theme, which is why the fallback exists at
all. Seti has no setting to configure, and an extension cannot add associations to a theme it does
not own. Nor can the extension quietly borrow Seti's SQL glyph: `getIconClasses` derives exactly
one language class per file, from the language VS Code detects for that path, so Seti's
`sql-lang-file-icon` rule is reachable only by making VS Code believe `.nql` *is* SQL — which
costs the NQuery grammar and the language server. (Seti's SQL icon is also a character in a font
rather than an image, so there is nothing to point at even in principle.)

`.nql` and `.nqe` necessarily share the icon: an icon is contributed per *language*, and both are
the one `nquery` language. Distinguishing them would mean splitting the language in two, which
would cost them a shared grammar and break every `editorLangId == nquery` condition — far more
than an icon is worth. An icon theme configured as above can tell them apart, since it matches on
the extension.

`.nqproj` files already show the JSON icon under most themes, because the extension maps them to
the `jsonc` language and themes map that.

## Project files

NQuery's catalog is defined in code, so there is no single language server this extension can
ship. Instead, each application provides its own server executable, and a **project file** tells
the extension how to launch it.

A `.nqproj` file owns every `.nql`/`.nqe` file in its folder and all subfolders:

```jsonc
// warehouse.nqproj
{
  "$schema": "https://raw.githubusercontent.com/terrajobst/nquery-vnext/main/schemas/nqproj-1.json",
  "version": 1,
  "host": {
    "command": "dotnet",
    "args": ["${projectDir}/tools/WarehouseLsp.dll"],
    "env": { "WAREHOUSE_ENV": "staging" }
  },
  "settings": {
    "connection": "Server=staging-db;Database=Warehouse"
  }
}
```

`host` and `documents` ownership are the extension's business. **`settings` is opaque** — the
extension never inspects it and forwards it verbatim to the server as `initializationOptions`,
so an application can put whatever it needs in there.

Variables available in `command`, `args`, `cwd` and `env`: `${projectDir}`, `${workspaceFolder}`,
`${userHome}`, `${env:NAME}`.

### Ownership rules

- **One project per folder.** If a folder contains several `.nqproj` files, the alphabetically
  first one is used and the rest are ignored with a warning.
- **Projects cannot be nested.** A project inside another project's folder is an error and does
  not run. Its subtree stays unowned rather than being served from the outer project's catalog,
  which would silently be the wrong schema.

Together these guarantee that a file has at most one owning project, and that an open document's
owner never silently changes to a different project.

Files not covered by any project get no language server unless `nquery.defaultHost.command` is
configured.

## Remote workspaces

The extension declares `"extensionKind": ["workspace"]`, so in Remote-SSH, WSL, Dev Containers and
Codespaces it runs on the remote side — which is what you want, since the language server should
run where the queries and the catalog are. Path comparison and process launching then use the
remote machine's rules rather than your local ones.

The consequence is that **the .NET runtime and the server executable must exist on the remote**. A
project file's `host.command` is resolved there, so a dev container or Codespace needs the SDK in
its image and the host built inside it.

Virtual workspaces (vscode.dev, github.dev, Remote Repositories) are **not** supported: there is no
filesystem and no way to start a process, so no project can be found and no server can run. The
extension declares this, so VS Code says so rather than appearing to load and doing nothing.

## Workspace trust

A project file names an executable to launch. In an untrusted workspace the extension provides
syntax highlighting and project validation but starts no server processes.

## Settings

| Setting                      | Description                                 |
| ---------------------------- | ------------------------------------------- |
| `nquery.defaultHost.command` | Server for files not covered by any project |
| `nquery.defaultHost.args`    | Arguments for that server                   |
| `nquery.trace.server`        | `off` / `messages` / `verbose`              |

## Code actions

The lightbulb offers NQuery's quick fixes and refactorings — adding a missing `AS`, expanding a
`*`, rewriting `= NULL` as `IS NULL`, qualifying a column, and the rest. Quick fixes also appear
under **Quick Fix…** on a squiggle, refactorings under **Refactor…**.

## Running queries

| Command                         | Keybinding     | What it does                                          |
| ------------------------------- | -------------- | ----------------------------------------------------- |
| **NQuery: Run Query**           | `Ctrl+Shift+E` | Runs the active document and shows the rows in a grid |
| **NQuery: Show Execution Plan** | `Ctrl+Alt+P`   | Shows the optimizer pipeline as a tree                |

Both work on `.nql` and `.nqe` files, and both use the editor's current text — you do not have to
save first.

The plan panel has a step selector covering the whole pipeline: the unoptimized logical tree, one
entry per optimization pass that changed it, the optimized tree, and the physical plan that
actually runs. It opens on the physical plan.

Results are capped by `nquery.results.maxRows` (default 1000); the server enforces its own cap as
well, so this can only lower it. Binary columns render as `byte[1234]` rather than being
transferred.

### Copying and exporting results

| Command | Format |
| ------- | ------ |
| **NQuery: Copy Results** | TSV — pastes into Excel, Sheets and Notion with columns intact |
| **NQuery: Copy Results as Markdown** | Markdown table, for issues and docs |
| **NQuery: Export Results...** | CSV, TSV or Markdown to a file |

They live in the results panel's title bar — save and copy as icons, "Copy Results as Markdown"
in the `...` overflow — and in the Command Palette once a query has produced results.

CSV is written as UTF-8 **with** a byte order mark and CRLF line endings, because Excel on Windows
mis-decodes UTF-8 without one. Values containing the delimiter, a quote or a line break are quoted
per RFC 4180, so a company name with a comma cannot shift the remaining columns.

Two things to know about what you get:

- **NULL exports as an empty field**, which is what Excel expects but reads the same as an empty
  string. Set `nquery.export.nullText` if you need them distinguishable.
- **Binary columns export as the `byte[1234]` placeholder**, not the bytes. An export is a view of
  the grid, not a data dump.

Exporting a truncated result asks for confirmation first, since nothing in the resulting file says
it is partial. Set `nquery.export.csvDelimiter` to `;` for locales where Excel expects it.

There is deliberately no JSON export. Result cells arrive from the server as display strings, so
JSON would quote everything including numbers — data that looks typed and is not. That needs typed
values from the server rather than a guess in the client.

`NQuery: Export Results...` also accepts arguments, so it can be bound to a keybinding that always
writes the same format:

```jsonc
{
  "key": "ctrl+alt+e",
  "command": "nquery.exportResults",
  "args": { "format": "csv" }
}
```

A host can refuse to run queries — appropriate when the catalog is backed by production data or
carries schema without rows. When it does, both commands disappear from the palette, the editor
title bar, and their keybindings.

## Development

```
npm install
npm run compile
npm test                  # unit tests, then integration tests
npm run test:unit         # fast; no VS Code involved
npm run test:integration  # launches a real VS Code
```

**Unit tests** (`tests/unit`, `node:test`) cover the logic that has no business touching the
editor: the rules deciding which project owns a file (`projectRules.ts`) and the webview HTML
generation (`render.ts`). Both are deliberately free of any `vscode` dependency so they can be
tested directly rather than through a mocked editor API; `projects.ts`, `resultsPanel.ts` and
`planPanel.ts` are thin adapters over them.

**Integration tests** (`tests/integration`, `@vscode/test-cli`) cover what unit tests cannot —
that the extension really discovers projects, launches the executable a project file names, and
gets diagnostics back. They run in two workspaces: `samples/northwind` for the end-to-end path,
and `tests/fixtures/projects` — deliberately misconfigured with a duplicate and a nested project
— for the validation diagnostics.

They need the Northwind host built first:

```
dotnet build ../src/NQuery.LanguageServer.Northwind
```

### Dependency overrides

`package.json` pins `diff` and `serialize-javascript` through `overrides`. Both arrive
transitively via `mocha`, which `@vscode/test-cli` uses to run the integration tests, and both
carry advisories. There is no released `mocha` that avoids them — the advisories span every 11.x —
so `npm audit fix --force` only *downgrades* `mocha`, leaving the vulnerable versions in place
while also giving up the newer runner. The overrides pin the fixed transitive versions instead.

Both are test-only and never ship: `vsce package` bundles production dependencies, so the packaged
extension contains neither. Drop the overrides once `mocha` ships updated bounds.

Run `npm audit` from `vscode/`, not the repository root — there is no `package.json` at the root,
which is what `ENOLOCK` means if you see it.

### Repository scripts

The repository scripts cover the extension too: `build.cmd` compiles it after the .NET build, and
`test.cmd` runs the unit tests after the .NET tests. Integration tests are opt-in there via
`test.cmd -full`, because the first run downloads VS Code. Both scripts skip the extension with a
message when `npm` is not on PATH, so working on the query engine alone does not require Node.

## Commands

- **NQuery: Run Query**
- **NQuery: Show Execution Plan**
- **NQuery: Restart Server**
- **NQuery: Show Output**
- **NQuery: Reload Catalog** — asks the server to re-resolve its catalog

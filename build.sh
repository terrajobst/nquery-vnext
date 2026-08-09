#!/usr/bin/env bash

set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# The differential tests replay queries against the engines in external/. They compile out when
# those are missing, so restoring and building them here is what keeps them running: a stale
# checkout costs coverage silently rather than loudly.
if [[ -f "$script_dir/.gitmodules" ]] && command -v git > /dev/null 2>&1; then
    git -C "$script_dir" submodule update --init external/nquery-baseline external/nquery-old ||
        echo "Could not restore the submodules; the differential tests will compile out."
fi

# Only the baseline is built: OldEngineDefinitionTests reads the old engine's test definitions as
# files, and the old engine's assembly is needed by the benchmarks alone. Debug rather than
# whatever "$@" asks for, because the test project references artifacts/bin/NQuery/debug_net8.0.
# Building NQuery.Data builds NQuery with it, which is the other half of the "baseline" alias.
if [[ -f "$script_dir/external/nquery-baseline/src/NQuery.Data/NQuery.Data.csproj" ]]; then
    echo
    echo "Building the baseline engine..."
    dotnet build "$script_dir/external/nquery-baseline/src/NQuery.Data" -c Debug --nologo
    echo
fi

dotnet build "$script_dir/NQuery.slnx" -t build -t pack --nologo "$@"

# The VS Code extension is a separate toolchain, so it is skipped rather than failed when npm
# is absent -- working on the query engine alone should not require Node.
if ! command -v npm > /dev/null 2>&1; then
    echo
    echo "Skipping the VS Code extension: npm was not found on PATH."
    exit 0
fi

echo
echo "Building the VS Code extension..."

cd "$script_dir/vscode"

# Only on a cold clone: restoring on every build would dominate an otherwise incremental one.
if [[ ! -d node_modules ]]; then
    npm ci
fi

npm run compile

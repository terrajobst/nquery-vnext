#!/usr/bin/env bash

set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# The VS Code integration tests launch a real VS Code, and the first run downloads it (about a
# gigabyte), so they are opt-in locally. CI runs them on every push regardless.
run_integration=
if [[ "${1:-}" == "-full" ]]; then
    run_integration=1
fi

dotnet test --solution "$script_dir/NQuery.slnx"

if ! command -v npm > /dev/null 2>&1; then
    echo
    echo "Skipping the VS Code extension tests: npm was not found on PATH."
    exit 0
fi

echo
echo "Testing the VS Code extension..."

cd "$script_dir/vscode"

if [[ ! -d node_modules ]]; then
    npm ci
fi

npm run test:unit

if [[ -n "$run_integration" ]]; then
    # A real VS Code needs a display; on a headless Linux box this has to run under xvfb-run.
    npm run test:integration
else
    echo
    echo "Skipping the VS Code integration tests. Run \"test.sh -full\" to include them."
fi

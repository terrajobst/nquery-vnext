import { defineConfig } from '@vscode/test-cli';

// Two workspaces, because the things worth testing end-to-end need different ones: the sample
// exercises a real server over a real catalog, while the fixture exercises the project rules that
// only fire when a workspace is misconfigured.
//
// --disable-workspace-trust matters: the extension deliberately starts no servers in an untrusted
// workspace, so without it every server-dependent assertion would fail for the wrong reason.
const launchArgs = ['--disable-workspace-trust'];

export default defineConfig([
    {
        label: 'sample',
        files: 'out/tests/integration/sample.test.js',
        workspaceFolder: '../samples/northwind',
        launchArgs,
        // The first run downloads VS Code, and the server has to build its catalog before the
        // first diagnostics arrive.
        mocha: { timeout: 120000 }
    },
    {
        label: 'projects',
        files: 'out/tests/integration/projects.test.js',
        workspaceFolder: './tests/fixtures/projects',
        launchArgs,
        mocha: { timeout: 120000 }
    }
]);

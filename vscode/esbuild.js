// Bundles the extension into a single file that `main` points at. Without this the VSIX carries
// node_modules as loose files -- vscode-languageclient and the protocol packages alone are most of
// the package -- and VS Code pays for resolving all of them on every activation.
//
// This does no type checking: esbuild strips types rather than understanding them. `npm run
// compile` is what type checks, and vscode:prepublish runs it first for exactly that reason.

const esbuild = require('esbuild');

const production = process.argv.includes('--production');
const watch = process.argv.includes('--watch');

/** @type {import('esbuild').BuildOptions} */
const options = {
    entryPoints: ['src/extension.ts'],
    bundle: true,
    outfile: 'dist/extension.js',

    // VS Code loads the extension with require() in a Node host, and supplies the `vscode` module
    // itself -- bundling it is both impossible and unnecessary.
    platform: 'node',
    format: 'cjs',
    external: ['vscode'],

    // Matches the Node in VS Code 1.85, which is what `engines.vscode` promises to run on.
    target: 'node18',

    // Prefer ESM entry points over CommonJS ones. jsonc-parser's `main` is a UMD build whose
    // inner require() is the factory's own parameter, so esbuild cannot see through it and leaves
    // a runtime require('./impl/format') in the output -- which then fails to resolve, because a
    // bundled VSIX ships no node_modules. Its `module` entry is plain ESM and bundles cleanly.
    mainFields: ['module', 'main'],

    sourcemap: !production,
    minify: production,
    logLevel: 'info'
};

async function main() {
    if (watch) {
        const context = await esbuild.context(options);
        await context.watch();
        return;
    }

    await esbuild.build(options);
}

main().catch(error => {
    console.error(error);
    process.exit(1);
});

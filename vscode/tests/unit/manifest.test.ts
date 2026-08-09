import * as assert from 'node:assert/strict';
import * as fs from 'node:fs';
import * as path from 'node:path';
import { describe, it } from 'node:test';

// These declarations have no runtime behaviour inside the extension, so nothing else would notice
// if an edit dropped them -- the failure mode is a silently wrong experience in a remote or
// virtual workspace, months later. Asserting them here is the only thing that keeps them honest.
const manifest = JSON.parse(
    fs.readFileSync(path.join(__dirname, '..', '..', '..', 'package.json'), 'utf8')) as {
        main?: string;
        extensionKind?: string[];
        capabilities?: {
            untrustedWorkspaces?: { supported?: boolean | string; description?: string };
            virtualWorkspaces?: { supported?: boolean; description?: string };
        };
        contributes?: {
            commands?: { command: string; icon?: string }[];
            languages?: { id: string; icon?: { light?: string; dark?: string } }[];
            menus?: { 'editor/title'?: { command: string; when?: string; group?: string }[] };
        };
    };

describe('manifest', () => {
    it('runs on the workspace side', () => {
        // The extension spawns the language server and compares paths using the host's platform
        // rules. On the UI side against a remote workspace both would be wrong -- local Windows
        // path semantics applied to remote Linux paths -- and wrong quietly rather than loudly.
        assert.deepEqual(manifest.extensionKind, ['workspace']);
    });

    it('declares that virtual workspaces are unsupported', () => {
        const virtualWorkspaces = manifest.capabilities?.virtualWorkspaces;

        // vscode.dev and Remote Repositories have no filesystem and cannot start a process, so
        // the extension would find no projects and start no servers. Declaring it makes VS Code
        // say so instead of leaving the user with a silent no-op.
        assert.equal(virtualWorkspaces?.supported, false);
        assert.ok(virtualWorkspaces?.description, 'a reason is shown to the user, so it must be set');
    });

    it('declares limited support in untrusted workspaces', () => {
        const untrusted = manifest.capabilities?.untrustedWorkspaces;

        assert.equal(untrusted?.supported, 'limited');
        assert.ok(untrusted?.description);
    });

    it('points main at the compiled entry point', () => {
        // tests/ compiles alongside src/, so main carries the extra src segment. Getting this
        // wrong breaks activation only once packaged.
        assert.equal(manifest.main, './out/src/extension.js');

        const entry = path.join(__dirname, '..', '..', '..', 'out', 'src', 'extension.js');
        assert.ok(fs.existsSync(entry), `${entry} does not exist; the manifest points at nothing`);
    });

    it('points the language icon at files that exist', () => {
        // This icon only shows under icon themes that have never heard of .nql -- which is every
        // theme until one adds a mapping -- so a broken path degrades to the blank default file
        // icon rather than to an error. Nobody would notice for a long time.
        const nquery = (manifest.contributes?.languages ?? []).find(l => l.id === 'nquery');
        const root = path.join(__dirname, '..', '..', '..');

        for (const kind of ['light', 'dark'] as const) {
            const declared = nquery?.icon?.[kind];
            assert.ok(declared, `the ${kind} language icon is not declared`);

            const file = path.join(root, declared);
            assert.ok(fs.existsSync(file), `${declared} does not exist`);

            // Both are the same codicon recolored to Seti's SQL colors, so a copy-paste that
            // leaves one holding the other's fill is the likely mistake -- and it is invisible
            // until you switch themes and find the icon no longer matches its neighbours.
            const fill = kind === 'light' ? '#dd4b78' : '#f55385';
            assert.match(fs.readFileSync(file, 'utf8'), new RegExp(`fill="${fill}"`, 'i'),
                `${declared} is the icon for ${kind} themes, so it must be filled ${fill}`);
        }
    });

    it('surfaces the result commands on the results panel itself', () => {
        // The palette alone is not discoverable: these commands only appear once a query has run,
        // so someone looking at a grid of results has no way to find out they exist.
        const titleMenu = manifest.contributes?.menus?.['editor/title'] ?? [];
        const onResultsPanel = titleMenu.filter(m => m.when === "activeWebviewPanelId == 'nquery.results'");

        assert.deepEqual(
            onResultsPanel.map(m => m.command).sort(),
            ['nquery.copyResults', 'nquery.copyResultsAsMarkdown', 'nquery.exportResults']);

        // The two shown as icons need one; the third falls into the overflow menu, where the
        // title is what is read.
        const icons = new Map((manifest.contributes?.commands ?? []).map(c => [c.command, c.icon]));

        for (const entry of onResultsPanel.filter(m => m.group?.startsWith('navigation'))) {
            assert.ok(icons.get(entry.command), `${entry.command} is a toolbar icon but declares none`);
        }
    });

    it('keeps the command declarations and the registrations in step', () => {
        const declared = (manifest.contributes?.commands ?? []).map(c => c.command).sort();

        assert.deepEqual(declared, [
            'nquery.copyResults',
            'nquery.copyResultsAsMarkdown',
            'nquery.execute',
            'nquery.exportResults',
            'nquery.reloadCatalog',
            'nquery.restartServer',
            'nquery.showOutput',
            'nquery.showPlan'
        ]);
    });
});

import * as assert from 'node:assert/strict';
import * as vscode from 'vscode';

import { activateExtension, fileInWorkspace, waitFor } from './helpers';

// The unit tests cover the ownership rules as pure functions. This covers the part they cannot:
// that the extension actually globs the workspace, runs those rules over what it finds, and
// surfaces the verdict as diagnostics VS Code will show in the Problems panel.
//
// The fixture workspace is deliberately misconfigured -- a.nqproj and b.nqproj share a folder,
// and nested/nested.nqproj sits inside a.nqproj's.
suite('project validation', () => {
    suiteSetup(async () => {
        await activateExtension();
    });

    function projectDiagnostics(uri: vscode.Uri): vscode.Diagnostic[] {
        return vscode.languages.getDiagnostics(uri).filter(d => d.source === 'nquery');
    }

    test('warns on every project file in a folder that has more than one', async () => {
        const a = fileInWorkspace('a.nqproj');
        const b = fileInWorkspace('b.nqproj');

        const onA = await waitFor('a warning on a.nqproj', () => {
            const found = projectDiagnostics(a);
            return found.length > 0 ? found : undefined;
        });

        const onB = await waitFor('a warning on b.nqproj', () => {
            const found = projectDiagnostics(b);
            return found.length > 0 ? found : undefined;
        });

        // Both are flagged, not just the loser, so the problem is visible from either file.
        assert.equal(onA[0].severity, vscode.DiagnosticSeverity.Warning);
        assert.equal(onB[0].severity, vscode.DiagnosticSeverity.Warning);

        // The alphabetically first one wins, and both messages say so.
        assert.match(onA[0].message, /'a\.nqproj' is used/);
        assert.match(onB[0].message, /'a\.nqproj' is used/);
        assert.match(onB[0].message, /this project is ignored/);
    });

    test('errors on a nested project and points at the containing one', async () => {
        const nested = fileInWorkspace('nested', 'nested.nqproj');

        const diagnostics = await waitFor('an error on the nested project', () => {
            const found = projectDiagnostics(nested);
            return found.length > 0 ? found : undefined;
        });

        const error = diagnostics[0];
        assert.equal(error.severity, vscode.DiagnosticSeverity.Error);
        assert.match(error.message, /cannot be nested/);

        // The related information is what makes the error actionable -- it links to the project
        // this one is sitting inside.
        assert.ok(error.relatedInformation && error.relatedInformation.length > 0);
        assert.match(error.relatedInformation[0].location.uri.fsPath, /a\.nqproj$/);
    });

    test('does not flag the winning project as nested', async () => {
        const a = fileInWorkspace('a.nqproj');

        const diagnostics = await waitFor('diagnostics on a.nqproj', () => {
            const found = projectDiagnostics(a);
            return found.length > 0 ? found : undefined;
        });

        assert.ok(!diagnostics.some(d => /cannot be nested/.test(d.message)));
    });
});

import * as assert from 'node:assert/strict';
import * as vscode from 'vscode';

import { activateExtension, fileInWorkspace, openDocument, serverDiagnostics, waitFor } from './helpers';

/** Counts RFC 4180 fields, so a quoted comma inside a value is not miscounted as a separator. */
function countFields(line: string): number {
    let fields = 1;
    let inQuotes = false;

    for (let i = 0; i < line.length; i++) {
        if (line[i] === '"') {
            inQuotes = !inQuotes;
        } else if (line[i] === ',' && !inQuotes) {
            fields++;
        }
    }

    return fields;
}

// Runs against samples/northwind with the real Northwind host, so a pass here means the whole
// chain works: project discovery, launching the executable named in the .nqproj, the LSP
// handshake, catalog resolution, and diagnostics coming back. None of that is reachable from the
// unit tests.
suite('sample workspace', () => {
    suiteSetup(async () => {
        await activateExtension();
    });

    /**
     * Runs the body against a throwaway document inside the workspace, then removes it. Scratch
     * files rather than edits to the committed samples, so a failing test cannot leave the repo
     * dirty. The file has to live in the workspace for the project to own it.
     */
    async function withScratchQuery(
        text: string,
        body: (uri: vscode.Uri) => Promise<void>,
        extension = '.nql'): Promise<void> {
        const uri = fileInWorkspace('queries', `integration-scratch${extension}`);
        await vscode.workspace.fs.writeFile(uri, Buffer.from(text, 'utf8'));

        try {
            await openDocument(uri);
            await body(uri);
        } finally {
            await vscode.commands.executeCommand('workbench.action.closeActiveEditor');

            try {
                await vscode.workspace.fs.delete(uri);
            } catch {
                // Already gone.
            }
        }
    }

    test('activates', async () => {
        const extension = await activateExtension();
        assert.ok(extension.isActive);
    });

    test('registers its commands', async () => {
        const commands = await vscode.commands.getCommands(true);

        for (const command of [
            'nquery.execute',
            'nquery.showPlan',
            'nquery.restartServer',
            'nquery.showOutput',
            'nquery.reloadCatalog'
        ]) {
            assert.ok(commands.includes(command), `${command} is not registered`);
        }
    });

    test('treats .nql and .nqe as nquery documents', async () => {
        const query = await openDocument(fileInWorkspace('queries', 'top-customers.nql'));
        assert.equal(query.languageId, 'nquery');

        const expression = await openDocument(fileInWorkspace('queries', 'scratch.nqe'));
        assert.equal(expression.languageId, 'nquery');
    });

    test('reports no errors for the sample queries', async () => {
        // "No diagnostics" is indistinguishable from "the server never started", so prove the
        // server is alive and bound to the catalog first, using a file that must produce an
        // error, and only then assert the absence of one on a valid query.
        await withScratchQuery('SELECT * FROM ThisTableDoesNotExist', async scratch => {
            await waitFor('the server to report the unknown table', () =>
                serverDiagnostics(scratch).find(d => d.code === 'UndeclaredTable'));
        });

        const uri = fileInWorkspace('queries', 'top-customers.nql');
        await openDocument(uri);

        await waitFor('no errors on a valid sample query', () => {
            const errors = serverDiagnostics(uri).filter(d => d.severity === vscode.DiagnosticSeverity.Error);
            return errors.length === 0 ? true : undefined;
        });
    });

    test('reports an error for an unknown table, end to end', async () => {
        await withScratchQuery('SELECT * FROM ThisTableDoesNotExist', async uri => {
            const diagnostic = await waitFor('an UndeclaredTable diagnostic from the server', () =>
                serverDiagnostics(uri).find(d => d.code === 'UndeclaredTable'));

            assert.equal(diagnostic.severity, vscode.DiagnosticSeverity.Error);
            assert.equal(diagnostic.source, 'nquery');
            assert.match(diagnostic.message, /ThisTableDoesNotExist/);
        });
    });

    test('binds a .nqe expression document', async () => {
        await withScratchQuery('COALESCE(NULL, 40) + 2', async uri => {
            // A bare expression only binds if the server mapped .nqe to DocumentKind.Expression;
            // parsed as a query it would be a syntax error.
            await waitFor('no errors on a valid expression document', () => {
                const errors = serverDiagnostics(uri).filter(d => d.severity === vscode.DiagnosticSeverity.Error);
                return errors.length === 0 ? true : undefined;
            });
        }, '.nqe');
    });

    test('exports real results to a file', async () => {
        // The unit tests format hand-written results; this formats results that actually came
        // back from the engine, so it covers the wiring from query to panel to formatter.
        //
        // Asserted through the filesystem rather than the clipboard: vscode.env.clipboard does
        // not round-trip in a background test window, so a clipboard assertion would fail for
        // reasons having nothing to do with this extension.
        await openDocument(fileInWorkspace('queries', 'top-customers.nql'));
        await vscode.commands.executeCommand('nquery.execute');

        const target = fileInWorkspace('queries', 'integration-export.csv');

        try {
            await vscode.commands.executeCommand('nquery.exportResults', { format: 'csv', uri: target });

            const written = await waitFor('the exported file', async () => {
                try {
                    return Buffer.from(await vscode.workspace.fs.readFile(target)).toString('utf8');
                } catch {
                    return undefined;
                }
            });

            // Excel on Windows needs the byte order mark to decode UTF-8.
            assert.ok(written.startsWith('﻿'), 'CSV should start with a byte order mark');

            const [header, ...rows] = written.slice(1).trimEnd().split('\r\n');
            assert.equal(header, 'CompanyName,City,OrderCount');
            assert.ok(rows.length > 0, 'expected at least one data row');

            // Every row has the same field count as the header, which is what quoting exists to
            // guarantee -- Germany company names contain commas.
            for (const row of rows) {
                assert.equal(countFields(row), 3, `unexpected field count in: ${row}`);
            }
        } finally {
            try {
                await vscode.workspace.fs.delete(target);
            } catch {
                // Never written.
            }
        }
    });

    test('runs the copy commands without error', async () => {
        // The clipboard cannot be read back here, so this only asserts the commands complete --
        // the formatting itself is covered by the unit tests and by the export test above.
        await openDocument(fileInWorkspace('queries', 'top-customers.nql'));
        await vscode.commands.executeCommand('nquery.execute');

        await vscode.commands.executeCommand('nquery.copyResults');
        await vscode.commands.executeCommand('nquery.copyResultsAsMarkdown');
    });

    test('offers code actions and applies one', async () => {
        await withScratchQuery('SELECT * FROM Customers c', async uri => {
            const document = await vscode.workspace.openTextDocument(uri);

            // 'c' is an alias without AS. Asking through the real code action provider proves the
            // lightbulb path works, not just that the server returns something.
            const position = new vscode.Position(0, document.getText().length - 1);

            const actions = await waitFor('a code action offering to add AS', async () => {
                const offered = await vscode.commands.executeCommand<vscode.CodeAction[]>(
                    'vscode.executeCodeActionProvider',
                    uri,
                    new vscode.Range(position, position));

                return offered?.find(a => /\bAS\b/i.test(a.title));
            });

            assert.ok(actions.edit, 'the action should carry a workspace edit');

            const applied = await vscode.workspace.applyEdit(actions.edit);
            assert.ok(applied, 'the edit should apply cleanly');

            assert.equal(document.getText(), 'SELECT * FROM Customers AS c');
        });
    });

    test('enables the run and plan commands once the server reports its capabilities', async () => {
        await openDocument(fileInWorkspace('queries', 'top-customers.nql'));

        // The context keys are set from the server's advertised capabilities, so this only passes
        // once the handshake has actually completed. Executing the command is the observable
        // proxy: a `when`-gated command still exists, so this asserts it runs without throwing.
        await waitFor('the plan command to succeed', async () => {
            try {
                await vscode.commands.executeCommand('nquery.showPlan');
                return true;
            } catch {
                return undefined;
            }
        });
    });
});

import * as path from 'path';
import * as vscode from 'vscode';

import { ProjectClient, fingerprintOf } from './client';
import { ExportFormat, byteOrderMark, fileExtension, formatResults } from './export';
import { PlanPanel, ShowPlanResult } from './planPanel';
import { Project, diagnosticSource, discoverProjects, findOwningProject, projectGlob } from './projects';
import { ExecuteResult, ResultsPanel } from './resultsPanel';

let projectDiagnostics: vscode.DiagnosticCollection;
let statusBar: vscode.StatusBarItem;

const clients = new Map<string, ProjectClient>();
let defaultClient: ProjectClient | undefined;
let projects: Project[] = [];
let refreshQueue: Promise<void> = Promise.resolve();

export async function activate(context: vscode.ExtensionContext): Promise<void> {
    projectDiagnostics = vscode.languages.createDiagnosticCollection('nquery-projects');
    statusBar = vscode.window.createStatusBarItem(vscode.StatusBarAlignment.Right, 100);
    statusBar.command = 'nquery.showOutput';

    context.subscriptions.push(projectDiagnostics, statusBar);

    const watcher = vscode.workspace.createFileSystemWatcher(projectGlob);
    context.subscriptions.push(
        watcher,
        watcher.onDidCreate(() => scheduleRefresh()),
        watcher.onDidChange(() => scheduleRefresh()),
        watcher.onDidDelete(() => scheduleRefresh()),
        vscode.workspace.onDidChangeWorkspaceFolders(() => scheduleRefresh()),
        vscode.window.onDidChangeActiveTextEditor(() => updateStatusBar()),
        vscode.workspace.onDidChangeConfiguration(e => {
            if (e.affectsConfiguration('nquery.defaultHost')) {
                scheduleRefresh();
            }
        }),

        // Opening a folder untrusted and then trusting it must bring the servers up without a
        // window reload.
        vscode.workspace.onDidGrantWorkspaceTrust(() => scheduleRefresh()),

        vscode.commands.registerCommand('nquery.restartServer', restartServerCommand),
        vscode.commands.registerCommand('nquery.showOutput', showOutputCommand),
        vscode.commands.registerCommand('nquery.reloadCatalog', reloadCatalogCommand),
        vscode.commands.registerCommand('nquery.execute', executeCommand),
        vscode.commands.registerCommand('nquery.showPlan', showPlanCommand),
        vscode.commands.registerCommand('nquery.copyResults', () => copyResultsCommand('tsv')),
        vscode.commands.registerCommand('nquery.copyResultsAsMarkdown', () => copyResultsCommand('markdown')),
        vscode.commands.registerCommand('nquery.exportResults', exportResultsCommand));

    // Gates the export commands so they are hidden when the panel holds nothing to export.
    ResultsPanel.onDidChange = () => {
        void vscode.commands.executeCommand('setContext', 'nquery.hasResults', ResultsPanel.results !== undefined);
    };

    await refresh();
}

export async function deactivate(): Promise<void> {
    await Promise.all([...clients.values()].map(c => c.stop()));
    clients.clear();
}

/** Serializes refreshes so a burst of file events cannot interleave two reconciliations. */
function scheduleRefresh(): void {
    refreshQueue = refreshQueue.then(() => refresh()).catch(() => undefined);
}

async function refresh(): Promise<void> {
    const folders = vscode.workspace.workspaceFolders ?? [];
    const discovery = await discoverProjects(folders);

    projectDiagnostics.clear();
    for (const [fsPath, diagnostics] of discovery.diagnostics) {
        projectDiagnostics.set(vscode.Uri.file(fsPath), diagnostics);
    }

    projects = discovery.projects;

    // A project file names an executable to run, so an untrusted workspace gets diagnostics and
    // syntax highlighting but no server processes.
    if (!vscode.workspace.isTrusted) {
        await stopAll();
        updateStatusBar();
        return;
    }

    await reconcile(projects);
    await reconcileDefaultHost();
    updateStatusBar();
}

async function reconcileDefaultHost(): Promise<void> {
    const configuration = vscode.workspace.getConfiguration('nquery');
    const command = configuration.get<string | null>('defaultHost.command') ?? '';
    const args = configuration.get<string[]>('defaultHost.args') ?? [];

    // Only meaningful when nothing else is running -- see ProjectClient.createDefault.
    const wanted = projects.length === 0 && command.length > 0
        ? JSON.stringify({ command, args })
        : undefined;

    if (defaultClient && defaultClient.fingerprint !== wanted) {
        await defaultClient.stop();
        defaultClient = undefined;
    }

    if (wanted && !defaultClient) {
        defaultClient = ProjectClient.createDefault(command, args, vscode.workspace.workspaceFolders?.[0]);
        await defaultClient?.start(updateStatusBar);
    }
}

async function reconcile(desired: Project[]): Promise<void> {
    const desiredByPath = new Map(desired.map(p => [p.uri.fsPath, p]));

    // Stop clients whose project vanished, or whose launch configuration changed -- host command,
    // args, env and the settings blob are all fixed at process start.
    for (const [fsPath, client] of [...clients]) {
        const project = desiredByPath.get(fsPath);
        if (!project || fingerprintOf(project) !== client.fingerprint) {
            await client.stop();
            clients.delete(fsPath);
        }
    }

    for (const project of desired) {
        if (clients.has(project.uri.fsPath)) {
            continue;
        }

        const client = ProjectClient.create(project);
        clients.set(project.uri.fsPath, client);
        await client.start(updateStatusBar);
    }
}

async function stopAll(): Promise<void> {
    for (const [fsPath, client] of [...clients]) {
        await client.stop();
        clients.delete(fsPath);
    }

    if (defaultClient) {
        await defaultClient.stop();
        defaultClient = undefined;
    }
}

function activeProject(): Project | undefined {
    const editor = vscode.window.activeTextEditor;
    if (!editor || editor.document.languageId !== 'nquery') {
        return undefined;
    }

    return findOwningProject(projects, editor.document.uri);
}

/**
 * Drives the `when` clauses for Run Query / Show Plan. Scoped to the active document rather than
 * the workspace, because two projects can point at hosts that disagree about whether running
 * queries is allowed.
 */
function updateContextKeys(): void {
    const editor = vscode.window.activeTextEditor;
    const isNQuery = editor?.document.languageId === 'nquery';

    let canExecute = false;
    let canShowPlan = false;

    if (isNQuery && editor) {
        const project = findOwningProject(projects, editor.document.uri);
        const client = project ? clients.get(project.uri.fsPath) : defaultClient;
        const experimental = client?.client.initializeResult?.capabilities?.experimental as
            { execute?: boolean; showPlan?: boolean } | undefined;

        // A server that predates these capabilities advertises nothing; treat that as "no".
        canExecute = experimental?.execute === true;
        canShowPlan = experimental?.showPlan === true;
    }

    void vscode.commands.executeCommand('setContext', 'nquery.canExecute', canExecute);
    void vscode.commands.executeCommand('setContext', 'nquery.canShowPlan', canShowPlan);
}

function updateStatusBar(): void {
    updateContextKeys();

    const editor = vscode.window.activeTextEditor;

    if (!editor || editor.document.languageId !== 'nquery') {
        statusBar.hide();
        return;
    }

    const project = findOwningProject(projects, editor.document.uri);
    const client = project ? clients.get(project.uri.fsPath) : defaultClient;

    statusBar.backgroundColor = undefined;

    // A failed catalog is the one state the user most needs to see: everything still "works",
    // but nothing resolves. A transient notification is too easy to miss, so it also lands here.
    if (client?.catalogError) {
        statusBar.text = `$(error) NQuery: catalog unavailable`;
        statusBar.tooltip = `${client.catalogError}\n\nRun "NQuery: Reload Catalog" to retry, or "NQuery: Show Output" for details.`;
        statusBar.backgroundColor = new vscode.ThemeColor('statusBarItem.errorBackground');
        statusBar.show();
        return;
    }

    if (!project && defaultClient) {
        statusBar.text = '$(database) NQuery: default host';
        statusBar.tooltip = 'This file is not covered by a .nqproj file and is served by nquery.defaultHost.';
    } else if (!project) {
        statusBar.text = '$(warning) NQuery: no project';
        statusBar.tooltip = 'This file is not covered by any .nqproj file, so no catalog is available.';
    } else if (!vscode.workspace.isTrusted) {
        statusBar.text = `$(shield) NQuery: ${project.name} (untrusted)`;
        statusBar.tooltip = 'The workspace is not trusted, so the language server was not started.';
    } else {
        statusBar.text = `$(database) NQuery: ${project.name}`;
        statusBar.tooltip = project.uri.fsPath;
    }

    statusBar.show();
}

async function pickProject(): Promise<Project | undefined> {
    if (projects.length === 0) {
        void vscode.window.showInformationMessage('NQuery: no project files were found in this workspace.');
        return undefined;
    }

    const active = activeProject();
    if (active) {
        return active;
    }

    if (projects.length === 1) {
        return projects[0];
    }

    const picked = await vscode.window.showQuickPick(
        projects.map(p => ({ label: p.name, description: path.dirname(p.uri.fsPath), project: p })),
        { placeHolder: 'Select an NQuery project' });

    return picked?.project;
}

async function restartServerCommand(): Promise<void> {
    const project = await pickProject();
    if (!project) {
        return;
    }

    const existing = clients.get(project.uri.fsPath);
    if (existing) {
        await existing.stop();
        clients.delete(project.uri.fsPath);
    }

    const client = ProjectClient.create(project);
    clients.set(project.uri.fsPath, client);
    await client.start(updateStatusBar);
}

async function showOutputCommand(): Promise<void> {
    const project = await pickProject();
    const client = project ? clients.get(project.uri.fsPath) : undefined;
    client?.client.outputChannel.show();
}

async function reloadCatalogCommand(): Promise<void> {
    const project = await pickProject();
    const client = project ? clients.get(project.uri.fsPath) : undefined;

    if (!client) {
        return;
    }

    try {
        await client.client.sendRequest('nquery/reloadCatalog', {});
    } catch (error) {
        void vscode.window.showErrorMessage(`NQuery: the server did not reload its catalog: ${error}`);
    }
}

/**
 * The server for the active NQuery document. Ownership is exclusive -- project roots are
 * disjoint -- so this is unambiguous; the default host only exists when there are no projects.
 */
function clientForActiveDocument(): { client: ProjectClient; document: vscode.TextDocument } | undefined {
    const editor = vscode.window.activeTextEditor;

    if (!editor || editor.document.languageId !== 'nquery') {
        void vscode.window.showInformationMessage('NQuery: open an .nql or .nqe file first.');
        return undefined;
    }

    const project = findOwningProject(projects, editor.document.uri);
    const client = project ? clients.get(project.uri.fsPath) : defaultClient;

    if (!client) {
        void vscode.window.showWarningMessage(
            'NQuery: no language server is running for this file. Add a .nqproj project file or set nquery.defaultHost.command.');
        return undefined;
    }

    return { client, document: editor.document };
}

async function executeCommand(): Promise<void> {
    const target = clientForActiveDocument();
    if (!target) {
        return;
    }

    // Text is already in sync: the client sends didChange on every edit, and LSP preserves
    // message order, so the server sees the current buffer even when it is unsaved.
    await vscode.window.withProgress(
        { location: vscode.ProgressLocation.Window, title: 'NQuery: running query…' },
        async () => {
            try {
                const result = await target.client.client.sendRequest<ExecuteResult>('nquery/execute', {
                    textDocument: { uri: target.document.uri.toString() },
                    maxRows: vscode.workspace.getConfiguration('nquery').get<number>('results.maxRows')
                });

                ResultsPanel.show(target.document.uri, result);
            } catch (error) {
                void vscode.window.showErrorMessage(`NQuery: the query could not be run: ${error}`);
            }
        });
}

async function showPlanCommand(): Promise<void> {
    const target = clientForActiveDocument();
    if (!target) {
        return;
    }

    await vscode.window.withProgress(
        { location: vscode.ProgressLocation.Window, title: 'NQuery: building execution plan…' },
        async () => {
            try {
                const result = await target.client.client.sendRequest<ShowPlanResult>('nquery/showPlan', {
                    textDocument: { uri: target.document.uri.toString() }
                });

                PlanPanel.show(target.document.uri, result);
            } catch (error) {
                void vscode.window.showErrorMessage(`NQuery: the execution plan could not be built: ${error}`);
            }
        });
}

function exportOptions(): { nullText: string; delimiter: string } {
    const configuration = vscode.workspace.getConfiguration('nquery');

    return {
        nullText: configuration.get<string>('export.nullText') ?? '',
        delimiter: configuration.get<string>('export.csvDelimiter') || ','
    };
}

function requireResults(): { documentUri: vscode.Uri; result: ExecuteResult } | undefined {
    const results = ResultsPanel.results;

    if (!results) {
        void vscode.window.showInformationMessage('NQuery: run a query first — there are no results to export.');
        return undefined;
    }

    return results;
}

async function copyResultsCommand(format: ExportFormat): Promise<void> {
    const results = requireResults();
    if (!results) {
        return;
    }

    // No byte order mark: it is only needed so Excel decodes a *file* correctly, and it would
    // show up as a stray character when pasted.
    await vscode.env.clipboard.writeText(formatResults(results.result, format, exportOptions()));

    const rows = results.result.rows.length;
    const suffix = results.result.truncated ? ' (truncated)' : '';
    void vscode.window.showInformationMessage(`NQuery: copied ${rows} ${rows === 1 ? 'row' : 'rows'}${suffix}.`);
}

/**
 * Both arguments are optional and normally come from the user. Supplying them skips the
 * corresponding prompt, which lets the command be bound to a keybinding with fixed arguments or
 * invoked by another extension -- and is how the integration tests drive it, since a save dialog
 * cannot be answered headlessly.
 */
async function exportResultsCommand(options?: { format?: ExportFormat; uri?: vscode.Uri }): Promise<void> {
    const results = requireResults();
    if (!results) {
        return;
    }

    // Writing a truncated result to a file named after the query is a data-loss trap: nothing in
    // the file says it is partial. Copying is transient enough not to warrant the interruption.
    // Skipped when a target was supplied, since the caller has already decided.
    if (results.result.truncated && !options?.uri) {
        const proceed = await vscode.window.showWarningMessage(
            `These results are truncated at ${results.result.rows.length} rows, so the file will be incomplete.`,
            { modal: true },
            'Export Anyway');

        if (proceed !== 'Export Anyway') {
            return;
        }
    }

    const formats: { label: string; description: string; format: ExportFormat }[] = [
        { label: 'CSV', description: 'Comma-separated, UTF-8 with BOM for Excel', format: 'csv' },
        { label: 'TSV', description: 'Tab-separated', format: 'tsv' },
        { label: 'Markdown', description: 'Table, for pasting into docs and issues', format: 'markdown' }
    ];

    let format = options?.format;

    if (!format) {
        const picked = await vscode.window.showQuickPick(formats, { placeHolder: 'Export results as' });
        if (!picked) {
            return;
        }

        format = picked.format;
    }

    const base = path.basename(results.documentUri.fsPath, path.extname(results.documentUri.fsPath));
    const defaultUri = vscode.Uri.file(
        path.join(path.dirname(results.documentUri.fsPath), `${base}.${fileExtension(format)}`));

    const target = options?.uri ?? await vscode.window.showSaveDialog({ defaultUri });
    if (!target) {
        return;
    }

    const text = formatResults(results.result, format, exportOptions());

    // Excel on Windows mis-decodes UTF-8 without a BOM, so CSV files get one. The other formats
    // are read by tools that do not need it.
    const content = format === 'csv' ? byteOrderMark + text : text;

    try {
        await vscode.workspace.fs.writeFile(target, Buffer.from(content, 'utf8'));
        void vscode.window.showInformationMessage(`NQuery: exported ${results.result.rows.length} rows to ${path.basename(target.fsPath)}.`);
    } catch (error) {
        void vscode.window.showErrorMessage(`NQuery: could not write ${target.fsPath}: ${error}`);
    }
}

export const _testing = { diagnosticSource };

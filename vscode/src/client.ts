import * as vscode from 'vscode';
import {
    CloseAction,
    CloseHandlerResult,
    ErrorAction,
    ErrorHandler,
    ErrorHandlerResult,
    Executable,
    LanguageClient,
    LanguageClientOptions,
    ServerOptions
} from 'vscode-languageclient/node';

import { Project, expandVariables } from './projects';

interface CatalogStatus {
    projectName: string;
    available: boolean;
    errorMessage?: string;
}

export class ProjectClient {
    readonly client: LanguageClient;

    /** Set while the host cannot produce a catalog; drives the status bar. */
    catalogError: string | undefined;

    /** Set while the server process is not running at all; drives the status bar. */
    serverError: string | undefined;

    /** Unexpected exits already retried. Reset by a start, so a restart gets a fresh budget. */
    private restarts = 0;

    /** Set once a stop has been asked for, so a dropping connection is not restarted behind it. */
    private stopping = false;

    private onChanged: (() => void) | undefined;

    private constructor(
        readonly project: Project,
        /**
         * True for the nquery.defaultHost server, which is restarted from settings rather than
         * from a project file and has no meaningful `project.uri`.
         */
        readonly isDefault: boolean,
        /** Identity of the launch configuration, so a project file edit can be detected. */
        readonly fingerprint: string,
        id: string,
        name: string,
        serverOptions: ServerOptions,
        clientOptions: LanguageClientOptions) {
        this.client = new LanguageClient(id, name, serverOptions, {
            ...clientOptions,
            errorHandler: this.createErrorHandler()
        });
    }

    /**
     * Serves files that no project covers. It is only ever started when the workspace has no
     * projects at all: a document selector cannot express "everything except these folders", so
     * running it alongside projects would hand owned documents to two servers.
     */
    static createDefault(command: string, args: string[], folder: vscode.WorkspaceFolder | undefined): ProjectClient | undefined {
        if (!command) {
            return undefined;
        }

        const executable: Executable = { command, args, options: { cwd: folder?.uri.fsPath } };
        const serverOptions: ServerOptions = { run: executable, debug: executable };

        const clientOptions: LanguageClientOptions = {
            documentSelector: [{ scheme: 'file', language: 'nquery' }],
            outputChannelName: 'NQuery (default)',
            diagnosticCollectionName: 'nquery-default',
            initializationOptions: { projectName: 'default', settings: undefined }
        };

        const project = {
            uri: vscode.Uri.file(''),
            name: 'default',
            rootDir: folder?.uri.fsPath ?? '',
            workspaceFolder: folder as vscode.WorkspaceFolder,
            host: { command, args },
            settings: undefined
        } as Project;

        return new ProjectClient(
            project,
            true,
            JSON.stringify({ command, args }),
            'nquery.default',
            'NQuery (default)',
            serverOptions,
            clientOptions);
    }

    static create(project: Project): ProjectClient {
        const command = expandVariables(project.host.command, project);
        const args = (project.host.args ?? []).map(a => expandVariables(a, project));
        const cwd = project.host.cwd ? expandVariables(project.host.cwd, project) : project.rootDir;

        const env: Record<string, string> = { ...process.env as Record<string, string> };
        for (const [key, value] of Object.entries(project.host.env ?? {})) {
            env[key] = expandVariables(value, project);
        }

        const executable: Executable = { command, args, options: { cwd, env } };
        const serverOptions: ServerOptions = { run: executable, debug: executable };

        const clientOptions: LanguageClientOptions = {
            // Scoped to the project's folder, which is safe because project roots are disjoint --
            // no document is ever offered to two servers.
            documentSelector: [
                {
                    scheme: 'file',
                    language: 'nquery',
                    pattern: globForFolder(project.rootDir)
                }
            ],
            workspaceFolder: project.workspaceFolder,
            outputChannelName: `NQuery (${project.name})`,
            diagnosticCollectionName: `nquery-${project.name}`,
            initializationOptions: {
                projectFile: project.uri.toString(),
                projectName: project.name,
                // Passed through untouched: the extension does not know or care what is in here.
                settings: project.settings
            }
        };

        return new ProjectClient(
            project,
            false,
            fingerprintOf(project),
            `nquery.${project.name}`,
            `NQuery (${project.name})`,
            serverOptions,
            clientOptions);
    }

    async start(onChanged?: () => void): Promise<void> {
        this.onChanged = onChanged;
        this.restarts = 0;
        this.stopping = false;
        this.serverError = undefined;

        // Registered before start() so the first status, which the server sends as soon as it
        // resolves the catalog, cannot be missed.
        this.client.onNotification('nquery/catalogStatus', (status: CatalogStatus) => {
            this.catalogError = status.available ? undefined : (status.errorMessage ?? 'unknown error');
            onChanged?.();
        });

        try {
            await this.client.start();
        } catch (error) {
            this.serverError = `${error}`;
            void vscode.window.showErrorMessage(
                `NQuery: failed to start the language server for '${this.project.name}': ${error}`);
            onChanged?.();
        }
    }

    async stop(): Promise<void> {
        // Ordered before the stop, because the close handler runs as the connection drops and must
        // not resurrect the very server being torn down.
        this.stopping = true;

        try {
            await this.client.stop();
        } catch {
            // stop() refuses -- and throws before running its own clean-up -- unless the client is
            // Running, which is exactly what a crashed or still-starting one is not. The server
            // process is killed either way (the node client does that in a `finally`), so the
            // output channel is the only thing left that the clean-up would have released.
            this.client.outputChannel.dispose();
        }
    }

    /**
     * Replaces the stock handler, which restarts five times in three minutes and reports only into
     * the output channel. A host that cannot launch at all -- a path that no longer exists, a
     * runtime that is not installed -- burns all five in well under a second, so the user gets five
     * identical failures scrolling past and a status bar that still looks healthy. One retry covers
     * a server that genuinely crashed; past that the failure is recorded for the status bar and the
     * server is left alone until the user asks for a restart.
     */
    private createErrorHandler(): ErrorHandler {
        return {
            error: (_error, _message, count): ErrorHandlerResult => {
                // Same threshold as the default handler: a few failed messages are survivable, a
                // steady stream means the connection itself is gone.
                return count !== undefined && count <= 3
                    ? { action: ErrorAction.Continue, handled: true }
                    : { action: ErrorAction.Shutdown, handled: true };
            },

            closed: (): CloseHandlerResult => {
                if (this.stopping) {
                    return { action: CloseAction.DoNotRestart, handled: true };
                }

                if (this.restarts === 0) {
                    this.restarts++;
                    return { action: CloseAction.Restart, handled: true };
                }

                this.setServerError('The server process exited and did not come back up.');
                return { action: CloseAction.DoNotRestart, handled: true };
            }
        };
    }

    private setServerError(message: string): void {
        this.serverError = message;
        this.onChanged?.();
    }
}

/**
 * A DocumentFilter pattern is an LSP glob string matched against the document's absolute path,
 * so the folder is emitted with forward slashes even on Windows.
 */
function globForFolder(folder: string): string {
    return `${folder.replace(/\\/g, '/')}/**/*`;
}

/**
 * Covers everything that is baked in when the server process starts: the launch configuration
 * and the settings blob, which is only ever sent as initializationOptions. A change to any of it
 * requires a restart rather than a notification.
 */
export function fingerprintOf(project: Project): string {
    return JSON.stringify({ host: project.host, settings: project.settings });
}

import * as path from 'path';
import * as vscode from 'vscode';
import {
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
    /** Set while the host cannot produce a catalog; drives the status bar. */
    catalogError: string | undefined;

    private constructor(
        readonly project: Project,
        readonly client: LanguageClient,
        /** Identity of the launch configuration, so a project file edit can be detected. */
        readonly fingerprint: string) {
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

        const client = new LanguageClient('nquery.default', 'NQuery (default)', serverOptions, clientOptions);
        const project = {
            uri: vscode.Uri.file(''),
            name: 'default',
            rootDir: folder?.uri.fsPath ?? '',
            workspaceFolder: folder as vscode.WorkspaceFolder,
            host: { command, args },
            settings: undefined
        } as Project;

        return new ProjectClient(project, client, JSON.stringify({ command, args }));
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

        const client = new LanguageClient(
            `nquery.${project.name}`,
            `NQuery (${project.name})`,
            serverOptions,
            clientOptions);

        return new ProjectClient(project, client, fingerprintOf(project));
    }

    async start(onCatalogStatus?: () => void): Promise<void> {
        // Registered before start() so the first status, which the server sends as soon as it
        // resolves the catalog, cannot be missed.
        this.client.onNotification('nquery/catalogStatus', (status: CatalogStatus) => {
            this.catalogError = status.available ? undefined : (status.errorMessage ?? 'unknown error');
            onCatalogStatus?.();
        });

        try {
            await this.client.start();
        } catch (error) {
            const name = path.basename(this.project.uri.fsPath);
            void vscode.window.showErrorMessage(`NQuery: failed to start the language server for '${name}': ${error}`);
        }
    }

    async stop(): Promise<void> {
        try {
            await this.client.stop();
        } catch {
            // Already dead; nothing useful left to do.
        }
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

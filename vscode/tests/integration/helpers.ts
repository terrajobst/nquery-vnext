import * as path from 'path';
import * as vscode from 'vscode';

export const extensionId = 'nquery.vscode-nquery';

export async function activateExtension(): Promise<vscode.Extension<unknown>> {
    const extension = vscode.extensions.getExtension(extensionId);

    if (!extension) {
        throw new Error(`The extension '${extensionId}' is not installed in the test instance.`);
    }

    await extension.activate();
    return extension;
}

export function workspaceFolder(): vscode.WorkspaceFolder {
    const folder = vscode.workspace.workspaceFolders?.[0];

    if (!folder) {
        throw new Error('The test instance opened without a workspace folder.');
    }

    return folder;
}

export function fileInWorkspace(...segments: string[]): vscode.Uri {
    return vscode.Uri.file(path.join(workspaceFolder().uri.fsPath, ...segments));
}

/**
 * Polls until the condition holds. Everything interesting here is asynchronous and has no event to
 * await -- diagnostics arrive when the server gets round to publishing them -- so polling is the
 * honest way to express "eventually".
 */
export async function waitFor<T>(
    description: string,
    probe: () => T | undefined | Promise<T | undefined>,
    timeoutMs = 60000): Promise<T> {
    const started = Date.now();
    let last: unknown;

    while (Date.now() - started < timeoutMs) {
        try {
            const value = await probe();
            if (value !== undefined && value !== null && value !== false) {
                return value as T;
            }
        } catch (error) {
            last = error;
        }

        await delay(200);
    }

    const suffix = last ? ` Last error: ${last}` : '';
    throw new Error(`Timed out after ${timeoutMs}ms waiting for ${description}.${suffix}`);
}

export function delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
}

export async function openDocument(uri: vscode.Uri): Promise<vscode.TextDocument> {
    const document = await vscode.workspace.openTextDocument(uri);
    await vscode.window.showTextDocument(document);
    return document;
}

/** Diagnostics the language server published, as opposed to the extension's own project checks. */
export function serverDiagnostics(uri: vscode.Uri): vscode.Diagnostic[] {
    return vscode.languages.getDiagnostics(uri);
}

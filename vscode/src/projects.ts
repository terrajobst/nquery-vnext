import * as path from 'path';
import * as vscode from 'vscode';

import {
    HostSpec,
    ProjectProblem,
    expandVariables as expandRuleVariables,
    findOwningRoot,
    parseProjectFile,
    selectProjectFiles
} from './projectRules';

export const projectGlob = '**/*.nqproj';
export const diagnosticSource = 'nquery';

export type { HostSpec } from './projectRules';

export interface Project {
    /** The .nqproj file itself. */
    uri: vscode.Uri;
    /** File name without extension; used for the output channel and status bar. */
    name: string;
    /** Folder containing the project file. It owns this folder and everything beneath it. */
    rootDir: string;
    workspaceFolder: vscode.WorkspaceFolder;
    host: HostSpec;
    /** Opaque application-specific blob, forwarded to the server untouched. */
    settings: unknown;
}

export interface ProjectDiscovery {
    projects: Project[];
    diagnostics: Map<string, vscode.Diagnostic[]>;
}

/**
 * Adapter over projectRules: finds the candidate files, reads them, and turns the rules' verdicts
 * into vscode types. The rules themselves live in projectRules.ts and are tested directly.
 */
export async function discoverProjects(folders: readonly vscode.WorkspaceFolder[]): Promise<ProjectDiscovery> {
    const projects: Project[] = [];
    const problems: ProjectProblem[] = [];

    for (const folder of folders) {
        const found = await vscode.workspace.findFiles(
            new vscode.RelativePattern(folder, projectGlob),
            '**/{node_modules,bin,obj,.git}/**');

        const { winners, problems: selectionProblems } = selectProjectFiles(found.map(u => u.fsPath));
        problems.push(...selectionProblems);

        for (const file of winners) {
            let text: string;

            try {
                text = Buffer.from(await vscode.workspace.fs.readFile(vscode.Uri.file(file))).toString('utf8');
            } catch (error) {
                problems.push({ file, severity: 'error', message: `Cannot read project file: ${error}` });
                continue;
            }

            const { content, problem } = parseProjectFile(file, text);

            if (problem || !content?.host) {
                if (problem) {
                    problems.push(problem);
                }
                continue;
            }

            projects.push({
                uri: vscode.Uri.file(file),
                name: path.basename(file, path.extname(file)),
                rootDir: path.dirname(file),
                workspaceFolder: folder,
                host: content.host,
                settings: content.settings
            });
        }
    }

    return { projects, diagnostics: toDiagnostics(problems) };
}

function toDiagnostics(problems: readonly ProjectProblem[]): Map<string, vscode.Diagnostic[]> {
    const result = new Map<string, vscode.Diagnostic[]>();

    for (const problem of problems) {
        const severity = problem.severity === 'error'
            ? vscode.DiagnosticSeverity.Error
            : vscode.DiagnosticSeverity.Warning;

        const diagnostic = new vscode.Diagnostic(new vscode.Range(0, 0, 0, 1), problem.message, severity);
        diagnostic.source = diagnosticSource;

        if (problem.relatedFile) {
            diagnostic.relatedInformation = [
                new vscode.DiagnosticRelatedInformation(
                    new vscode.Location(vscode.Uri.file(problem.relatedFile), new vscode.Position(0, 0)),
                    'Containing project')
            ];
        }

        const list = result.get(problem.file);

        if (list) {
            list.push(diagnostic);
        } else {
            result.set(problem.file, [diagnostic]);
        }
    }

    return result;
}

export function findOwningProject(projects: readonly Project[], uri: vscode.Uri): Project | undefined {
    if (uri.scheme !== 'file') {
        return undefined;
    }

    const root = findOwningRoot(projects.map(p => p.rootDir), uri.fsPath);
    return root === undefined ? undefined : projects.find(p => p.rootDir === root);
}

export function expandVariables(value: string, project: Project): string {
    return expandRuleVariables(value, {
        projectDir: project.rootDir,
        workspaceFolder: project.workspaceFolder.uri.fsPath
    });
}

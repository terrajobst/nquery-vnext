import * as path from 'path';
import * as jsonc from 'jsonc-parser';

// The project rules, deliberately free of any `vscode` dependency: they are plain functions over
// paths and strings so they can be tested directly rather than through a mocked editor API.
// projects.ts is the thin adapter that turns workspace state into these inputs and the results
// back into vscode types.

export interface HostSpec {
    command: string;
    args?: string[];
    cwd?: string;
    env?: Record<string, string>;
    transport?: 'stdio';
}

export interface ProjectFileContent {
    version?: number;
    host?: HostSpec;
    settings?: unknown;
}

export type ProblemSeverity = 'error' | 'warning';

export interface ProjectProblem {
    /** Absolute path of the .nqproj file the problem belongs to. */
    file: string;
    severity: ProblemSeverity;
    message: string;
    /** A second file worth linking to, e.g. the project a nested one sits inside. */
    relatedFile?: string;
}

export interface PathVariables {
    projectDir: string;
    workspaceFolder: string;
    userHome?: string;
    env?: Record<string, string | undefined>;
}

/**
 * Decides which .nqproj files actually run.
 *
 *  - One project per folder. If a folder has several, the alphabetically first wins and the rest
 *    are ignored with a warning on every file in that folder, so the problem is visible whichever
 *    one is open.
 *  - Projects cannot be nested. A project inside another project's folder is an error and does not
 *    run; its subtree stays unowned rather than falling back to the outer catalog, which would
 *    silently serve those files from the wrong schema.
 *
 * Together these guarantee the surviving roots are pairwise disjoint, so a file has at most one
 * owner and an open document's owner never changes to a different project.
 */
export function selectProjectFiles(files: readonly string[]): { winners: string[]; problems: ProjectProblem[] } {
    const problems: ProjectProblem[] = [];
    const winners = pickOnePerFolder(files, problems);

    // Ascending path order puts an ancestor before anything nested inside it, so the first project
    // seen for a subtree is the outermost and any later one is the offender.
    winners.sort((x, y) => comparePaths(path.dirname(x), path.dirname(y)));

    const accepted: string[] = [];

    for (const file of winners) {
        const rootDir = path.dirname(file);
        const ancestor = accepted.find(a => isAncestorDir(path.dirname(a), rootDir));

        if (ancestor) {
            problems.push({
                file,
                severity: 'error',
                message: `Project files cannot be nested. This project is inside the folder of '${path.basename(ancestor)}' and will be ignored.`,
                relatedFile: ancestor
            });
            continue;
        }

        accepted.push(file);
    }

    return { winners: accepted, problems };
}

function pickOnePerFolder(files: readonly string[], problems: ProjectProblem[]): string[] {
    const byFolder = new Map<string, string[]>();

    for (const file of files) {
        const key = normalize(path.dirname(file));
        const list = byFolder.get(key);
        if (list) {
            list.push(file);
        } else {
            byFolder.set(key, [file]);
        }
    }

    const winners: string[] = [];

    for (const [, group] of byFolder) {
        group.sort((x, y) => path.basename(x).localeCompare(path.basename(y)));
        const winner = group[0];
        winners.push(winner);

        if (group.length === 1) {
            continue;
        }

        const folderName = path.basename(path.dirname(winner));
        const winnerName = path.basename(winner);

        for (const file of group) {
            const suffix = file === winner
                ? `'${winnerName}' is used; the others are ignored.`
                : `'${winnerName}' is used; this project is ignored.`;

            problems.push({
                file,
                severity: 'warning',
                message: `Folder '${folderName}' contains ${group.length} project files. ${suffix}`
            });
        }
    }

    return winners;
}

export function parseProjectFile(file: string, text: string): { content?: ProjectFileContent; problem?: ProjectProblem } {
    const errors: jsonc.ParseError[] = [];
    const parsed = jsonc.parse(text, errors, { allowTrailingComma: true }) as Record<string, unknown> | undefined;

    if (errors.length > 0) {
        return {
            problem: {
                file,
                severity: 'error',
                message: `Cannot parse project file: ${jsonc.printParseErrorCode(errors[0].error)}.`
            }
        };
    }

    if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) {
        return { problem: { file, severity: 'error', message: 'The project file must contain a JSON object.' } };
    }

    if (parsed.version !== 1) {
        return {
            problem: {
                file,
                severity: 'error',
                message: `Unsupported project file version '${String(parsed.version)}'. Expected 1.`
            }
        };
    }

    const host = parsed.host as HostSpec | undefined;

    if (!host || typeof host.command !== 'string' || host.command.length === 0) {
        return { problem: { file, severity: 'error', message: "The project file must specify 'host.command'." } };
    }

    return { content: { version: 1, host, settings: parsed.settings } };
}

/** Longest-prefix match. Roots are disjoint, so at most one can match. */
export function findOwningRoot(roots: readonly string[], filePath: string): string | undefined {
    const dir = path.dirname(filePath);

    let best: string | undefined;

    for (const root of roots) {
        if (!isSameOrAncestorDir(root, dir)) {
            continue;
        }

        if (!best || root.length > best.length) {
            best = root;
        }
    }

    return best;
}

export function expandVariables(value: string, variables: PathVariables): string {
    const env = variables.env ?? process.env;

    return value
        .replace(/\$\{projectDir\}/g, variables.projectDir)
        .replace(/\$\{workspaceFolder\}/g, variables.workspaceFolder)
        .replace(/\$\{userHome\}/g, variables.userHome ?? process.env.HOME ?? process.env.USERPROFILE ?? '')
        .replace(/\$\{env:([^}]+)\}/g, (_, name: string) => env[name] ?? '');
}

/** Windows paths compare case-insensitively; POSIX paths do not. */
export function normalize(p: string): string {
    const resolved = path.resolve(p);
    return process.platform === 'win32' ? resolved.toLowerCase() : resolved;
}

function comparePaths(x: string, y: string): number {
    const a = normalize(x);
    const b = normalize(y);
    return a < b ? -1 : a > b ? 1 : 0;
}

export function isAncestorDir(ancestor: string, descendant: string): boolean {
    const a = normalize(ancestor);
    const d = normalize(descendant);
    return d.length > a.length && d.startsWith(a + path.sep);
}

export function isSameOrAncestorDir(ancestor: string, descendant: string): boolean {
    const a = normalize(ancestor);
    const d = normalize(descendant);
    return a === d || (d.length > a.length && d.startsWith(a + path.sep));
}

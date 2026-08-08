import * as assert from 'node:assert/strict';
import * as path from 'path';
import { describe, it } from 'node:test';

import {
    expandVariables,
    findOwningRoot,
    isAncestorDir,
    parseProjectFile,
    selectProjectFiles
} from '../../src/projectRules';

// Absolute paths so the rules run against the same shape they see in a real workspace. The drive
// letter is only used on win32; elsewhere these are ordinary rooted paths.
const root = process.platform === 'win32' ? 'C:\\ws' : '/ws';
const p = (...parts: string[]) => path.join(root, ...parts);

describe('selectProjectFiles', () => {
    it('accepts sibling projects', () => {
        const files = [p('warehouse', 'warehouse.nqproj'), p('telemetry', 'telemetry.nqproj')];

        const { winners, problems } = selectProjectFiles(files);

        assert.equal(winners.length, 2);
        assert.deepEqual(problems, []);
    });

    it('keeps the alphabetically first project when a folder has several', () => {
        const files = [p('warehouse', 'prod.nqproj'), p('warehouse', 'staging.nqproj')];

        const { winners, problems } = selectProjectFiles(files);

        assert.deepEqual(winners, [p('warehouse', 'prod.nqproj')]);

        // Every file in the folder is flagged, not just the losers, so the problem is visible
        // whichever one the user opens.
        assert.equal(problems.length, 2);
        assert.ok(problems.every(x => x.severity === 'warning'));
        assert.ok(problems.every(x => x.message.includes(`'prod.nqproj' is used`)));
    });

    it('is not affected by the order files are discovered in', () => {
        const forward = selectProjectFiles([p('w', 'a.nqproj'), p('w', 'b.nqproj')]);
        const reverse = selectProjectFiles([p('w', 'b.nqproj'), p('w', 'a.nqproj')]);

        assert.deepEqual(forward.winners, reverse.winners);
    });

    it('rejects a nested project and blames the inner one', () => {
        const outer = p('warehouse', 'warehouse.nqproj');
        const inner = p('warehouse', 'experimental', 'experimental.nqproj');

        const { winners, problems } = selectProjectFiles([outer, inner]);

        assert.deepEqual(winners, [outer]);

        const problem = problems.find(x => x.file === inner);
        assert.ok(problem, 'expected a problem on the nested project');
        assert.equal(problem.severity, 'error');
        assert.equal(problem.relatedFile, outer);
        assert.ok(problem.message.includes('cannot be nested'));
    });

    it('blames the inner project regardless of discovery order', () => {
        const outer = p('warehouse', 'warehouse.nqproj');
        const inner = p('warehouse', 'experimental', 'experimental.nqproj');

        const { winners } = selectProjectFiles([inner, outer]);

        assert.deepEqual(winners, [outer]);
    });

    it('rejects a project nested several levels down', () => {
        const outer = p('a', 'a.nqproj');
        const inner = p('a', 'b', 'c', 'd', 'd.nqproj');

        const { winners } = selectProjectFiles([outer, inner]);

        assert.deepEqual(winners, [outer]);
    });

    it('does not treat a sibling folder with a shared prefix as nested', () => {
        // 'warehouse-archive' starts with 'warehouse' as a string but is not inside it.
        const a = p('warehouse', 'warehouse.nqproj');
        const b = p('warehouse-archive', 'archive.nqproj');

        const { winners, problems } = selectProjectFiles([a, b]);

        assert.equal(winners.length, 2);
        assert.deepEqual(problems, []);
    });

    it('leaves surviving roots pairwise disjoint', () => {
        const files = [
            p('a', 'a.nqproj'),
            p('a', 'nested', 'n.nqproj'),
            p('b', 'b.nqproj'),
            p('b', 'second.nqproj')
        ];

        const { winners } = selectProjectFiles(files);
        const roots = winners.map(w => path.dirname(w));

        for (const x of roots) {
            for (const y of roots) {
                if (x !== y) {
                    assert.ok(!isAncestorDir(x, y), `${x} must not contain ${y}`);
                }
            }
        }
    });
});

describe('findOwningRoot', () => {
    const roots = [p('warehouse'), p('telemetry')];

    it('matches a file directly in the project folder', () => {
        assert.equal(findOwningRoot(roots, p('warehouse', 'q.nql')), p('warehouse'));
    });

    it('matches a file in a subfolder', () => {
        assert.equal(findOwningRoot(roots, p('warehouse', 'queries', 'deep', 'q.nql')), p('warehouse'));
    });

    it('returns nothing for a file outside every project', () => {
        assert.equal(findOwningRoot(roots, p('scratch.nql')), undefined);
    });

    it('does not match a sibling folder with a shared prefix', () => {
        assert.equal(findOwningRoot([p('warehouse')], p('warehouse-archive', 'q.nql')), undefined);
    });

    it('picks the longest matching root', () => {
        // Nesting is rejected upstream, so this cannot arise from discovery -- it just pins the
        // longest-prefix rule itself.
        const nested = [p('a'), p('a', 'b')];
        assert.equal(findOwningRoot(nested, p('a', 'b', 'q.nql')), p('a', 'b'));
    });
});

describe('parseProjectFile', () => {
    const file = p('w', 'w.nqproj');

    it('accepts a minimal project', () => {
        const { content, problem } = parseProjectFile(file, '{"version":1,"host":{"command":"dotnet"}}');

        assert.equal(problem, undefined);
        assert.equal(content?.host?.command, 'dotnet');
    });

    it('accepts comments and trailing commas', () => {
        const text = `{
            // the host to launch
            "version": 1,
            "host": { "command": "dotnet", },
        }`;

        const { content, problem } = parseProjectFile(file, text);

        assert.equal(problem, undefined);
        assert.equal(content?.host?.command, 'dotnet');
    });

    it('passes the settings blob through untouched', () => {
        const text = '{"version":1,"host":{"command":"x"},"settings":{"connection":"a","nested":{"n":1}}}';

        const { content } = parseProjectFile(file, text);

        assert.deepEqual(content?.settings, { connection: 'a', nested: { n: 1 } });
    });

    it('reports malformed JSON', () => {
        const { content, problem } = parseProjectFile(file, '{ this is not json');

        assert.equal(content, undefined);
        assert.equal(problem?.severity, 'error');
        assert.ok(problem.message.includes('Cannot parse'));
    });

    it('reports an unsupported version', () => {
        const { problem } = parseProjectFile(file, '{"version":2,"host":{"command":"x"}}');

        assert.equal(problem?.severity, 'error');
        assert.ok(problem.message.includes('version'));
    });

    it('reports a missing version', () => {
        const { problem } = parseProjectFile(file, '{"host":{"command":"x"}}');

        assert.equal(problem?.severity, 'error');
    });

    it('reports a missing host command', () => {
        for (const text of ['{"version":1}', '{"version":1,"host":{}}', '{"version":1,"host":{"command":""}}']) {
            const { problem } = parseProjectFile(file, text);
            assert.equal(problem?.severity, 'error', text);
            assert.ok(problem.message.includes('host.command'), text);
        }
    });

    it('rejects a top-level array', () => {
        const { problem } = parseProjectFile(file, '[]');

        assert.equal(problem?.severity, 'error');
    });
});

describe('expandVariables', () => {
    const variables = {
        projectDir: p('warehouse'),
        workspaceFolder: root,
        userHome: p('home'),
        env: { WAREHOUSE_ENV: 'staging' }
    };

    it('expands the path variables', () => {
        assert.equal(expandVariables('${projectDir}/tools/x.dll', variables), `${p('warehouse')}/tools/x.dll`);
        assert.equal(expandVariables('${workspaceFolder}/a', variables), `${root}/a`);
        assert.equal(expandVariables('${userHome}/a', variables), `${p('home')}/a`);
    });

    it('expands environment variables', () => {
        assert.equal(expandVariables('${env:WAREHOUSE_ENV}', variables), 'staging');
    });

    it('expands an unset environment variable to nothing', () => {
        assert.equal(expandVariables('[${env:NOT_SET_ANYWHERE}]', variables), '[]');
    });

    it('expands every occurrence', () => {
        assert.equal(
            expandVariables('${projectDir}:${projectDir}', variables),
            `${p('warehouse')}:${p('warehouse')}`);
    });

    it('leaves unknown variables alone', () => {
        assert.equal(expandVariables('${unknownThing}', variables), '${unknownThing}');
    });
});

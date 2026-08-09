import * as assert from 'node:assert/strict';
import { describe, it } from 'node:test';

import {
    ExecuteResult,
    ShowPlanNodeInfo,
    ShowPlanResult,
    escapeHtml,
    pageCount,
    renderPlan,
    renderResults,
    resultPage
} from '../../src/render';

const webview = { cspSource: 'vscode-webview://test' };

function results(overrides: Partial<ExecuteResult> = {}): ExecuteResult {
    return {
        columns: [{ name: 'CompanyName', type: 'string' }],
        rows: [['Alfreds Futterkiste']],
        truncated: false,
        elapsedMilliseconds: 7,
        ...overrides
    };
}

function node(overrides: Partial<ShowPlanNodeInfo> = {}): ShowPlanNodeInfo {
    return { operatorName: 'Query', isScalar: false, properties: [], children: [], ...overrides };
}

function plan(overrides: Partial<ShowPlanResult> = {}): ShowPlanResult {
    return { steps: [{ name: 'Physical', root: node() }], ...overrides };
}

describe('escapeHtml', () => {
    it('escapes every character that could break out of markup', () => {
        assert.equal(escapeHtml(`<b>&"'`), '&lt;b&gt;&amp;&quot;&#39;');
    });

    it('escapes ampersands before the entities it introduces', () => {
        assert.equal(escapeHtml('&lt;'), '&amp;lt;');
    });
});

/** `count` single-column rows, each carrying its own one-based number. */
function manyRows(count: number): (string | null)[][] {
    return Array.from({ length: count }, (_, index) => [`row ${index + 1}`]);
}

describe('pageCount', () => {
    it('divides rows into whole pages, rounding up', () => {
        assert.equal(pageCount(1000, 500), 2);
        assert.equal(pageCount(1001, 500), 3);
    });

    it('is one for an empty result, so there is always a page to show', () => {
        assert.equal(pageCount(0, 500), 1);
    });

    it('falls back to the default when the configured size is unusable', () => {
        // A `minimum` in package.json is advice to the settings editor, not a guarantee; zero
        // would otherwise divide the result into infinitely many pages.
        assert.equal(pageCount(1000, 0), 2);
        assert.equal(pageCount(1000, -5), 2);
        assert.equal(pageCount(1000, Number.NaN), 2);
    });
});

describe('resultPage', () => {
    it('slices the requested page', () => {
        const page = resultPage(results({ rows: manyRows(1200) }), 1, 500);

        assert.equal(page.index, 1);
        assert.equal(page.rows.length, 500);
        assert.deepEqual(page.rows[0], ['row 501']);
    });

    it('returns a short final page', () => {
        const page = resultPage(results({ rows: manyRows(1200) }), 2, 500);

        assert.equal(page.rows.length, 200);
        assert.deepEqual(page.rows[199], ['row 1200']);
    });

    it('clamps an out-of-range index rather than returning nothing', () => {
        // The index arrives from the webview, where it can be typed into the page box, and a
        // re-run can shrink the result under a page number the webview still believes in.
        const result = results({ rows: manyRows(1200) });

        assert.equal(resultPage(result, 99, 500).index, 2);
        assert.equal(resultPage(result, -1, 500).index, 0);
        assert.equal(resultPage(result, Number.NaN, 500).index, 0);
    });
});

describe('renderResults', () => {
    it('produces a document with a locked-down CSP and a nonced script', () => {
        const html = renderResults(webview, 'q.nql', results());

        assert.ok(html.startsWith('<!DOCTYPE html>'));
        assert.ok(html.includes(`default-src 'none'`));
        assert.ok(html.includes(webview.cspSource));

        const match = html.match(/script-src 'nonce-([A-Za-z0-9]+)'/);
        assert.ok(match, 'expected a nonce-based script CSP');
        assert.ok(html.includes(`<script nonce="${match[1]}">`));
    });

    it('carries only the first page in the document', () => {
        // The whole point of paging: a result large enough to need it is also large enough that
        // turning all of it into markup is what hurts.
        const html = renderResults(webview, 'q.nql', results({ rows: manyRows(1200) }), 500);

        assert.ok(html.includes('"row 1"'));
        assert.ok(html.includes('"row 500"'));
        assert.ok(!html.includes('"row 501"'));
        assert.ok(!html.includes('"row 1200"'));
    });

    it('escapes cell content out of markup', () => {
        // A result cell is attacker-controlled as far as the panel is concerned: it comes from
        // whatever rows the catalog's data contains. Cells reach the page as JSON and are set
        // with textContent, so nothing in them is ever parsed as HTML -- but the JSON itself
        // still sits inside a <script> block, which a raw `<` could end.
        const html = renderResults(webview, 'q.nql', results({
            rows: [['</script><img src=x onerror=alert(1)>']]
        }));

        assert.ok(!html.includes('</script><img'));
        assert.ok(html.includes('\\u003c/script>\\u003cimg src=x onerror=alert(1)>'));
    });

    it('escapes column names and the document name', () => {
        const html = renderResults(webview, '<script>.nql', results({
            columns: [{ name: '<script>', type: 'string' }]
        }));

        // The one script in the document is the paging script, opened by the renderer itself.
        assert.equal(html.match(/<script/g)?.length, 1);
        assert.ok(html.includes('&lt;script&gt;'));
    });

    it('sends SQL NULL as JSON null, distinctly from the text NULL', () => {
        const html = renderResults(webview, 'q.nql', results({ rows: [[null], ['NULL']] }));

        assert.ok(html.includes('render([[null],["NULL"]]);'));
    });

    it('reports row count, elapsed time and truncation', () => {
        const html = renderResults(webview, 'q.nql', results({
            rows: [['a'], ['b']],
            truncated: true,
            elapsedMilliseconds: 42
        }));

        assert.ok(html.includes('2 rows'));
        assert.ok(html.includes('42 ms'));
        assert.ok(html.includes('truncated at 2'));
    });

    it('groups thousands in the counts', () => {
        const html = renderResults(webview, 'q.nql', results({ rows: manyRows(12431) }), 500);

        assert.ok(html.includes('12,431 rows'));
        assert.ok(html.includes('of 25'));
    });

    it('uses the singular for one row and omits truncation when not truncated', () => {
        const html = renderResults(webview, 'q.nql', results());

        assert.ok(html.includes('1 row'));
        assert.ok(!html.includes('truncated'));
    });

    it('omits the pager when everything fits on one page', () => {
        const html = renderResults(webview, 'q.nql', results({ rows: manyRows(500) }), 500);

        assert.ok(!html.includes('class="pager"'));
    });

    it('renders a pager bounded by the page count', () => {
        const html = renderResults(webview, 'q.nql', results({ rows: manyRows(1200) }), 500);

        assert.ok(html.includes('class="pager"'));
        assert.ok(html.includes('max="3"'));
        assert.ok(html.includes('const pageCount = 3;'));
        assert.ok(html.includes('const totalRows = 1200;'));
    });

    it('says so when there are no rows', () => {
        const html = renderResults(webview, 'q.nql', results({ rows: [] }));

        assert.ok(html.includes('returned no rows'));
    });

    it('renders an error instead of a grid', () => {
        const html = renderResults(webview, 'q.nql', results({ errorMessage: "Table 'Bogus' is not declared." }));

        assert.ok(html.includes('<div class="error">'));
        assert.ok(html.includes('is not declared.'));
        assert.ok(!html.includes('<table>'));
    });

    it('escapes the error message', () => {
        const html = renderResults(webview, 'q.nql', results({ errorMessage: '<script>alert(1)</script>' }));

        assert.ok(!html.includes('<script>alert(1)'));
    });
});

describe('renderPlan', () => {
    it('nonces its script rather than allowing inline script wholesale', () => {
        const html = renderPlan(webview, 'q.nql', plan());

        const match = html.match(/script-src 'nonce-([A-Za-z0-9]+)'/);
        assert.ok(match, 'expected a nonce-based script CSP');
        assert.ok(html.includes(`<script nonce="${match[1]}">`));
    });

    it('uses a fresh nonce per render', () => {
        const first = renderPlan(webview, 'q.nql', plan()).match(/nonce-([A-Za-z0-9]+)/)![1];
        const second = renderPlan(webview, 'q.nql', plan()).match(/nonce-([A-Za-z0-9]+)/)![1];

        assert.notEqual(first, second);
    });

    it('selects and shows the last step, which is the physical plan', () => {
        const html = renderPlan(webview, 'q.nql', plan({
            steps: [
                { name: 'Unoptimized', root: node() },
                { name: 'Optimized', root: node() },
                { name: 'Physical', root: node() }
            ]
        }));

        assert.ok(html.includes('<option value="2" selected>Physical</option>'));
        assert.ok(html.includes('<option value="0">Unoptimized</option>'));

        // Only the selected step is visible; the others are hidden rather than absent, so
        // switching needs no round trip.
        assert.equal(html.match(/<div class="step" hidden>/g)?.length, 2);
        assert.equal(html.match(/<div class="step" >/g)?.length, 1);
    });

    it('renders a childless node as a leaf and a parent as a disclosure', () => {
        const html = renderPlan(webview, 'q.nql', plan({
            steps: [{ name: 'Physical', root: node({ operatorName: 'Filter', children: [node({ operatorName: 'Table (Customers)' })] }) }]
        }));

        assert.ok(html.includes('<details open>'));
        assert.ok(html.includes('<div class="leaf">'));
        assert.ok(html.includes('Table (Customers)'));
    });

    it('marks scalar subtrees', () => {
        const html = renderPlan(webview, 'q.nql', plan({
            steps: [{ name: 'Physical', root: node({ children: [node({ operatorName: 'Equal', isScalar: true })] }) }]
        }));

        assert.ok(html.includes('<span class="scalar">Equal</span>'));
    });

    it('omits the properties block when a node has none', () => {
        // ShowPlanNode.Properties is empty for most operators, so the common case must not emit
        // an empty container.
        const html = renderPlan(webview, 'q.nql', plan());

        assert.ok(!html.includes('class="props"'));
    });

    it('renders properties when a node has them', () => {
        const html = renderPlan(webview, 'q.nql', plan({
            steps: [{ name: 'Physical', root: node({ properties: [{ name: 'Output', value: 'c.CompanyName' }] }) }]
        }));

        assert.ok(html.includes('Output: c.CompanyName'));
    });

    it('escapes operator names and property values', () => {
        const html = renderPlan(webview, 'q.nql', plan({
            steps: [{
                name: '<step>',
                root: node({ operatorName: '<op>', properties: [{ name: '<n>', value: '<v>' }] })
            }]
        }));

        assert.ok(!html.includes('<op>'));
        assert.ok(!html.includes('<step>'));
        assert.ok(html.includes('&lt;op&gt;'));
    });

    it('renders an error instead of a tree', () => {
        const html = renderPlan(webview, 'q.nql', plan({ steps: [], errorMessage: 'The query has errors.' }));

        assert.ok(html.includes('<div class="error">'));
        assert.ok(!html.includes('<select'));
    });

    it('handles an empty step list without an error message', () => {
        const html = renderPlan(webview, 'q.nql', { steps: [] });

        assert.ok(html.includes('No plan is available.'));
    });

    it('renders deeply nested trees without truncating', () => {
        let root = node({ operatorName: 'Leaf' });
        for (let i = 0; i < 25; i++) {
            root = node({ operatorName: `Level${i}`, children: [root] });
        }

        const html = renderPlan(webview, 'q.nql', plan({ steps: [{ name: 'Physical', root }] }));

        assert.ok(html.includes('Level24'));
        assert.ok(html.includes('Leaf'));
    });
});

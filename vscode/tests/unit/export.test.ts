import * as assert from 'node:assert/strict';
import { describe, it } from 'node:test';

import { ExecuteResult } from '../../src/render';
import { byteOrderMark, fileExtension, formatResults } from '../../src/export';

function results(overrides: Partial<ExecuteResult> = {}): ExecuteResult {
    return {
        columns: [
            { name: 'CompanyName', type: 'string' },
            { name: 'OrderCount', type: 'int' }
        ],
        rows: [
            ['Alfreds Futterkiste', '6'],
            ['Blauer See Delikatessen', '7']
        ],
        truncated: false,
        elapsedMilliseconds: 7,
        ...overrides
    };
}

describe('CSV export', () => {
    it('writes a header row and CRLF line endings', () => {
        const csv = formatResults(results(), 'csv');

        assert.equal(csv, [
            'CompanyName,OrderCount',
            'Alfreds Futterkiste,6',
            'Blauer See Delikatessen,7',
            ''
        ].join('\r\n'));
    });

    it('quotes values containing the delimiter', () => {
        const csv = formatResults(results({ rows: [['Bottom-Dollar Markets, Inc.', '1']] }), 'csv');

        assert.ok(csv.includes('"Bottom-Dollar Markets, Inc.",1'));
    });

    it('doubles embedded quotes', () => {
        const csv = formatResults(results({ rows: [['He said "hi"', '1']] }), 'csv');

        assert.ok(csv.includes('"He said ""hi""",1'));
    });

    it('quotes values containing line breaks', () => {
        // Unquoted, this would silently become two rows.
        const csv = formatResults(results({ rows: [['line one\nline two', '1']] }), 'csv');

        assert.ok(csv.includes('"line one\nline two",1'));
    });

    it('writes NULL as an empty field by default', () => {
        const csv = formatResults(results({ rows: [[null, '1']] }), 'csv');

        assert.ok(csv.includes('\r\n,1\r\n'));
    });

    it('honours a configured null text, quoting it when necessary', () => {
        const csv = formatResults(results({ rows: [[null, '1']] }), 'csv', { nullText: '(null)' });
        assert.ok(csv.includes('(null),1'));

        const quoted = formatResults(results({ rows: [[null, '1']] }), 'csv', { nullText: 'a,b' });
        assert.ok(quoted.includes('"a,b",1'));
    });

    it('honours a configured delimiter and quotes against it', () => {
        const csv = formatResults(results({ rows: [['a;b', '1']] }), 'csv', { delimiter: ';' });

        assert.ok(csv.includes('CompanyName;OrderCount'));
        assert.ok(csv.includes('"a;b";1'));
    });

    it('does not quote a comma when the delimiter is a semicolon', () => {
        const csv = formatResults(results({ rows: [['a,b', '1']] }), 'csv', { delimiter: ';' });

        assert.ok(csv.includes('a,b;1'));
    });

    it('quotes column names that need it', () => {
        const csv = formatResults(results({ columns: [{ name: 'Order, Details', type: 'string' }], rows: [] }), 'csv');

        assert.ok(csv.startsWith('"Order, Details"'));
    });
});

describe('TSV export', () => {
    it('separates with tabs', () => {
        const tsv = formatResults(results(), 'tsv');

        assert.ok(tsv.includes('CompanyName\tOrderCount'));
        assert.ok(tsv.includes('Alfreds Futterkiste\t6'));
    });

    it('does not quote commas', () => {
        const tsv = formatResults(results({ rows: [['Bottom-Dollar Markets, Inc.', '1']] }), 'tsv');

        assert.ok(tsv.includes('Bottom-Dollar Markets, Inc.\t1'));
    });

    it('quotes a value containing a tab', () => {
        // Unquoted, an embedded tab shifts every following column on paste.
        const tsv = formatResults(results({ rows: [['a\tb', '1']] }), 'tsv');

        assert.ok(tsv.includes('"a\tb"\t1'));
    });
});

describe('Markdown export', () => {
    it('writes a table with a separator row', () => {
        const markdown = formatResults(results(), 'markdown');

        assert.equal(markdown, [
            '| CompanyName | OrderCount |',
            '| --- | ---: |',
            '| Alfreds Futterkiste | 6 |',
            '| Blauer See Delikatessen | 7 |',
            ''
        ].join('\n'));
    });

    it('right-aligns numeric columns only', () => {
        const markdown = formatResults(results({
            columns: [
                { name: 'Name', type: 'string' },
                { name: 'Count', type: 'long' },
                { name: 'Freight', type: 'decimal' },
                { name: 'Shipped', type: 'datetime' }
            ],
            rows: []
        }), 'markdown');

        assert.ok(markdown.includes('| --- | ---: | ---: | --- |'));
    });

    it('escapes pipes so they do not end the cell', () => {
        const markdown = formatResults(results({ rows: [['a|b', '1']] }), 'markdown');

        assert.ok(markdown.includes('a\\|b'));
    });

    it('replaces line breaks so they do not end the row', () => {
        const markdown = formatResults(results({ rows: [['line one\r\nline two', '1']] }), 'markdown');

        assert.ok(markdown.includes('line one<br>line two'));
        assert.ok(!markdown.includes('line one\r\n'));
    });

    it('produces nothing for a result with no columns', () => {
        assert.equal(formatResults(results({ columns: [], rows: [] }), 'markdown'), '');
    });
});

describe('export plumbing', () => {
    it('maps formats to file extensions', () => {
        assert.equal(fileExtension('csv'), 'csv');
        assert.equal(fileExtension('tsv'), 'tsv');
        assert.equal(fileExtension('markdown'), 'md');
    });

    it('exposes a byte order mark that is exactly one character', () => {
        // Prepended to CSV files only; Excel on Windows mis-decodes UTF-8 without it.
        assert.equal(byteOrderMark, '﻿');
        assert.equal(byteOrderMark.length, 1);
    });

    it('carries the binary placeholder through rather than inventing data', () => {
        // Binary columns arrive as "byte[1234]" from the server, so an export is a view of the
        // grid, not a data dump.
        const csv = formatResults(results({
            columns: [{ name: 'Picture', type: 'byte[]' }],
            rows: [['byte[10746]']]
        }), 'csv');

        assert.ok(csv.includes('byte[10746]'));
    });
});

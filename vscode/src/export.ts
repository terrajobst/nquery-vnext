import { ExecuteResult, ResultColumn } from './render';

// Result cells arrive as pre-rendered display strings, which is why these are all text formats.
// JSON is deliberately absent: everything would be quoted, including numbers, which looks like
// typed data and is not. That needs typed values from the server, not a client-side guess.

export type ExportFormat = 'csv' | 'tsv' | 'markdown';

export interface ExportOptions {
    /** What an SQL NULL becomes. Empty by default, which is what Excel expects. */
    nullText?: string;
    /** CSV only; ';' in locales where Excel expects it. */
    delimiter?: string;
}

/**
 * Excel on Windows mis-decodes UTF-8 without it, turning any non-ASCII column into mojibake.
 * Only for files -- never prepend it to clipboard text.
 */
export const byteOrderMark = '﻿';

export function formatResults(result: ExecuteResult, format: ExportFormat, options: ExportOptions = {}): string {
    switch (format) {
        case 'csv':
            return toDelimited(result, options.delimiter || ',', options);
        case 'tsv':
            return toDelimited(result, '\t', options);
        case 'markdown':
            return toMarkdown(result, options);
    }
}

export function fileExtension(format: ExportFormat): string {
    return format === 'markdown' ? 'md' : format;
}

/**
 * RFC 4180: quote when the value contains the delimiter, a quote or a line break, and double any
 * embedded quotes. Applied to TSV as well -- a tab or newline inside a cell would otherwise
 * silently shift every following column when pasted.
 */
function toDelimited(result: ExecuteResult, delimiter: string, options: ExportOptions): string {
    const nullText = options.nullText ?? '';
    const lines: string[] = [];

    lines.push(result.columns.map(c => quote(c.name, delimiter)).join(delimiter));

    for (const row of result.rows) {
        lines.push(row.map(cell => quote(cell ?? nullText, delimiter)).join(delimiter));
    }

    // CRLF, because that is what RFC 4180 specifies and what Excel is happiest with.
    return lines.join('\r\n') + '\r\n';
}

function quote(value: string, delimiter: string): string {
    const needsQuoting = value.includes(delimiter)
        || value.includes('"')
        || value.includes('\n')
        || value.includes('\r');

    return needsQuoting ? `"${value.replace(/"/g, '""')}"` : value;
}

function toMarkdown(result: ExecuteResult, options: ExportOptions): string {
    const nullText = options.nullText ?? '';

    if (result.columns.length === 0) {
        return '';
    }

    const alignments = result.columns.map(c => isNumeric(c) ? '---:' : '---');

    const lines = [
        `| ${result.columns.map(c => escapeCell(c.name)).join(' | ')} |`,
        `| ${alignments.join(' | ')} |`
    ];

    for (const row of result.rows) {
        lines.push(`| ${row.map(cell => escapeCell(cell ?? nullText)).join(' | ')} |`);
    }

    return lines.join('\n') + '\n';
}

// A pipe would end the cell, and a line break would end the row.
function escapeCell(value: string): string {
    return value
        .replace(/\|/g, '\\|')
        .replace(/\r\n|\r|\n/g, '<br>');
}

// Numeric columns are right-aligned so the digits line up, which is most of the point of putting
// results in a table. The names match ValueFormatter.FormatTypeName on the server.
function isNumeric(column: ResultColumn): boolean {
    switch (column.type) {
        case 'byte':
        case 'short':
        case 'int':
        case 'long':
        case 'float':
        case 'double':
        case 'decimal':
            return true;
        default:
            return false;
    }
}

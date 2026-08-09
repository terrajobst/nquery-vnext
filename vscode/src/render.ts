// Pure HTML generation for the two webview panels. Nothing here imports `vscode`; the panels
// supply the one thing they need from it (the webview's CSP source) through WebviewLike, which
// keeps this testable in a plain node process.

export interface WebviewLike {
    readonly cspSource: string;
}

export interface ResultColumn {
    name: string;
    type: string;
}

export interface ExecuteResult {
    columns: ResultColumn[];
    /** Pre-rendered display strings; null is SQL NULL. */
    rows: (string | null)[][];
    truncated: boolean;
    elapsedMilliseconds: number;
    errorMessage?: string;
}

export interface ShowPlanProperty {
    name: string;
    value: string;
}

export interface ShowPlanNodeInfo {
    operatorName: string;
    isScalar: boolean;
    properties: ShowPlanProperty[];
    children: ShowPlanNodeInfo[];
}

export interface ShowPlanStep {
    name: string;
    root: ShowPlanNodeInfo;
}

export interface ShowPlanResult {
    steps: ShowPlanStep[];
    errorMessage?: string;
}

export function escapeHtml(value: string): string {
    return value
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

/** Shared chrome: CSP, nonce, and theme-aware base styling. */
export function renderDocument(webview: WebviewLike, title: string, body: string, script?: string): string {
    const nonce = createNonce();

    // Nothing is loaded from anywhere: no remote resources, and no inline script without the nonce.
    const csp = [
        `default-src 'none'`,
        `style-src ${webview.cspSource} 'unsafe-inline'`,
        script ? `script-src 'nonce-${nonce}'` : `script-src 'none'`
    ].join('; ');

    return `<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta http-equiv="Content-Security-Policy" content="${csp}">
<title>${escapeHtml(title)}</title>
<style>${baseStyles}</style>
</head>
<body>
${body}
${script ? `<script nonce="${nonce}">${script}</script>` : ''}
</body>
</html>`;
}

/** Rows shown at once. The whole result is held by the extension host regardless. */
export const defaultPageSize = 500;

export interface ResultPage {
    /** Clamped into range, so a stale or hand-typed page number cannot land outside the result. */
    index: number;
    rows: (string | null)[][];
}

/** Always at least one, so an empty result still has a page zero to show. */
export function pageCount(rowCount: number, pageSize: number): number {
    return Math.max(1, Math.ceil(rowCount / normalizePageSize(pageSize)));
}

export function resultPage(result: ExecuteResult, index: number, pageSize: number): ResultPage {
    const size = normalizePageSize(pageSize);
    const clamped = Math.min(Math.max(Math.trunc(index) || 0, 0), pageCount(result.rows.length, size) - 1);
    const start = clamped * size;

    return { index: clamped, rows: result.rows.slice(start, start + size) };
}

// Settings are not validated for us: a `minimum` in package.json is advice to the settings editor,
// not a guarantee, and zero or a negative would divide the result into infinitely many pages.
function normalizePageSize(pageSize: number): number {
    return Number.isFinite(pageSize) && pageSize >= 1 ? Math.trunc(pageSize) : defaultPageSize;
}

/**
 * Renders the chrome and the first page. Every later page is asked for over `postMessage` and
 * built in the webview, because a result large enough to need paging is also large enough that
 * turning all of it into HTML is the thing that hurts.
 */
export function renderResults(
    webview: WebviewLike,
    name: string,
    result: ExecuteResult,
    pageSize: number = defaultPageSize): string {
    if (result.errorMessage) {
        const body = `
            <div class="toolbar"><span>${escapeHtml(name)}</span></div>
            <div class="error">${escapeHtml(result.errorMessage)}</div>`;
        return renderDocument(webview, 'NQuery Results', body);
    }

    const size = normalizePageSize(pageSize);
    const total = result.rows.length;
    const pages = pageCount(total, size);

    const rowLabel = total === 1 ? '1 row' : `${group(total)} rows`;
    const truncated = result.truncated
        ? `<span class="warning">truncated at ${group(total)}</span>`
        : '';

    // Omitted for a single page: the buttons would all be disabled and the range would repeat the
    // row count that is already in the toolbar.
    const pager = pages === 1 ? '' : `
            <span class="muted" id="range"></span>
            <span class="pager">
                <button type="button" id="first" title="First page" aria-label="First page">&#171;</button>
                <button type="button" id="prev" title="Previous page" aria-label="Previous page">&#8249;</button>
                <label for="page">Page</label>
                <input type="number" id="page" min="1" max="${pages}" value="1" aria-label="Page number">
                <span class="muted">of ${group(pages)}</span>
                <button type="button" id="next" title="Next page" aria-label="Next page">&#8250;</button>
                <button type="button" id="last" title="Last page" aria-label="Last page">&#187;</button>
            </span>`;

    const toolbar = `
        <div class="toolbar">
            <span>${escapeHtml(name)}</span>
            <span class="muted">${rowLabel}</span>
            <span class="muted">${result.elapsedMilliseconds} ms</span>
            ${truncated}
            ${pager}
        </div>`;

    if (result.columns.length === 0) {
        return renderDocument(webview, 'NQuery Results', `${toolbar}<div class="empty">The query returned no columns.</div>`);
    }

    const header = result.columns
        .map(c => `<th>${escapeHtml(c.name)}<span class="type">${escapeHtml(c.type)}</span></th>`)
        .join('');

    const empty = total === 0
        ? '<div class="empty">The query returned no rows.</div>'
        : '';

    const body = `
        ${toolbar}
        <div class="grid-wrapper">
            <table>
                <thead><tr><th class="ordinal"></th>${header}</tr></thead>
                <tbody></tbody>
            </table>
        </div>
        ${empty}`;

    return renderDocument(webview, 'NQuery Results', body, resultsScript(result, size, pages, total));
}

function resultsScript(result: ExecuteResult, pageSize: number, pages: number, total: number): string {
    // The first page is inlined so the grid is never briefly blank; the rest arrive by message.
    const initial = resultPage(result, 0, pageSize);

    // Cells are built with textContent rather than markup, so nothing in the data can be parsed as
    // HTML -- the escaping problem simply does not arise on this path.
    return `
        const api = acquireVsCodeApi();
        const pageSize = ${pageSize};
        const pageCount = ${pages};
        const totalRows = ${total};

        const tbody = document.querySelector('tbody');
        const wrapper = document.querySelector('.grid-wrapper');
        const pager = document.querySelector('.pager');
        const range = document.getElementById('range');
        const input = document.getElementById('page');

        // The page on screen, and the one last asked for -- they differ while a request is in
        // flight, and navigation steps from the latter so a fast double-click advances twice.
        let index = 0;
        let requested = 0;

        // Backslashes are doubled because this source is embedded in a template literal.
        function group(value) {
            return String(value).replace(/\\B(?=(\\d{3})+(?!\\d))/g, ',');
        }

        function render(rows) {
            const fragment = document.createDocumentFragment();
            const first = index * pageSize;

            rows.forEach((row, offset) => {
                const tr = document.createElement('tr');
                const ordinal = document.createElement('td');
                ordinal.className = 'ordinal';
                ordinal.textContent = group(first + offset + 1);
                tr.appendChild(ordinal);

                for (const cell of row) {
                    const td = document.createElement('td');
                    if (cell === null) {
                        td.className = 'null';
                        td.textContent = 'NULL';
                    } else {
                        td.textContent = cell;
                    }
                    tr.appendChild(td);
                }

                fragment.appendChild(tr);
            });

            tbody.replaceChildren(fragment);
            wrapper.scrollTop = 0;
        }

        function update() {
            if (!pager) {
                return;
            }

            const from = index * pageSize + 1;
            const to = Math.min(totalRows, (index + 1) * pageSize);
            range.textContent = 'rows ' + group(from) + '–' + group(to) + ' of ' + group(totalRows);
            input.value = String(index + 1);

            document.getElementById('first').disabled = index === 0;
            document.getElementById('prev').disabled = index === 0;
            document.getElementById('next').disabled = index >= pageCount - 1;
            document.getElementById('last').disabled = index >= pageCount - 1;
        }

        function go(target) {
            if (!Number.isFinite(target)) {
                update();
                return;
            }

            const clamped = Math.max(0, Math.min(pageCount - 1, Math.trunc(target)));
            if (clamped === requested) {
                update();
                return;
            }

            requested = clamped;
            api.postMessage({ type: 'page', index: clamped });
        }

        window.addEventListener('message', event => {
            if (event.data && event.data.type === 'page') {
                index = event.data.index;
                requested = index;
                render(event.data.rows);
                update();
            }
        });

        if (pager) {
            document.getElementById('first').addEventListener('click', () => go(0));
            document.getElementById('prev').addEventListener('click', () => go(requested - 1));
            document.getElementById('next').addEventListener('click', () => go(requested + 1));
            document.getElementById('last').addEventListener('click', () => go(pageCount - 1));
            input.addEventListener('change', () => go(Number(input.value) - 1));
        }

        render(${toScriptJson(initial.rows)});
        update();`;
}

/** `</script>` inside a string literal would end the block, so every `<` is emitted escaped. */
function toScriptJson(value: unknown): string {
    return JSON.stringify(value).replace(/</g, '\\u003c');
}

/** Thousands separators, done here rather than through toLocaleString so output is deterministic. */
function group(value: number): string {
    return String(value).replace(/\B(?=(\d{3})+(?!\d))/g, ',');
}

export function renderPlan(webview: WebviewLike, name: string, result: ShowPlanResult): string {
    if (result.errorMessage || result.steps.length === 0) {
        const message = result.errorMessage ?? 'No plan is available.';
        const body = `
            <div class="toolbar"><span>${escapeHtml(name)}</span></div>
            <div class="error">${escapeHtml(message)}</div>`;
        return renderDocument(webview, 'NQuery Plan', body);
    }

    // Steps run unoptimized -> one entry per optimization pass that changed the tree -> optimized
    // -> physical. The physical plan is what actually runs, so it opens selected.
    const lastIndex = result.steps.length - 1;

    const options = result.steps
        .map((step, index) => `<option value="${index}"${index === lastIndex ? ' selected' : ''}>${escapeHtml(step.name)}</option>`)
        .join('');

    const steps = result.steps
        .map((step, index) => `<div class="step" ${index === lastIndex ? '' : 'hidden'}>${renderNode(step.root)}</div>`)
        .join('');

    const body = `
        <div class="toolbar">
            <span>${escapeHtml(name)}</span>
            <label class="muted" for="step">Step</label>
            <select id="step">${options}</select>
            <span class="muted">${result.steps.length} of ${result.steps.length}</span>
        </div>
        <div class="plan">${steps}</div>`;

    const script = `
        const select = document.getElementById('step');
        const steps = Array.from(document.querySelectorAll('.step'));
        const counter = document.querySelector('.toolbar span:last-of-type');
        function show(index) {
            steps.forEach((s, i) => { s.hidden = i !== index; });
            counter.textContent = (index + 1) + ' of ' + steps.length;
        }
        select.addEventListener('change', () => show(select.selectedIndex));
        show(select.selectedIndex);`;

    return renderDocument(webview, 'NQuery Plan', body, script);
}

function renderNode(node: ShowPlanNodeInfo): string {
    // ShowPlanNode.Properties is empty for most operators -- the detail is encoded in
    // OperatorName (e.g. "Table (Customers), DefinedValues := c.CompanyName:1"), so the name leads
    // and properties are only shown when a node actually has them.
    const label = `<span class="${node.isScalar ? 'scalar' : ''}">${escapeHtml(node.operatorName)}</span>`;

    const properties = node.properties.length === 0
        ? ''
        : `<div class="props">${node.properties
            .map(p => `<div>${escapeHtml(p.name)}: ${escapeHtml(p.value)}</div>`)
            .join('')}</div>`;

    if (node.children.length === 0) {
        return `<div class="leaf">${label}</div>${properties}`;
    }

    const children = node.children.map(renderNode).join('');

    return `<details open>
        <summary>${label}</summary>
        ${properties}
        <div class="children">${children}</div>
    </details>`;
}

function createNonce(): string {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let nonce = '';
    for (let i = 0; i < 32; i++) {
        nonce += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return nonce;
}

// Everything is expressed in VS Code's theme variables, so the panels follow the active colour
// theme without the extension knowing anything about light or dark.
const baseStyles = `
    :root { color-scheme: light dark; }

    body {
        font-family: var(--vscode-font-family);
        font-size: var(--vscode-font-size);
        color: var(--vscode-foreground);
        background-color: var(--vscode-editor-background);
        padding: 0;
        margin: 0;
    }

    .toolbar {
        position: sticky;
        top: 0;
        z-index: 2;
        display: flex;
        flex-wrap: wrap;
        gap: 12px;
        align-items: center;
        padding: 8px 12px;
        /* Opaque for the same reason as the table header. */
        background-color: var(--vscode-editor-background);
        background-image: linear-gradient(
            var(--vscode-editorWidget-background, transparent),
            var(--vscode-editorWidget-background, transparent));
        border-bottom: 1px solid var(--vscode-panel-border, transparent);
    }

    .toolbar .muted { color: var(--vscode-descriptionForeground); }

    /* Pushes the range and the pager to the far end of the toolbar, away from the counts. */
    #range { margin-left: auto; }

    .pager { display: flex; gap: 4px; align-items: center; }

    .pager label { color: var(--vscode-descriptionForeground); }

    .pager button {
        min-width: 24px;
        padding: 1px 6px;
        color: var(--vscode-button-secondaryForeground, var(--vscode-foreground));
        background-color: var(--vscode-button-secondaryBackground, transparent);
        border: 1px solid var(--vscode-contrastBorder, transparent);
        border-radius: 2px;
        cursor: pointer;
    }

    .pager button:hover:enabled { background-color: var(--vscode-button-secondaryHoverBackground, var(--vscode-list-hoverBackground)); }

    .pager button:disabled { opacity: 0.4; cursor: default; }

    .pager input {
        width: 5em;
        color: var(--vscode-input-foreground);
        background-color: var(--vscode-input-background);
        border: 1px solid var(--vscode-input-border, transparent);
        padding: 1px 4px;
        border-radius: 2px;
    }

    .warning {
        color: var(--vscode-editorWarning-foreground, #cca700);
    }

    .error {
        margin: 12px;
        padding: 10px 12px;
        white-space: pre-wrap;
        font-family: var(--vscode-editor-font-family);
        color: var(--vscode-errorForeground);
        background-color: var(--vscode-inputValidation-errorBackground, transparent);
        border: 1px solid var(--vscode-inputValidation-errorBorder, var(--vscode-errorForeground));
        border-radius: 3px;
    }

    .empty { margin: 12px; color: var(--vscode-descriptionForeground); }

    /* Results grid */
    .grid-wrapper { overflow: auto; max-height: calc(100vh - 45px); }

    table { border-collapse: collapse; width: max-content; min-width: 100%; }

    th, td {
        text-align: left;
        padding: 3px 10px;
        border-bottom: 1px solid var(--vscode-panel-border, rgba(128,128,128,0.25));
        border-right: 1px solid var(--vscode-panel-border, rgba(128,128,128,0.25));
        font-family: var(--vscode-editor-font-family);
        font-size: var(--vscode-editor-font-size);
        white-space: pre;
        vertical-align: top;
    }

    /* A sticky header must be fully opaque or rows show through it while scrolling. Theme header
       colours are frequently semi-transparent, and a CSS variable fallback only applies when the
       variable is undefined -- not when it resolves to a translucent colour. So paint the tint as
       a background-image over an opaque background-color. */
    thead th {
        position: sticky;
        top: 0;
        z-index: 1;
        background-color: var(--vscode-editor-background);
        background-image: linear-gradient(
            var(--vscode-keybindingTable-headerBackground, transparent),
            var(--vscode-keybindingTable-headerBackground, transparent));
        box-shadow: inset 0 -1px 0 var(--vscode-panel-border, rgba(128,128,128,0.35));
        font-family: var(--vscode-font-family);
        font-weight: 600;
    }

    thead th .type {
        display: block;
        font-weight: 400;
        font-size: 0.85em;
        color: var(--vscode-descriptionForeground);
    }

    tbody tr:nth-child(even) { background-color: var(--vscode-list-hoverBackground, transparent); }

    td.null { font-style: italic; color: var(--vscode-descriptionForeground); }

    /* The table is stretched to min-width:100%, and auto layout hands the slack to every column
       including this one. width:1% collapses it to its content instead, leaving the slack to the
       real columns. */
    th.ordinal, td.ordinal {
        width: 1%;
        white-space: nowrap;
    }

    td.ordinal {
        color: var(--vscode-editorLineNumber-foreground);
        text-align: right;
        user-select: none;
    }

    /* Plan tree */
    .plan { padding: 10px 14px; }

    .plan details { margin: 0; }

    .plan summary {
        cursor: pointer;
        padding: 2px 0;
        font-family: var(--vscode-editor-font-family);
        font-size: var(--vscode-editor-font-size);
    }

    .plan summary::marker { color: var(--vscode-descriptionForeground); }

    .plan .leaf {
        padding: 2px 0 2px 1.1em;
        font-family: var(--vscode-editor-font-family);
        font-size: var(--vscode-editor-font-size);
    }

    .plan .children { margin-left: 1.1em; border-left: 1px solid var(--vscode-tree-indentGuidesStroke, rgba(128,128,128,0.35)); padding-left: 10px; }

    .plan .scalar { color: var(--vscode-debugTokenExpression-number, var(--vscode-descriptionForeground)); }

    .plan .props { margin: 2px 0 4px 1.1em; color: var(--vscode-descriptionForeground); font-size: 0.9em; }

    .plan .props div { padding: 1px 0; }

    select {
        font-family: var(--vscode-font-family);
        font-size: var(--vscode-font-size);
        color: var(--vscode-dropdown-foreground);
        background-color: var(--vscode-dropdown-background);
        border: 1px solid var(--vscode-dropdown-border, transparent);
        padding: 2px 4px;
        border-radius: 2px;
    }
`;

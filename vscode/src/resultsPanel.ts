import * as path from 'path';
import * as vscode from 'vscode';

import { ExecuteResult, defaultPageSize, renderResults, resultPage } from './render';

export type { ExecuteResult, ResultColumn } from './render';
export { defaultPageSize } from './render';

export interface DisplayedResults {
    documentUri: vscode.Uri;
    result: ExecuteResult;
}

/** One reused panel, like the Markdown preview -- running again updates it instead of stacking. */
export class ResultsPanel {
    private static current: ResultsPanel | undefined;

    private readonly panel: vscode.WebviewPanel;
    private disposed = false;
    private displayed: DisplayedResults | undefined;
    private pageSize = defaultPageSize;

    private constructor(panel: vscode.WebviewPanel) {
        this.panel = panel;
        this.panel.onDidDispose(() => {
            this.disposed = true;
            if (ResultsPanel.current === this) {
                ResultsPanel.current = undefined;
                ResultsPanel.onDidChange?.();
            }
        });

        // The whole result stays here and pages are handed over one at a time. Keeping it on this
        // side is what makes export whole-table: the commands see every row, not the page on
        // screen, and the webview never has to hold more than it is showing.
        this.panel.webview.onDidReceiveMessage((message: { type?: string; index?: number }) => {
            if (message?.type !== 'page' || typeof message.index !== 'number' || !this.displayed) {
                return;
            }

            const page = resultPage(this.displayed.result, message.index, this.pageSize);
            void this.panel.webview.postMessage({ type: 'page', index: page.index, rows: page.rows });
        });
    }

    /** Notified when there are results to export, or no longer are. */
    static onDidChange: (() => void) | undefined;

    /** What the panel is currently showing, if it holds an exportable result. */
    static get results(): DisplayedResults | undefined {
        const current = ResultsPanel.current;

        if (!current || current.disposed || current.displayed?.result.errorMessage) {
            return undefined;
        }

        return current.displayed;
    }

    static show(documentUri: vscode.Uri, result: ExecuteResult, pageSize: number = defaultPageSize): void {
        const existing = ResultsPanel.current;
        const panel = existing && !existing.disposed
            ? existing.panel
            : vscode.window.createWebviewPanel(
                'nquery.results',
                'NQuery Results',
                { viewColumn: vscode.ViewColumn.Beside, preserveFocus: true },
                // Scripts only drive paging; nothing is loaded from outside.
                { enableScripts: true, retainContextWhenHidden: true });

        if (!existing || existing.disposed) {
            ResultsPanel.current = new ResultsPanel(panel);
        }

        const current = ResultsPanel.current!;

        // Set before the HTML, because the webview asks for its second page as soon as it loads.
        current.displayed = { documentUri, result };
        current.pageSize = pageSize;

        const name = path.basename(documentUri.fsPath);
        panel.title = `NQuery Results — ${name}`;
        panel.webview.html = renderResults(panel.webview, name, result, pageSize);
        panel.reveal(panel.viewColumn, true);

        ResultsPanel.onDidChange?.();
    }
}

import * as path from 'path';
import * as vscode from 'vscode';

import { ExecuteResult, renderResults } from './render';

export type { ExecuteResult, ResultColumn } from './render';

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

    private constructor(panel: vscode.WebviewPanel) {
        this.panel = panel;
        this.panel.onDidDispose(() => {
            this.disposed = true;
            if (ResultsPanel.current === this) {
                ResultsPanel.current = undefined;
                ResultsPanel.onDidChange?.();
            }
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

    static show(documentUri: vscode.Uri, result: ExecuteResult): void {
        const existing = ResultsPanel.current;
        const panel = existing && !existing.disposed
            ? existing.panel
            : vscode.window.createWebviewPanel(
                'nquery.results',
                'NQuery Results',
                { viewColumn: vscode.ViewColumn.Beside, preserveFocus: true },
                { enableScripts: false, retainContextWhenHidden: true });

        if (!existing || existing.disposed) {
            ResultsPanel.current = new ResultsPanel(panel);
        }

        const name = path.basename(documentUri.fsPath);
        panel.title = `NQuery Results — ${name}`;
        panel.webview.html = renderResults(panel.webview, name, result);
        panel.reveal(panel.viewColumn, true);

        ResultsPanel.current!.displayed = { documentUri, result };
        ResultsPanel.onDidChange?.();
    }
}

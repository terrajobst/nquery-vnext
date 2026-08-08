import * as path from 'path';
import * as vscode from 'vscode';

import { ShowPlanResult, renderPlan } from './render';

export type { ShowPlanNodeInfo, ShowPlanProperty, ShowPlanResult, ShowPlanStep } from './render';

export class PlanPanel {
    private static current: PlanPanel | undefined;

    private readonly panel: vscode.WebviewPanel;
    private disposed = false;

    private constructor(panel: vscode.WebviewPanel) {
        this.panel = panel;
        this.panel.onDidDispose(() => {
            this.disposed = true;
            if (PlanPanel.current === this) {
                PlanPanel.current = undefined;
            }
        });
    }

    static show(documentUri: vscode.Uri, result: ShowPlanResult): void {
        const existing = PlanPanel.current;
        const panel = existing && !existing.disposed
            ? existing.panel
            : vscode.window.createWebviewPanel(
                'nquery.plan',
                'NQuery Plan',
                { viewColumn: vscode.ViewColumn.Beside, preserveFocus: true },
                // Scripts only drive the step selector; nothing is loaded from outside.
                { enableScripts: true, retainContextWhenHidden: true });

        if (!existing || existing.disposed) {
            PlanPanel.current = new PlanPanel(panel);
        }

        const name = path.basename(documentUri.fsPath);
        panel.title = `NQuery Plan — ${name}`;
        panel.webview.html = renderPlan(panel.webview, name, result);
        panel.reveal(panel.viewColumn, true);
    }
}

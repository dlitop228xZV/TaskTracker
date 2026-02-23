import { ReportsAPI } from './api.js';
import { escapeHtml, formatDate, showLoading, showError } from './main.js';

const el = (id) => document.getElementById(id);

function renderStatus(items) {
  if (!items?.length) {
    el('rStatus').innerHTML = '<p class="text-muted text-center">Нет данных</p>';
    return;
  }
  const rows = items.map(x => `
    <tr>
      <td style="padding:10px; border-bottom:1px solid #e5e7eb;">${escapeHtml(x.status)}</td>
      <td style="padding:10px; border-bottom:1px solid #e5e7eb; text-align:right;">${escapeHtml(x.count)}</td>
    </tr>
  `).join('');
  el('rStatus').innerHTML = `
    <div style="overflow:auto;">
      <table style="width:100%; border-collapse: collapse;">
        <thead>
          <tr style="border-bottom: 1px solid #e5e7eb;">
            <th style="padding:10px; text-align:left;">Статус</th>
            <th style="padding:10px; text-align:right;">Количество</th>
          </tr>
        </thead>
        <tbody>${rows}</tbody>
      </table>
    </div>
  `;
}

function renderOverdue(items) {
  if (!items?.length) {
    el('rOverdue').innerHTML = '<p class="text-muted text-center">Просроченных задач нет</p>';
    return;
  }

  el('rOverdue').innerHTML = items.map(g => {
    const tasks = (g.tasks || []).map(t => `
      <li>
        <strong>${escapeHtml(t.title)}</strong>
        <span class="text-muted">(дедлайн: ${escapeHtml(formatDate(t.dueDate))})</span>
      </li>
    `).join('');

    return `
      <div style="padding: 12px; border: 1px solid #e5e7eb; border-radius: 12px; margin-bottom: 12px;">
        <div style="display:flex; justify-content:space-between; gap: 12px; flex-wrap:wrap;">
          <div><strong>${escapeHtml(g.assignee)}</strong></div>
          <div class="text-muted">Просрочено: <strong>${escapeHtml(g.overdueCount)}</strong></div>
        </div>
        <ul style="margin: 10px 0 0 18px;">${tasks}</ul>
      </div>
    `;
  }).join('');
}

function renderAvg(value) {
  if (value === null || value === undefined) {
    el('rAvg').innerHTML = '<p class="text-muted">Недостаточно данных для расчёта</p>';
    return;
  }
  const days = Number(value);
  el('rAvg').innerHTML = `<p style="font-size:1.2rem; margin:0;"><strong>${escapeHtml(days.toFixed(2))}</strong> дней</p>`;
}

document.addEventListener('DOMContentLoaded', async () => {
  showLoading(el('rStatus'));
  showLoading(el('rOverdue'));
  showLoading(el('rAvg'));

  try {
    const [status, overdue, avg] = await Promise.all([
      ReportsAPI.statusSummary(),
      ReportsAPI.overdueByAssignee(),
      ReportsAPI.avgCompletionTime()
    ]);
    renderStatus(status);
    renderOverdue(overdue);
    renderAvg(avg);
  } catch (e) {
    console.error(e);
    showError(el('rStatus'), 'Не удалось загрузить отчёты.');
    showError(el('rOverdue'), 'Не удалось загрузить отчёты.');
    showError(el('rAvg'), 'Не удалось загрузить отчёты.');
  }
});

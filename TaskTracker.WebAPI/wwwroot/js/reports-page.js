import { apiFetch } from "./api.js";

const els = {
  error: document.getElementById("reportsError"),
  statusTbody: document.getElementById("statusSummaryTbody"),
  bars: document.getElementById("statusBars"),
  overdueTbody: document.getElementById("overdueByAssigneeTbody"),
  overdueEmpty: document.getElementById("overdueEmpty"),
  avgBox: document.getElementById("avgCompletionBox"),
};

init();

async function init() {
  try {
    hideError();
    await Promise.all([
      loadStatusSummary(),
      loadOverdueByAssignee(),
      loadAvgCompletion()
    ]);
  } catch (e) {
    showError(`Ошибка загрузки отчётов: ${e.message}`);
  }
}

async function loadStatusSummary() {
  const items = await apiFetch("/api/reports/status-summary");
  els.statusTbody.innerHTML = "";
  els.bars.innerHTML = "";

  const safe = Array.isArray(items) ? items : [];
  const max = Math.max(1, ...safe.map(x => Number(x.count ?? x.Count ?? 0)));

  for (const it of safe) {
    const status = it.status ?? it.Status ?? "—";
    const count = Number(it.count ?? it.Count ?? 0);

    const tr = document.createElement("tr");
    tr.innerHTML = `<td>${escapeHtml(status)}</td><td>${count}</td>`;
    els.statusTbody.appendChild(tr);

    const row = document.createElement("div");
    row.className = "bar-row";
    row.innerHTML = `
      <div class="muted" style="font-weight:800">${escapeHtml(status)}</div>
      <div class="bar"><span style="width:${Math.round((count / max)*100)}%"></span></div>
      <div style="text-align:right;font-weight:800">${count}</div>
    `;
    els.bars.appendChild(row);
  }

  if (!safe.length) {
    els.statusTbody.innerHTML = `<tr><td colspan="2" class="muted">Нет данных.</td></tr>`;
  }
}

async function loadOverdueByAssignee() {
  const items = await apiFetch("/api/reports/overdue-by-assignee");
  els.overdueTbody.innerHTML = "";

  const safe = Array.isArray(items) ? items : [];
  if (!safe.length) {
    els.overdueEmpty.style.display = "block";
    return;
  }
  els.overdueEmpty.style.display = "none";

  for (const it of safe) {
    const name = it.assigneeName ?? it.AssigneeName ?? "—";
    const count = Number(it.overdueCount ?? it.OverdueCount ?? 0);

    const tasks = it.tasks ?? it.Tasks ?? [];
    const titles = tasks.map(t => t.title ?? t.Title ?? `#${t.id ?? t.Id}`).join(", ");

    const tr = document.createElement("tr");
    tr.innerHTML = `
      <td>${escapeHtml(name)}</td>
      <td>${count}</td>
      <td>${escapeHtml(titles || "—")}</td>
    `;
    els.overdueTbody.appendChild(tr);
  }
}

async function loadAvgCompletion() {
  // API returns number or object? We'll handle both
  const data = await apiFetch("/api/reports/avg-completion-time");
  let value = null;

  if (typeof data === "number") value = data;
  else if (data && typeof data === "object") {
    value = data.averageDays ?? data.AverageDays ?? data.value ?? data.Value ?? null;
  }

  if (value == null || Number.isNaN(Number(value))) {
    els.avgBox.className = "notice";
    els.avgBox.textContent = "Недостаточно данных для расчёта.";
    return;
  }

  els.avgBox.className = "notice";
  els.avgBox.textContent = `Среднее время закрытия: ${Number(value).toFixed(2)} дней.`;
}

function showError(msg){
  els.error.textContent = msg;
  els.error.style.display = "block";
}
function hideError(){
  els.error.textContent = "";
  els.error.style.display = "none";
}

function escapeHtml(str){
  return String(str)
    .replaceAll("&","&amp;")
    .replaceAll("<","&lt;")
    .replaceAll(">","&gt;")
    .replaceAll('"',"&quot;")
    .replaceAll("'","&#039;");
}
import { apiFetch, formatDate, priorityToText, effectiveStatusToBadgeClass, toIsoDate, statusToText } from "./api.js";

const els = {
  createForm: document.getElementById("createTaskForm"),
  createError: document.getElementById("createError"),
  createBtn: document.getElementById("createBtn"),
  resetBtn: document.getElementById("resetBtn"),

  assigneeId: document.getElementById("assigneeId"),
  tagIds: document.getElementById("tagIds"),
  dueDate: document.getElementById("dueDate"),

  filterStatus: document.getElementById("filterStatus"),
  filterAssignee: document.getElementById("filterAssignee"),
  filterDueAfter: document.getElementById("filterDueAfter"),
  filterDueBefore: document.getElementById("filterDueBefore"),
  filterTagIds: document.getElementById("filterTagIds"),
  applyFiltersBtn: document.getElementById("applyFiltersBtn"),
  clearFiltersBtn: document.getElementById("clearFiltersBtn"),

  tbody: document.getElementById("tasksTbody"),
  emptyState: document.getElementById("emptyState"),
  listError: document.getElementById("listError"),

  // modal
  editBackdrop: document.getElementById("editBackdrop"),
  editForm: document.getElementById("editTaskForm"),
  editSubtitle: document.getElementById("editSubtitle"),
  editError: document.getElementById("editError"),
  cancelEditBtn: document.getElementById("cancelEditBtn"),

  editId: document.getElementById("editId"),
  editTitle: document.getElementById("editTitle"),
  editDescription: document.getElementById("editDescription"),
  editAssigneeId: document.getElementById("editAssigneeId"),
  editDueDate: document.getElementById("editDueDate"),
  editPriority: document.getElementById("editPriority"),
  editStatus: document.getElementById("editStatus"),
  editTagIds: document.getElementById("editTagIds"),
};

let cachedUsers = [];
let cachedTags = [];

init();

async function init() {
  // sensible default dueDate = tomorrow
  const tomorrow = new Date(Date.now() + 24*60*60*1000);
  els.dueDate.value = toInputDateValue(tomorrow);

  wireEvents();

  await Promise.all([
    loadUsers(),
    loadTags()
  ]);

  // fill filters after users/tags are loaded
  await refreshTasks();
}

function wireEvents() {
  els.resetBtn.addEventListener("click", () => {
    els.createForm.reset();
    // restore defaults
    els.dueDate.value = toInputDateValue(new Date(Date.now() + 24*60*60*1000));
    document.getElementById("priority").value = "2";
    hideError(els.createError);
  });

  els.createForm.addEventListener("submit", onCreateSubmit);

  els.applyFiltersBtn.addEventListener("click", refreshTasks);
  els.clearFiltersBtn.addEventListener("click", () => {
    els.filterStatus.value = "All";
    els.filterAssignee.value = "All";
    els.filterDueAfter.value = "";
    els.filterDueBefore.value = "";
    [...els.filterTagIds.options].forEach(o => o.selected = false);
    refreshTasks();
  });

  els.cancelEditBtn.addEventListener("click", closeEditModal);
  els.editBackdrop.addEventListener("click", (e) => {
    if (e.target === els.editBackdrop) closeEditModal();
  });

  els.editForm.addEventListener("submit", onEditSubmit);
  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && els.editBackdrop.classList.contains("open")) closeEditModal();
  });
}

async function loadUsers() {
  try {
    cachedUsers = await apiFetch("/api/users");
    fillUsersSelect(els.assigneeId, cachedUsers, true);
    fillUsersSelect(els.filterAssignee, cachedUsers, false, true);
    fillUsersSelect(els.editAssigneeId, cachedUsers, false);

    // when users are loaded, ensure create select has placeholder removed
  } catch (e) {
    showError(els.createError, `Не удалось загрузить пользователей: ${e.message}`);
  }
}

async function loadTags() {
  try {
    cachedTags = await apiFetch("/api/tags");
    fillTagsSelect(els.tagIds, cachedTags);
    fillTagsSelect(els.filterTagIds, cachedTags);
    fillTagsSelect(els.editTagIds, cachedTags);
  } catch (e) {
    // tags are optional for basic CRUD, but we show a warning
    showError(els.createError, `Не удалось загрузить теги: ${e.message}`);
  }
}

function fillUsersSelect(select, users, includePlaceholder, includeAllOption=false) {
  select.innerHTML = "";
  if (includeAllOption) {
    select.appendChild(new Option("All", "All"));
  }
  if (includePlaceholder) {
    select.appendChild(new Option("Выберите пользователя...", "", true, true));
  }
  for (const u of users) {
    const opt = new Option(u.name ?? u.Name ?? `User #${u.id ?? u.Id}`, String(u.id ?? u.Id));
    select.appendChild(opt);
  }
}

function fillTagsSelect(select, tags) {
  select.innerHTML = "";
  for (const t of tags) {
    const opt = new Option(t.name ?? t.Name, String(t.id ?? t.Id));
    select.appendChild(opt);
  }
}

async function refreshTasks() {
  hideError(els.listError);

  try {
    const url = buildTasksUrlFromFilters();
    const tasks = await apiFetch(url);

    renderTasks(tasks || []);
  } catch (e) {
    showError(els.listError, `Не удалось загрузить задачи: ${e.message}`);
    renderTasks([]);
  }
}

function buildTasksUrlFromFilters() {
  const params = new URLSearchParams();

  const status = els.filterStatus.value;
  if (status && status !== "All") {
    // controller accepts string (we'll align backend)
    params.set("status", status);
  }

  const assignee = els.filterAssignee.value;
  if (assignee && assignee !== "All") params.set("assigneeId", assignee);

  const dueAfter = toIsoDate(els.filterDueAfter.value);
  if (dueAfter) params.set("dueAfter", dueAfter);

  const dueBefore = toIsoDate(els.filterDueBefore.value);
  if (dueBefore) params.set("dueBefore", dueBefore);

  const tagIds = [...els.filterTagIds.selectedOptions].map(o => o.value).filter(Boolean);
  for (const id of tagIds) params.append("tagIds", id);

  const qs = params.toString();
  return qs ? `/api/tasks?${qs}` : "/api/tasks";
}

function renderTasks(tasks) {
  els.tbody.innerHTML = "";

  if (!tasks.length) {
    els.emptyState.style.display = "block";
    return;
  }
  els.emptyState.style.display = "none";

  for (const t of tasks) {
    const tr = document.createElement("tr");

    const eff = t.effectiveStatus ?? t.EffectiveStatus ?? statusToText(t.status ?? t.Status);
    if ((eff || "").toLowerCase() === "overdue") tr.classList.add("task-overdue");

    tr.appendChild(tdText(t.title ?? t.Title));
    tr.appendChild(tdText(t.assigneeName ?? t.AssigneeName ?? "—"));
    tr.appendChild(tdStatusBadge(eff));
    tr.appendChild(tdText(formatDate(t.dueDate ?? t.DueDate)));

    const prioVal = t.priority ?? t.Priority;
    tr.appendChild(tdText(priorityToText(prioVal)));

    const tags = t.tags ?? t.Tags ?? [];
    tr.appendChild(tdTags(tags));

    const actionsTd = document.createElement("td");
    actionsTd.className = "actions-cell";
    actionsTd.appendChild(actionButton("Edit", "btn-secondary", () => openEditModal(t.id ?? t.Id)));
    actionsTd.appendChild(actionButton("Delete", "btn-danger", () => deleteTask(t.id ?? t.Id)));
    tr.appendChild(actionsTd);

    els.tbody.appendChild(tr);
  }
}

function tdText(text) {
  const td = document.createElement("td");
  td.textContent = text ?? "";
  return td;
}

function tdTags(tags) {
  const td = document.createElement("td");
  if (!tags || !tags.length) {
    td.innerHTML = '<span class="muted">—</span>';
    return td;
  }
  const wrap = document.createElement("div");
  wrap.className = "tags";
  for (const name of tags) {
    const s = document.createElement("span");
    s.className = "tag";
    s.textContent = name;
    wrap.appendChild(s);
  }
  td.appendChild(wrap);
  return td;
}

function tdStatusBadge(effectiveStatus) {
  const td = document.createElement("td");
  const badge = document.createElement("span");
  const cls = effectiveStatusToBadgeClass(effectiveStatus);
  badge.className = `badge ${cls}`.trim();
  const dot = document.createElement("span");
  dot.className = "badge-dot";
  badge.appendChild(dot);
  badge.appendChild(document.createTextNode(effectiveStatus || "—"));
  td.appendChild(badge);
  return td;
}

function actionButton(text, cls, onClick) {
  const btn = document.createElement("button");
  btn.type = "button";
  btn.className = cls;
  btn.style.marginRight = "6px";
  btn.textContent = text;
  btn.addEventListener("click", (e) => {
    e.stopPropagation();
    onClick();
  });
  return btn;
}

async function onCreateSubmit(e) {
  e.preventDefault();
  hideError(els.createError);

  const title = document.getElementById("title").value.trim();
  const description = document.getElementById("description").value.trim();
  const assigneeId = Number(els.assigneeId.value);
  const dueDate = els.dueDate.value;
  const priority = Number(document.getElementById("priority").value);
  const tagIds = [...els.tagIds.selectedOptions].map(o => Number(o.value)).filter(Boolean);

  if (!title || title.length < 3) {
    showError(els.createError, "Title обязателен (минимум 3 символа).");
    return;
  }
  if (!assigneeId) {
    showError(els.createError, "Выберите исполнителя (Assignee).");
    return;
  }
  if (!dueDate) {
    showError(els.createError, "Укажите DueDate.");
    return;
  }

  const payload = {
    title,
    description,
    assigneeId,
    dueDate: `${dueDate}T00:00:00`,
    priority,
    tagIds
  };

  els.createBtn.disabled = true;
  try {
    await apiFetch("/api/tasks", { method: "POST", body: JSON.stringify(payload) });
    els.createForm.reset();
    els.dueDate.value = toInputDateValue(new Date(Date.now() + 24*60*60*1000));
    document.getElementById("priority").value = "2";
    await refreshTasks();
  } catch (err) {
    showError(els.createError, `Не удалось создать задачу: ${err.message}`);
  } finally {
    els.createBtn.disabled = false;
  }
}

async function deleteTask(id) {
  if (!id) return;
  const ok = confirm("Вы уверены?");
  if (!ok) return;

  try {
    await apiFetch(`/api/tasks/${id}`, { method: "DELETE" });
    await refreshTasks();
  } catch (e) {
    showError(els.listError, `Не удалось удалить задачу: ${e.message}`);
  }
}

async function openEditModal(id) {
  hideError(els.editError);
  els.editSubtitle.textContent = "Загрузка...";
  els.editBackdrop.classList.add("open");

  try {
    const task = await apiFetch(`/api/tasks/${id}`);
    fillEditForm(task);
    els.editSubtitle.textContent = `Task #${id}`;
  } catch (e) {
    showError(els.editError, `Не удалось загрузить задачу: ${e.message}`);
  }
}

function closeEditModal() {
  els.editBackdrop.classList.remove("open");
  els.editForm.reset();
  hideError(els.editError);
}

function fillEditForm(task) {
  const id = task.id ?? task.Id;
  els.editId.value = id;

  els.editTitle.value = task.title ?? task.Title ?? "";
  els.editDescription.value = task.description ?? task.Description ?? "";

  const assigneeId = task.assigneeId ?? task.AssigneeId;
  if (assigneeId != null) els.editAssigneeId.value = String(assigneeId);

  const due = task.dueDate ?? task.DueDate;
  els.editDueDate.value = due ? toInputDateValue(new Date(due)) : "";

  const prio = task.priority ?? task.Priority;
  if (prio != null) els.editPriority.value = String(prio);

  const status = task.status ?? task.Status;
  if (status != null) els.editStatus.value = String(status);

  // Map tag names in task to ids using cachedTags
  const tagNames = task.tags ?? task.Tags ?? [];
  const selectedIds = new Set(
    cachedTags
      .filter(t => tagNames.includes(t.name ?? t.Name))
      .map(t => String(t.id ?? t.Id))
  );
  [...els.editTagIds.options].forEach(o => o.selected = selectedIds.has(o.value));
}

async function onEditSubmit(e) {
  e.preventDefault();
  hideError(els.editError);

  const id = Number(els.editId.value);
  const title = els.editTitle.value.trim();
  const description = els.editDescription.value.trim();
  const assigneeId = Number(els.editAssigneeId.value);
  const dueDate = els.editDueDate.value;
  const priority = Number(els.editPriority.value);
  const status = Number(els.editStatus.value);
  const tagIds = [...els.editTagIds.selectedOptions].map(o => Number(o.value)).filter(Boolean);

  if (!id) return;

  const payload = {
    title,
    description,
    assigneeId: assigneeId || null,
    dueDate: dueDate ? `${dueDate}T00:00:00` : null,
    priority: priority || null,
    status: status || null,
    tagIds
  };

  try {
    await apiFetch(`/api/tasks/${id}`, { method: "PUT", body: JSON.stringify(payload) });
    closeEditModal();
    await refreshTasks();
  } catch (e2) {
    showError(els.editError, `Не удалось сохранить: ${e2.message}`);
  }
}

function showError(el, msg) {
  el.textContent = msg;
  el.style.display = "block";
}
function hideError(el) {
  el.textContent = "";
  el.style.display = "none";
}

function toInputDateValue(date) {
  if (!(date instanceof Date) || Number.isNaN(date.getTime())) return "";
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, "0");
  const dd = String(date.getDate()).padStart(2, "0");
  return `${yyyy}-${mm}-${dd}`;
}
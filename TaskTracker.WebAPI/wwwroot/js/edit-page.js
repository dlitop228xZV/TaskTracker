import { apiFetch } from "./api.js";

const els = {
  error: document.getElementById("editPageError"),
  form: document.getElementById("editPageForm"),
  id: document.getElementById("taskId"),
  title: document.getElementById("pTitle"),
  description: document.getElementById("pDescription"),
  assigneeId: document.getElementById("pAssigneeId"),
  dueDate: document.getElementById("pDueDate"),
  priority: document.getElementById("pPriority"),
  status: document.getElementById("pStatus"),
  tagIds: document.getElementById("pTagIds"),
};

let cachedTags = [];

init();

async function init(){
  const id = Number(new URLSearchParams(location.search).get("id"));
  if (!id) {
    showError("Не указан id. Откройте /edit.html?id=123");
    return;
  }
  els.id.value = id;

  try{
    const [users, tags, task] = await Promise.all([
      apiFetch("/api/users"),
      apiFetch("/api/tags"),
      apiFetch(`/api/tasks/${id}`)
    ]);
    cachedTags = Array.isArray(tags) ? tags : [];
    fillUsers(users);
    fillTags(tags);
    fillTask(task);
    wire();
  } catch(e){
    showError(`Ошибка загрузки: ${e.message}`);
  }
}

function wire(){
  els.form.addEventListener("submit", async (e) => {
    e.preventDefault();
    hideError();

    const id = Number(els.id.value);
    const payload = {
      title: els.title.value.trim(),
      description: els.description.value.trim(),
      assigneeId: Number(els.assigneeId.value) || null,
      dueDate: els.dueDate.value ? `${els.dueDate.value}T00:00:00` : null,
      priority: Number(els.priority.value) || null,
      status: Number(els.status.value) || null,
      tagIds: [...els.tagIds.selectedOptions].map(o => Number(o.value)).filter(Boolean)
    };

    try{
      await apiFetch(`/api/tasks/${id}`, { method:"PUT", body: JSON.stringify(payload) });
      location.href = "/index.html";
    } catch(err){
      showError(`Не удалось сохранить: ${err.message}`);
    }
  });
}

function fillUsers(users){
  els.assigneeId.innerHTML = "";
  for(const u of (users||[])){
    els.assigneeId.appendChild(new Option(u.name ?? u.Name, String(u.id ?? u.Id)));
  }
}
function fillTags(tags){
  els.tagIds.innerHTML = "";
  for(const t of (tags||[])){
    els.tagIds.appendChild(new Option(t.name ?? t.Name, String(t.id ?? t.Id)));
  }
}
function fillTask(task){
  els.title.value = task.title ?? task.Title ?? "";
  els.description.value = task.description ?? task.Description ?? "";
  els.assigneeId.value = String(task.assigneeId ?? task.AssigneeId ?? "");
  els.dueDate.value = toInputDateValue(new Date(task.dueDate ?? task.DueDate));
  els.priority.value = String(task.priority ?? task.Priority ?? "2");
  els.status.value = String(task.status ?? task.Status ?? "1");

  const tagNames = task.tags ?? task.Tags ?? [];
  const selectedIds = new Set(
    cachedTags
      .filter(t => tagNames.includes(t.name ?? t.Name))
      .map(t => String(t.id ?? t.Id))
  );
  [...els.tagIds.options].forEach(o => o.selected = selectedIds.has(o.value));
}

function toInputDateValue(date) {
  if (!(date instanceof Date) || Number.isNaN(date.getTime())) return "";
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, "0");
  const dd = String(date.getDate()).padStart(2, "0");
  return `${yyyy}-${mm}-${dd}`;
}

function showError(msg){
  els.error.textContent = msg;
  els.error.style.display="block";
}
function hideError(){
  els.error.textContent="";
  els.error.style.display="none";
}
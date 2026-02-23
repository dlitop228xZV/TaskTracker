import { TaskAPI, UsersAPI } from './api.js';
import { formatDate, escapeHtml, showLoading, showError } from './main.js';

const el = (id) => document.getElementById(id);

const state = {
  users: [],
  tasks: []
};

function openModal(mode, task = null) {
  el('formError').textContent = '';
  el('modal').style.display = 'flex';
  el('modalTitle').textContent = mode === 'create' ? 'Новая задача' : `Редактирование #${task.id}`;

  el('taskId').value = task?.id ?? '';
  el('title').value = task?.title ?? '';
  el('description').value = task?.description ?? '';
  el('assigneeId').value = String(task?.assigneeId ?? (state.users[0]?.id ?? ''));

  // datetime-local expects "YYYY-MM-DDTHH:mm"
  const due = task?.dueDate ? new Date(task.dueDate) : null;
  el('dueDate').value = due ? new Date(due.getTime() - due.getTimezoneOffset()*60000).toISOString().slice(0,16) : '';

  el('status').value = task?.status ?? 'New';
  el('priority').value = String(task?.priority ?? 2);

  const delBtn = el('btnDelete');
  delBtn.style.display = task ? 'inline-flex' : 'none';
}

function closeModal() {
  el('modal').style.display = 'none';
}

function getFilters() {
  const status = el('fStatus').value || undefined;
  const assigneeId = el('fAssignee').value ? Number(el('fAssignee').value) : undefined;
  const dueAfter = el('fDueAfter').value || undefined;
  const dueBefore = el('fDueBefore').value || undefined;

  // backend принимает DateTime; строка YYYY-MM-DD парсится нормально
  return { status, assigneeId, dueAfter, dueBefore };
}

function renderTable(tasks) {
  if (!tasks.length) {
    el('tasksContainer').innerHTML = '<p class="text-muted text-center">Задач пока нет</p>';
    return;
  }

  const rows = tasks.map(t => {
    const effective = t.effectiveStatus || (t.isOverdue ? 'Overdue' : t.status);
    const statusClass = effective === 'Overdue' ? 'status-overdue' : '';

    return `
      <tr>
        <td>${escapeHtml(t.title)}</td>
        <td>${escapeHtml(t.assigneeName ?? `User#${t.assigneeId}`)}</td>
        <td><span class="task-status ${statusClass}">${escapeHtml(effective)}</span></td>
        <td>${escapeHtml(formatDate(t.dueDate))}</td>
        <td>${escapeHtml(String(t.priority))}</td>
        <td style="text-align:right; white-space:nowrap;">
          <button class="btn" data-action="edit" data-id="${t.id}" type="button" style="border:1px solid #e5e7eb;">Редактировать</button>
        </td>
      </tr>
    `;
  }).join('');

  el('tasksContainer').innerHTML = `
    <div style="overflow:auto;">
      <table style="width:100%; border-collapse: collapse;">
        <thead>
          <tr style="text-align:left; border-bottom: 1px solid #e5e7eb;">
            <th style="padding:10px;">Заголовок</th>
            <th style="padding:10px;">Исполнитель</th>
            <th style="padding:10px;">Статус</th>
            <th style="padding:10px;">Дедлайн</th>
            <th style="padding:10px;">Приоритет</th>
            <th style="padding:10px; text-align:right;">Действия</th>
          </tr>
        </thead>
        <tbody>
          ${rows}
        </tbody>
      </table>
    </div>
  `;

  el('tasksContainer').querySelectorAll('button[data-action="edit"]').forEach(btn => {
    btn.addEventListener('click', () => {
      const id = Number(btn.getAttribute('data-id'));
      const task = state.tasks.find(x => x.id === id);
      if (task) openModal('edit', task);
    });
  });
}

async function loadUsers() {
  state.users = await UsersAPI.getUsers();

  const fill = (selectEl) => {
    selectEl.innerHTML = '<option value="">Все</option>' + state.users
      .map(u => `<option value="${u.id}">${escapeHtml(u.name)} (ID ${u.id})</option>`)
      .join('');
  };

  fill(el('fAssignee'));

  // For form (required)
  const assigneeSelect = el('assigneeId');
  assigneeSelect.innerHTML = state.users
    .map(u => `<option value="${u.id}">${escapeHtml(u.name)} (ID ${u.id})</option>`)
    .join('');
}

async function loadTasks() {
  const container = el('tasksContainer');
  showLoading(container);

  const filters = getFilters();
  const tasks = await TaskAPI.getTasks(filters);

  state.tasks = tasks;
  renderTable(tasks);
}

async function onSubmit(e) {
  e.preventDefault();
  el('formError').textContent = '';

  const id = el('taskId').value ? Number(el('taskId').value) : null;

  const payload = {
    title: el('title').value.trim(),
    description: el('description').value.trim() || null,
    assigneeId: Number(el('assigneeId').value),
    dueDate: new Date(el('dueDate').value).toISOString(),
    status: el('status').value,
    priority: Number(el('priority').value),
    tagIds: []
  };

  try {
    if (!payload.title || payload.title.length < 3) throw new Error('Title: минимум 3 символа');

    if (id) {
      await TaskAPI.updateTask(id, payload);
    } else {
      // create DTO обычно не принимает status (у вас CreateTaskDto?)
      // но backend DTO может игнорировать лишнее; если будет 400 — уберите status.
      await TaskAPI.createTask(payload);
    }

    closeModal();
    await loadTasks();
  } catch (err) {
    el('formError').textContent = err?.message ?? String(err);
  }
}

async function onDelete() {
  const id = el('taskId').value ? Number(el('taskId').value) : null;
  if (!id) return;
  if (!confirm('Удалить задачу?')) return;

  try {
    await TaskAPI.deleteTask(id);
    closeModal();
    await loadTasks();
  } catch (err) {
    el('formError').textContent = err?.message ?? String(err);
  }
}

function wire() {
  el('btnNew').addEventListener('click', () => openModal('create'));
  el('btnClose').addEventListener('click', closeModal);
  el('modal').addEventListener('click', (e) => { if (e.target === el('modal')) closeModal(); });

  el('btnApply').addEventListener('click', loadTasks);
  el('btnReset').addEventListener('click', async () => {
    el('fStatus').value = '';
    el('fAssignee').value = '';
    el('fDueAfter').value = '';
    el('fDueBefore').value = '';
    await loadTasks();
  });

  el('taskForm').addEventListener('submit', onSubmit);
  el('btnDelete').addEventListener('click', onDelete);
}

document.addEventListener('DOMContentLoaded', async () => {
  wire();
  try {
    await loadUsers();
  } catch (e) {
    // Users are needed for create/edit; show hint
    console.warn('Не удалось загрузить пользователей:', e);
  }

  try {
    await loadTasks();
  } catch (e) {
    console.error(e);
    showError(el('tasksContainer'), 'Не удалось загрузить задачи. Проверьте, что API доступен и настроен base URL.');
  }
});

// API client (ES module)

// Если фронтенд и API запущены на разных портах/хостах —
// задайте window.__API_BASE_URL__ в index.html перед импортами, либо через localStorage.
const DEFAULT_BASE = '';

function getBaseUrl() {
  // 1) window override
  if (typeof window !== 'undefined' && window.__API_BASE_URL__) return window.__API_BASE_URL__;
  // 2) localStorage override
  try {
    const v = localStorage.getItem('TASK_TRACKER_API_BASE_URL');
    if (v) return v;
  } catch {}
  // 3) same-origin
  return DEFAULT_BASE;
}

async function request(path, { method = 'GET', query, body } = {}) {
  const base = getBaseUrl();

  const url = new URL((base || '') + path, window.location.origin);
  if (query) {
    Object.entries(query).forEach(([k, v]) => {
      if (v === undefined || v === null || v === '') return;
      if (Array.isArray(v)) v.forEach(x => url.searchParams.append(k, String(x)));
      else url.searchParams.set(k, String(v));
    });
  }

  const res = await fetch(url.toString(), {
    method,
    headers: {
      'Content-Type': 'application/json'
    },
    body: body ? JSON.stringify(body) : undefined
  });

  if (res.status === 204) return null;

  const text = await res.text();
  const data = text ? (() => { try { return JSON.parse(text); } catch { return text; } })() : null;

  if (!res.ok) {
    const msg = typeof data === 'string'
      ? data
      : (data?.error || data?.title || 'Ошибка запроса');
    throw new Error(`${res.status} ${res.statusText}: ${msg}`);
  }

  return data;
}

export const UsersAPI = {
  getUsers: () => request('/api/users')
};

export const TaskAPI = {
  getTasks: (filters = {}) => request('/api/tasks', { query: filters }),
  getTask: (id) => request(`/api/tasks/${id}`),
  createTask: (payload) => request('/api/tasks', { method: 'POST', body: payload }),
  updateTask: (id, payload) => request(`/api/tasks/${id}`, { method: 'PUT', body: payload }),
  deleteTask: (id) => request(`/api/tasks/${id}`, { method: 'DELETE' })
};

export const ReportsAPI = {
  statusSummary: () => request('/api/reports/status-summary'),
  overdueByAssignee: () => request('/api/reports/overdue-by-assignee'),
  avgCompletionTime: () => request('/api/reports/avg-completion-time')
};

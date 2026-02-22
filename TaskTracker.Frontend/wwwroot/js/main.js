// Shared UI helpers (ES module)

export function formatDate(value) {
  if (!value) return '—';
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return String(value);
  return d.toLocaleString('ru-RU', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
}

export function escapeHtml(str) {
  return String(str ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

export function showLoading(container, text = 'Загрузка...') {
  if (!container) return;
  container.innerHTML = `<p class="text-muted text-center">${escapeHtml(text)}</p>`;
}

export function showError(container, message = 'Ошибка') {
  if (!container) return;
  container.innerHTML = `<p class="text-muted text-center" style="color: var(--danger-color)">${escapeHtml(message)}</p>`;
}

export function setActiveNavLink() {
  const currentPath = window.location.pathname.replace(/\\/g, '/');
  document.querySelectorAll('.nav__link').forEach(link => {
    const href = link.getAttribute('href');
    if (!href) return;

    // Works both for /index.html and /pages/tasks.html etc.
    const normalizedHref = href.startsWith('/') ? href : `/${href.replace(/^\.\//, '')}`;
    if (currentPath.endsWith(href) || currentPath === normalizedHref) {
      link.classList.add('nav__link--active');
    } else {
      link.classList.remove('nav__link--active');
    }
  });
}

document.addEventListener('DOMContentLoaded', () => {
  setActiveNavLink();
});

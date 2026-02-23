const defaultHeaders = {
  "Content-Type": "application/json",
  "Accept": "application/json"
};

export async function apiFetch(path, options = {}) {
  const res = await fetch(path, {
    headers: { ...defaultHeaders, ...(options.headers || {}) },
    ...options
  });

  const contentType = res.headers.get("content-type") || "";
  const hasJson = contentType.includes("application/json");

  if (!res.ok) {
    let details = "";
    if (hasJson) {
      try {
        const body = await res.json();
        details = extractApiErrors(body) || JSON.stringify(body);
      } catch { /* ignore */ }
    } else {
      try { details = await res.text(); } catch { /* ignore */ }
    }
    const message = `HTTP ${res.status} ${res.statusText}${details ? ` — ${details}` : ""}`;
    const err = new Error(message);
    err.status = res.status;
    throw err;
  }

  if (res.status === 204) return null;
  return hasJson ? res.json() : res.text();
}

export function extractApiErrors(body) {
  if (!body || typeof body !== "object") return "";
  if (body.errors && typeof body.errors === "object") {
    const parts = [];
    for (const [field, msgs] of Object.entries(body.errors)) {
      if (Array.isArray(msgs)) parts.push(`${field}: ${msgs.join(", ")}`);
    }
    return parts.join(" • ");
  }
  if (typeof body.message === "string") return body.message;
  return "";
}

export function toIsoDate(dateInputValue) {
  // from <input type="date"> -> "YYYY-MM-DD"
  return dateInputValue || null;
}

export function formatDate(isoOrDate) {
  if (!isoOrDate) return "—";
  const d = new Date(isoOrDate);
  if (Number.isNaN(d.getTime())) return String(isoOrDate);
  return d.toLocaleDateString("ru-RU");
}

export function priorityToText(val) {
  const n = Number(val);
  if (n === 1) return "Low";
  if (n === 2) return "Medium";
  if (n === 3) return "High";
  return String(val ?? "");
}

export function statusToText(val) {
  const n = Number(val);
  if (n === 1) return "New";
  if (n === 2) return "InProgress";
  if (n === 3) return "Done";
  return String(val ?? "");
}

export function effectiveStatusToBadgeClass(eff) {
  const s = (eff || "").toLowerCase();
  if (s === "overdue") return "status-overdue";
  if (s === "new") return "status-new";
  if (s === "inprogress") return "status-inprogress";
  if (s === "done") return "status-done";
  return "";
}
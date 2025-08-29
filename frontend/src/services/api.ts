const API_URL = import.meta.env.VITE_API_URL?.replace(/\/$/, '') ?? '';

/**
 * Base helper to call the backend API. Adds the Authorization header if a
 * token is present in localStorage and sets appropriate defaults. Throws
 * an Error for non‑2xx responses with the message extracted from the
 * response body if available.
 */
export async function api(path: string, options: RequestInit = {}) {
  const token = localStorage.getItem('token');
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string> ?? {})
  };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  const res = await fetch(`${API_URL}/api${path}`, {
    ...options,
    headers
  });
  if (!res.ok) {
    let message = res.statusText;
    try {
      const data = await res.json();
      message = data.message || message;
    } catch {
      // ignore JSON parse errors
    }
    throw new Error(message);
  }
  // Attempt to parse JSON; return undefined for NoContent
  try {
    return await res.json();
  } catch {
    return undefined;
  }
}
import React, { useEffect, useState } from 'react';
import { api } from '../services/api';

interface UserRow {
  id: string;
  email: string;
  fullName: string | null;
  roles: string[];
  locked?: boolean;
}

interface Metrics {
  totalUsers: number;
  totalResumes: number;
  last24hResumes: number;
}

const Admin: React.FC = () => {
  const [users, setUsers] = useState<UserRow[]>([]);
  const [metrics, setMetrics] = useState<Metrics | null>(null);
  const [error, setError] = useState<string | null>(null);
  const roles = ['RegisteredUser', 'Admin'];

  const load = async () => {
    setError(null);
    try {
      const [u, m] = await Promise.all([api('/admin/users'), api('/admin/metrics')]);
      setUsers(u);
      setMetrics(m);
    } catch (err: any) {
      setError(err.message);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const changeRole = async (id: string, role: string) => {
    try {
      await api(`/admin/users/${id}/roles`, {
        method: 'POST',
        body: JSON.stringify({ role })
      });
      setUsers((prev) => prev.map((u) => (u.id === id ? { ...u, roles: [role] } : u)));
    } catch (err: any) {
      alert(err.message);
    }
  };

  const toggleLock = async (id: string, locked: boolean) => {
    try {
      await api(`/admin/users/${id}/lock`, {
        method: 'POST',
        body: JSON.stringify({ locked })
      });
      setUsers((prev) => prev.map((u) => (u.id === id ? { ...u, locked } : u)));
    } catch (err: any) {
      alert(err.message);
    }
  };

  return (
    <main className="container" style={{ marginTop: '2rem' }}>
      <h2>Admin Dashboard</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {metrics && (
        <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem' }}>
          <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0' }}>
            <strong>Total Users</strong>
            <div>{metrics.totalUsers}</div>
          </div>
          <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0' }}>
            <strong>Total Resumes</strong>
            <div>{metrics.totalResumes}</div>
          </div>
          <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0' }}>
            <strong>Resumes (24h)</strong>
            <div>{metrics.last24hResumes}</div>
          </div>
        </div>
      )}
      <h3>Users</h3>
      <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr>
              <th style={{ textAlign: 'left', padding: '0.5rem' }}>Email</th>
              <th style={{ textAlign: 'left', padding: '0.5rem' }}>Name</th>
              <th style={{ textAlign: 'left', padding: '0.5rem' }}>Role</th>
              <th style={{ textAlign: 'left', padding: '0.5rem' }}>Lock</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id} style={{ borderTop: '1px solid #e2e8f0' }}>
                <td style={{ padding: '0.5rem' }}>{u.email}</td>
                <td style={{ padding: '0.5rem' }}>{u.fullName ?? ''}</td>
                <td style={{ padding: '0.5rem' }}>
                  <select
                    value={u.roles[0]}
                    onChange={(e) => changeRole(u.id, e.target.value)}
                  >
                    {roles.map((r) => (
                      <option key={r} value={r}>
                        {r}
                      </option>
                    ))}
                  </select>
                </td>
                <td style={{ padding: '0.5rem' }}>
                  <label>
                    <input
                      type="checkbox"
                      checked={u.locked ?? false}
                      onChange={(e) => toggleLock(u.id, e.target.checked)}
                    />{' '}
                    Locked
                  </label>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </main>
  );
};

export default Admin;
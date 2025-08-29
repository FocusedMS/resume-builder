import React, { useEffect, useState } from 'react';
import { AdminApi, User, SystemMetrics, ResumeSummary, ResumeDetail, ResumeStats, TemplateStyle, UserActivity } from '../services/adminApi';

const Admin: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [metrics, setMetrics] = useState<SystemMetrics | null>(null);
  const [resumes, setResumes] = useState<ResumeSummary[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<'overview' | 'users' | 'resumes' | 'analytics'>('overview');
  const [resumePage, setResumePage] = useState(1);
  const [resumeSearch, setResumeSearch] = useState('');
  const [resumeTotal, setResumeTotal] = useState(0);
  const [resumeTotalPages, setResumeTotalPages] = useState(0);
  const [resumeOwnerFilter, setResumeOwnerFilter] = useState('');
  const [resumeTemplateFilter, setResumeTemplateFilter] = useState('');
  const [resumeSortBy, setResumeSortBy] = useState('updatedAt');
  const [resumeSortDir, setResumeSortDir] = useState('desc');
  const [resumeStats, setResumeStats] = useState<ResumeStats | null>(null);
  const [templateStyles, setTemplateStyles] = useState<TemplateStyle[]>([]);
  const [userActivity, setUserActivity] = useState<UserActivity[]>([]);

  const roles = ['RegisteredUser', 'Admin'];

  const loadData = async () => {
    setError(null);
    setLoading(true);
    try {
      const [u, m] = await Promise.all([
        AdminApi.getUsers(), 
        AdminApi.getMetrics()
      ]);
      setUsers(u);
      setMetrics(m);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const loadResumes = async (page = 1, search = '') => {
    try {
      const response = await AdminApi.getAllResumes(
        page, 
        20, 
        search, 
        resumeOwnerFilter, 
        resumeTemplateFilter, 
        resumeSortBy, 
        resumeSortDir
      );
      setResumes(response.items);
      setResumeTotal(response.total);
      setResumeTotalPages(response.totalPages);
      setResumePage(response.page);
    } catch (err: any) {
      setError(err.message);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (activeTab === 'resumes') {
      loadResumes(1, resumeSearch);
    }
    if (activeTab === 'analytics') {
      loadAnalyticsData();
    }
  }, [activeTab, resumeSearch]);

  const loadAnalyticsData = async () => {
    try {
      const [stats, templates, activity] = await Promise.all([
        AdminApi.getResumeStats(),
        AdminApi.getTemplateStyles(),
        AdminApi.getUserActivity()
      ]);
      setResumeStats(stats);
      setTemplateStyles(templates);
      setUserActivity(activity);
    } catch (err: any) {
      setError(err.message);
    }
  };

  const changeRole = async (id: string, role: string) => {
    try {
      await AdminApi.changeRole(id, role);
      setUsers((prev) => prev.map((u) => (u.id === id ? { ...u, roles: [role] } : u)));
    } catch (err: any) {
      alert(err.message);
    }
  };

  const addRole = async (id: string, role: string) => {
    try {
      await AdminApi.addRole(id, role);
      setUsers((prev) => prev.map((u) => 
        u.id === id ? { ...u, roles: [...u.roles, role] } : u
      ));
    } catch (err: any) {
      alert(err.message);
    }
  };

  const removeRole = async (id: string, role: string) => {
    try {
      await AdminApi.removeRole(id, role);
      setUsers((prev) => prev.map((u) => 
        u.id === id ? { ...u, roles: u.roles.filter(r => r !== role) } : u
      ));
    } catch (err: any) {
      alert(err.message);
    }
  };

  const toggleLock = async (id: string, locked: boolean) => {
    try {
      if (locked) {
        await AdminApi.lockUser(id);
      } else {
        await AdminApi.unlockUser(id);
      }
      setUsers((prev) => prev.map((u) => (u.id === id ? { ...u, isLocked: locked } : u)));
    } catch (err: any) {
      alert(err.message);
    }
  };

  const handleResumeSearch = (e: React.FormEvent) => {
    e.preventDefault();
    loadResumes(1, resumeSearch);
  };

  if (loading) return <div className="container" style={{ marginTop: '2rem' }}>Loading...</div>;

  return (
    <main className="container" style={{ marginTop: '2rem' }}>
      <h2>Admin Dashboard</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      
      {/* Tab Navigation */}
      <div style={{ display: 'flex', gap: '1rem', marginBottom: '2rem', borderBottom: '1px solid #e2e8f0' }}>
        <button
          onClick={() => setActiveTab('overview')}
          style={{
            padding: '0.5rem 1rem',
            border: 'none',
            background: activeTab === 'overview' ? 'var(--color-primary)' : 'transparent',
            color: activeTab === 'overview' ? 'white' : 'inherit',
            cursor: 'pointer',
            borderRadius: '4px 4px 0 0'
          }}
        >
          Overview
        </button>
        <button
          onClick={() => setActiveTab('users')}
          style={{
            padding: '0.5rem 1rem',
            border: 'none',
            background: activeTab === 'users' ? 'var(--color-primary)' : 'transparent',
            color: activeTab === 'users' ? 'white' : 'inherit',
            cursor: 'pointer',
            borderRadius: '4px 4px 0 0'
          }}
        >
          Users
        </button>
        <button
          onClick={() => setActiveTab('resumes')}
          style={{
            padding: '0.5rem 1rem',
            border: 'none',
            background: activeTab === 'resumes' ? 'var(--color-primary)' : 'transparent',
            color: activeTab === 'resumes' ? 'white' : 'inherit',
            cursor: 'pointer',
            borderRadius: '4px 4px 0 0'
          }}
        >
          Resumes
        </button>
        <button
          onClick={() => setActiveTab('analytics')}
          style={{
            padding: '0.5rem 1rem',
            border: 'none',
            background: activeTab === 'analytics' ? 'var(--color-primary)' : 'transparent',
            color: activeTab === 'analytics' ? 'white' : 'inherit',
            cursor: 'pointer',
            borderRadius: '4px 4px 0 0'
          }}
        >
          Analytics
        </button>
      </div>

      {/* Overview Tab */}
      {activeTab === 'overview' && metrics && (
        <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem' }}>
          <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0', flex: 1 }}>
            <strong>Total Users</strong>
            <div style={{ fontSize: '2rem', color: 'var(--color-primary)' }}>{metrics.totalUsers}</div>
          </div>
          <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0', flex: 1 }}>
            <strong>Total Resumes</strong>
            <div style={{ fontSize: '2rem', color: 'var(--color-primary)' }}>{metrics.totalResumes}</div>
          </div>
          <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0', flex: 1 }}>
            <strong>Resumes (24h)</strong>
            <div style={{ fontSize: '2rem', color: 'var(--color-primary)' }}>{metrics.last24hResumes}</div>
          </div>
        </div>
      )}

      {/* Users Tab */}
      {activeTab === 'users' && (
        <div>
          <h3>User Management</h3>
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr>
                  <th style={{ textAlign: 'left', padding: '0.5rem' }}>Email</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem' }}>Name</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem' }}>Roles</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem' }}>Status</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr key={u.id} style={{ borderTop: '1px solid #e2e8f0' }}>
                    <td style={{ padding: '0.5rem' }}>{u.email}</td>
                    <td style={{ padding: '0.5rem' }}>{u.fullName ?? ''}</td>
                    <td style={{ padding: '0.5rem' }}>
                      <div style={{ display: 'flex', gap: '0.25rem', flexWrap: 'wrap' }}>
                        {u.roles.map((role) => (
                          <span
                            key={role}
                            style={{
                              background: role === 'Admin' ? '#dc2626' : '#059669',
                              color: 'white',
                              padding: '0.25rem 0.5rem',
                              borderRadius: '4px',
                              fontSize: '0.75rem'
                            }}
                          >
                            {role}
                          </span>
                        ))}
                      </div>
                      <div style={{ marginTop: '0.5rem' }}>
                        <select
                          onChange={(e) => addRole(u.id, e.target.value)}
                          style={{ fontSize: '0.75rem' }}
                        >
                          <option value="">Add role...</option>
                          {roles.filter(r => !u.roles.includes(r)).map((r) => (
                            <option key={r} value={r}>{r}</option>
                          ))}
                        </select>
                      </div>
                    </td>
                    <td style={{ padding: '0.5rem' }}>
                      <span style={{
                        color: u.isLocked ? '#dc2626' : '#059669',
                        fontWeight: 'bold'
                      }}>
                        {u.isLocked ? 'Locked' : 'Active'}
                      </span>
                    </td>
                    <td style={{ padding: '0.5rem' }}>
                      <div style={{ display: 'flex', gap: '0.5rem' }}>
                        <button
                          onClick={() => toggleLock(u.id, !u.isLocked)}
                          style={{
                            padding: '0.25rem 0.5rem',
                            fontSize: '0.75rem',
                            background: u.isLocked ? '#059669' : '#dc2626',
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer'
                          }}
                        >
                          {u.isLocked ? 'Unlock' : 'Lock'}
                        </button>
                        {u.roles.length > 1 && u.roles.map((role) => (
                          <button
                            key={role}
                            onClick={() => removeRole(u.id, role)}
                            style={{
                              padding: '0.25rem 0.5rem',
                              fontSize: '0.75rem',
                              background: '#6b7280',
                              color: 'white',
                              border: 'none',
                              borderRadius: '4px',
                              cursor: 'pointer'
                            }}
                          >
                            Remove {role}
                          </button>
                        ))}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Resumes Tab */}
      {activeTab === 'resumes' && (
        <div>
          <h3>Global Resume Access</h3>
          
          {/* Enhanced Search and Filters */}
          <form onSubmit={handleResumeSearch} style={{ marginBottom: '1rem' }}>
            <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', alignItems: 'center' }}>
              <input
                type="text"
                placeholder="Search resumes..."
                value={resumeSearch}
                onChange={(e) => setResumeSearch(e.target.value)}
                style={{ padding: '0.5rem', width: '250px' }}
              />
              <input
                type="text"
                placeholder="Filter by owner email..."
                value={resumeOwnerFilter}
                onChange={(e) => setResumeOwnerFilter(e.target.value)}
                style={{ padding: '0.5rem', width: '200px' }}
              />
              <select
                value={resumeTemplateFilter}
                onChange={(e) => setResumeTemplateFilter(e.target.value)}
                style={{ padding: '0.5rem', width: '150px' }}
              >
                <option value="">All Templates</option>
                <option value="classic">Classic</option>
                <option value="minimal">Minimal</option>
                <option value="modern">Modern</option>
              </select>
              <select
                value={resumeSortBy}
                onChange={(e) => setResumeSortBy(e.target.value)}
                style={{ padding: '0.5rem', width: '120px' }}
              >
                <option value="updatedAt">Updated</option>
                <option value="createdAt">Created</option>
                <option value="title">Title</option>
                <option value="owner">Owner</option>
              </select>
              <select
                value={resumeSortDir}
                onChange={(e) => setResumeSortDir(e.target.value)}
                style={{ padding: '0.5rem', width: '80px' }}
              >
                <option value="desc">↓</option>
                <option value="asc">↑</option>
              </select>
              <button type="submit" style={{ padding: '0.5rem 1rem' }}>Apply Filters</button>
            </div>
          </form>

          {/* Resume List */}
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr>
                  <th style={{ textAlign: 'left', padding: '0.5rem' }}>Title</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem' }}>Owner</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem' }}>Template</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem' }}>Created</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem' }}>Updated</th>
                </tr>
              </thead>
              <tbody>
                {resumes.map((r) => (
                  <tr key={r.resumeId} style={{ borderTop: '1px solid #e2e8f0' }}>
                    <td style={{ padding: '0.5rem' }}>{r.title}</td>
                    <td style={{ padding: '0.5rem' }}>
                      <div>{r.owner.fullName || 'N/A'}</div>
                      <div style={{ fontSize: '0.75rem', color: '#6b7280' }}>{r.owner.email}</div>
                    </td>
                    <td style={{ padding: '0.5rem' }}>
                      <span style={{
                        background: '#e5e7eb',
                        padding: '0.25rem 0.5rem',
                        borderRadius: '4px',
                        fontSize: '0.75rem',
                        textTransform: 'capitalize'
                      }}>
                        {r.templateStyle}
                      </span>
                    </td>
                    <td style={{ padding: '0.5rem', fontSize: '0.875rem' }}>
                      {new Date(r.createdAt).toLocaleDateString()}
                    </td>
                    <td style={{ padding: '0.5rem', fontSize: '0.875rem' }}>
                      {new Date(r.updatedAt).toLocaleDateString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {resumeTotalPages > 1 && (
            <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'center', marginTop: '1rem' }}>
              <button
                onClick={() => loadResumes(resumePage - 1, resumeSearch)}
                disabled={resumePage <= 1}
                style={{ padding: '0.5rem 1rem' }}
              >
                Previous
              </button>
              <span style={{ padding: '0.5rem' }}>
                Page {resumePage} of {resumeTotalPages}
              </span>
              <button
                onClick={() => loadResumes(resumePage + 1, resumeSearch)}
                disabled={resumePage >= resumeTotalPages}
                style={{ padding: '0.5rem 1rem' }}
              >
                Next
              </button>
            </div>
          )}
        </div>
      )}

      {/* Analytics Tab */}
      {activeTab === 'analytics' && (
        <div>
          <h3>Resume Analytics & Insights</h3>
          
          {/* Resume Statistics */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem', marginBottom: '2rem' }}>
            <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0' }}>
              <strong>Total Resumes</strong>
              <div style={{ fontSize: '1.5rem', color: 'var(--color-primary)' }}>{resumeStats?.totalResumes || 0}</div>
            </div>
            <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0' }}>
              <strong>Last 24 Hours</strong>
              <div style={{ fontSize: '1.5rem', color: '#059669' }}>{resumeStats?.resumesLast24h || 0}</div>
            </div>
            <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0' }}>
              <strong>Last 7 Days</strong>
              <div style={{ fontSize: '1.5rem', color: '#dc2626' }}>{resumeStats?.resumesLast7Days || 0}</div>
            </div>
            <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0' }}>
              <strong>Avg/Day (30d)</strong>
              <div style={{ fontSize: '1.5rem', color: '#7c3aed' }}>{resumeStats?.averageResumesPerDay?.toFixed(1) || 0}</div>
            </div>
          </div>

          {/* Template Usage */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem', marginBottom: '2rem' }}>
            <div>
              <h4>Template Usage</h4>
              <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0' }}>
                {templateStyles.map((template) => (
                  <div key={template.templateStyle} style={{ display: 'flex', justifyContent: 'space-between', padding: '0.5rem 0', borderBottom: '1px solid #f3f4f6' }}>
                    <span style={{ textTransform: 'capitalize' }}>{template.templateStyle}</span>
                    <span style={{ fontWeight: 'bold' }}>{template.count}</span>
                  </div>
                ))}
              </div>
            </div>

            <div>
              <h4>Most Active Users</h4>
              <div style={{ background: '#fff', padding: '1rem', borderRadius: 4, border: '1px solid #e2e8f0' }}>
                {userActivity.slice(0, 5).map((user) => (
                  <div key={user.userId} style={{ display: 'flex', justifyContent: 'space-between', padding: '0.5rem 0', borderBottom: '1px solid #f3f4f6' }}>
                    <span>{user.userName || user.userEmail}</span>
                    <span style={{ fontWeight: 'bold' }}>{user.resumeCount}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}
    </main>
  );
};

export default Admin;
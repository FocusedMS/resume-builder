import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api, download } from '../services/api';
import { useAuth } from '../App';

interface Resume {
  resumeId: number;
  title: string;
  updatedAt: string;
  templateStyle: string;
}

const Dashboard: React.FC = () => {
  const auth = useAuth();
  const [resumes, setResumes] = useState<Resume[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [showAll, setShowAll] = useState(false);
  const [filter, setFilter] = useState('');

  const loadData = async (all: boolean) => {
    setLoading(true);
    setError(null);
    try {
      const query = all ? '?all=1' : '';
      const data = await api(`/resumes${query}`);
      setResumes(data);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData(showAll && auth.role === 'Admin');
  }, [showAll, auth.role]);

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this resume? This action cannot be undone.')) return;
    try {
      await api(`/resumes/${id}`, { method: 'DELETE' });
      setResumes((prev) => prev.filter((r) => r.resumeId !== id));
    } catch (err: any) {
      alert(err.message);
    }
  };

  const handleDownload = async (id: number) => {
    try {
      await download(`/resumes/download/${id}`, `resume-${id}.pdf`);
    } catch (err: any) {
      alert(err.message || 'Download failed');
    }
  };

  const filtered = resumes.filter((r) => r.title.toLowerCase().includes(filter.toLowerCase()));

  return (
    <main className="container" style={{ marginTop: '2rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2>My Resumes</h2>
        {auth.role === 'Admin' && (
          <label style={{ fontSize: '0.9rem' }}>
            <input
              type="checkbox"
              checked={showAll}
              onChange={(e) => setShowAll(e.target.checked)}
              style={{ marginRight: '0.25rem' }}
            />
            Show all resumes
          </label>
        )}
      </div>
      <div style={{ marginTop: '1rem', marginBottom: '1rem' }}>
        <input
          type="text"
          placeholder="Filter by title"
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
        />
      </div>
      {loading && <p>Loading…</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {!loading && !error && (
        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr>
                <th style={{ textAlign: 'left', padding: '0.5rem' }}>Title</th>
                <th style={{ textAlign: 'left', padding: '0.5rem' }}>Updated</th>
                <th style={{ textAlign: 'left', padding: '0.5rem' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((resume) => (
                <tr key={resume.resumeId} style={{ borderTop: '1px solid #e2e8f0' }}>
                  <td style={{ padding: '0.5rem' }}>{resume.title}</td>
                  <td style={{ padding: '0.5rem' }}>{new Date(resume.updatedAt).toLocaleString()}</td>
                  <td style={{ padding: '0.5rem' }}>
                    <Link to={`/builder/${resume.resumeId}`} style={{ marginRight: '0.5rem' }}>
                      Edit
                    </Link>
                    <button onClick={() => handleDownload(resume.resumeId)} style={{ marginRight: '0.5rem' }}>
                      Download
                    </button>
                    <button onClick={() => handleDelete(resume.resumeId)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      <div style={{ marginTop: '1rem' }}>
        <Link to="/builder">
          <button>Create New Resume</button>
        </Link>
      </div>
    </main>
  );
};

export default Dashboard;
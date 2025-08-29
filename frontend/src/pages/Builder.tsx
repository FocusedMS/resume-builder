import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { api, download } from '../services/api';

interface Suggestion {
  section: string;
  priority: string;
  message: string;
  applyTemplate: string;
}

interface ResumeDto {
  title: string;
  personalInfo: string;
  education: string;
  experience: string;
  skills: string;
  templateStyle: string;
}

const MAX_COUNTS = {
  personalInfo: 8000,
  education: 16000,
  experience: 20000,
  skills: 4000
};

const Builder: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isNew = !id;
  const [resume, setResume] = useState<ResumeDto>({
    title: '',
    personalInfo: '',
    education: '',
    experience: '',
    skills: '',
    templateStyle: 'classic'
  });
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [suggestions, setSuggestions] = useState<Suggestion[] | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!isNew) {
      setLoading(true);
      api(`/resumes/${id}`)
        .then((data) => {
          setResume({
            title: data.title ?? '',
            personalInfo: data.personalInfo ?? '',
            education: data.education ?? '',
            experience: data.experience ?? '',
            skills: data.skills ?? '',
            templateStyle: data.templateStyle ?? 'classic'
          });
        })
        .catch((err: any) => setMessage(err.message))
        .finally(() => setLoading(false));
    } else {
      setLoading(false);
    }
  }, [id, isNew]);

  const updateField = (field: keyof ResumeDto, value: string) => {
    const max = (MAX_COUNTS as any)[field];
    if (max && value.length > max) {
      value = value.slice(0, max);
    }
    setResume((prev) => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    setSaving(true);
    setMessage(null);
    try {
      let resp;
      if (isNew) {
        resp = await api('/resumes', {
          method: 'POST',
          body: JSON.stringify(resume)
        });
        // redirect to edit page for new resume
        navigate(`/builder/${resp.resumeId}`);
      } else {
        resp = await api(`/resumes/${id}`, {
          method: 'PUT',
          body: JSON.stringify(resume)
        });
      }
      setMessage('Saved');
    } catch (err: any) {
      setMessage(err.message);
    } finally {
      setSaving(false);
    }
  };

  const handleDownload = async () => {
    if (isNew) {
      setMessage('Please save the resume before downloading');
      return;
    }
    try {
      await download(`/resumes/download/${id}`, `${resume.title || 'resume'}.pdf`);
    } catch (err: any) {
      setMessage(err.message || 'Download failed');
    }
  };

  const handleSuggestions = async () => {
    if (isNew) {
      setMessage('Save the resume before requesting suggestions');
      return;
    }
    try {
      const data = await api(`/resumes/${id}/ai-suggestions`, { method: 'POST' });
      setSuggestions(data.suggestions);
    } catch (err: any) {
      setMessage(err.message);
    }
  };

  const applySuggestion = (s: Suggestion) => {
    const field = s.section[0].toLowerCase() + s.section.slice(1) as keyof ResumeDto;
    updateField(field, (resume as any)[field] + (resume as any)[field] ? `\n${s.applyTemplate}` : s.applyTemplate);
    setSuggestions(null);
  };

  if (loading) return <p className="container">Loading…</p>;

  return (
    <main className="container" style={{ marginTop: '1rem' }}>
      <h2>{isNew ? 'Create Resume' : 'Edit Resume'}</h2>
      {message && <p style={{ color: message === 'Saved' ? 'green' : 'red' }}>{message}</p>}
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '2rem' }}>
        <div style={{ flex: '1 1 300px' }}>
          <label htmlFor="title">Title</label>
          <input
            id="title"
            type="text"
            value={resume.title}
            onChange={(e) => updateField('title', e.target.value)}
            required
            minLength={3}
            maxLength={160}
          />
          <label htmlFor="personalInfo">Personal Info</label>
          <textarea
            id="personalInfo"
            rows={6}
            value={resume.personalInfo}
            onChange={(e) => updateField('personalInfo', e.target.value)}
          />
          <small>
            {resume.personalInfo.length}/{MAX_COUNTS.personalInfo}
          </small>
          <label htmlFor="education">Education</label>
          <textarea
            id="education"
            rows={6}
            value={resume.education}
            onChange={(e) => updateField('education', e.target.value)}
          />
          <small>
            {resume.education.length}/{MAX_COUNTS.education}
          </small>
          <label htmlFor="experience">Experience</label>
          <textarea
            id="experience"
            rows={6}
            value={resume.experience}
            onChange={(e) => updateField('experience', e.target.value)}
          />
          <small>
            {resume.experience.length}/{MAX_COUNTS.experience}
          </small>
          <label htmlFor="skills">Skills (comma separated)</label>
          <textarea
            id="skills"
            rows={3}
            value={resume.skills}
            onChange={(e) => updateField('skills', e.target.value)}
          />
          <small>
            {resume.skills.length}/{MAX_COUNTS.skills}
          </small>
          <label htmlFor="templateStyle">Template Style</label>
          <select
            id="templateStyle"
            value={resume.templateStyle}
            onChange={(e) => updateField('templateStyle', e.target.value)}
          >
            <option value="classic">Classic</option>
            <option value="minimal">Minimal</option>
            <option value="modern">Modern</option>
          </select>
          <div style={{ marginTop: '1rem', display: 'flex', gap: '0.5rem' }}>
            <button type="button" onClick={handleSave} disabled={saving}>
              {saving ? 'Saving…' : 'Save'}
            </button>
            <button type="button" onClick={handleDownload}>Download PDF</button>
            <button type="button" onClick={handleSuggestions}>AI Suggestions</button>
          </div>
        </div>
        {/* Live preview placeholder */}
        <div style={{ flex: '1 1 300px', border: '1px solid #e2e8f0', padding: '1rem', borderRadius: 4, backgroundColor: '#fff' }}>
          <h3 style={{ marginTop: 0 }}>{resume.title || 'Resume Title'}</h3>
          {resume.personalInfo && (
            <>
              <h4>Personal Info</h4>
              <p style={{ whiteSpace: 'pre-wrap' }}>{resume.personalInfo}</p>
            </>
          )}
          {resume.education && (
            <>
              <h4>Education</h4>
              <p style={{ whiteSpace: 'pre-wrap' }}>{resume.education}</p>
            </>
          )}
          {resume.experience && (
            <>
              <h4>Experience</h4>
              <p style={{ whiteSpace: 'pre-wrap' }}>{resume.experience}</p>
            </>
          )}
          {resume.skills && (
            <>
              <h4>Skills</h4>
              <p>{resume.skills}</p>
            </>
          )}
        </div>
      </div>
      {/* Suggestions drawer */}
      {suggestions && (
        <div style={{ marginTop: '2rem', borderTop: '1px solid #e2e8f0', paddingTop: '1rem' }}>
          <h3>AI Suggestions</h3>
          {suggestions.length === 0 && <p>No suggestions available</p>}
          {suggestions.map((s, idx) => (
            <div key={idx} style={{ borderBottom: '1px solid #e2e8f0', padding: '0.5rem 0' }}>
              <strong>{s.section}</strong> ({s.priority})
              <p>{s.message}</p>
              {s.applyTemplate && (
                <button type="button" onClick={() => applySuggestion(s)}>
                  Apply Template
                </button>
              )}
            </div>
          ))}
          <button style={{ marginTop: '1rem' }} onClick={() => setSuggestions(null)}>
            Close
          </button>
        </div>
      )}
    </main>
  );
};

export default Builder;
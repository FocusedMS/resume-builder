import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../App';

const Home: React.FC = () => {
  const auth = useAuth();
  return (
    <main className="container" style={{ marginTop: '2rem' }}>
      <h1>Welcome to the AI‑Powered Resume Builder</h1>
      <p>
        Craft beautiful, personalised resumes with smart suggestions and clean
        templates. Sign up for free and start building your professional
        profile today.
      </p>
      {!auth.token && (
        <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
          <Link to="/register">
            <button>Get Started</button>
          </Link>
          <Link to="/login">
            <button>Log In</button>
          </Link>
        </div>
      )}
      {auth.token && (
        <div style={{ marginTop: '1rem' }}>
          <Link to="/dashboard">
            <button>Go to Dashboard</button>
          </Link>
        </div>
      )}
    </main>
  );
};

export default Home;
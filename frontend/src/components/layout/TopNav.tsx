import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../App';

/**
 * Top navigation bar displayed on every page. Shows links based on the
 * authenticated user’s role and provides a logout button.
 */
const TopNav: React.FC = () => {
  const auth = useAuth();
  const navigate = useNavigate();
  const handleLogout = () => {
    auth.logout();
    navigate('/');
  };
  return (
    <header style={{ backgroundColor: 'var(--color-primary)', color: '#fff' }}>
      <nav className="container" style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '0.75rem' }}>
        <div style={{ fontWeight: 600 }}>
          <Link to="/" style={{ color: '#fff' }}>
            Resume Builder
          </Link>
        </div>
        <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
          {auth.token && (
            <>
              <Link to="/dashboard" style={{ color: '#fff' }}>
                Dashboard
              </Link>
              {auth.role === 'Admin' && (
                <Link to="/admin" style={{ color: '#fff' }}>
                  Admin
                </Link>
              )}
              <button onClick={handleLogout} style={{ background: 'transparent', border: '1px solid #fff', padding: '0.25rem 0.5rem', borderRadius: 4, cursor: 'pointer' }}>
                Logout
              </button>
            </>
          )}
          {!auth.token && (
            <>
              <Link to="/login" style={{ color: '#fff' }}>
                Login
              </Link>
              <Link to="/register" style={{ color: '#fff' }}>
                Register
              </Link>
            </>
          )}
        </div>
      </nav>
    </header>
  );
};

export default TopNav;
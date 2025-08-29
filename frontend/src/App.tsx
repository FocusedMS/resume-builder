import React, { createContext, useContext, useEffect, useState } from 'react';
import { Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import TopNav from './components/layout/TopNav';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import Builder from './pages/Builder';
import Admin from './pages/Admin';

// Type for the authenticated user context
interface AuthState {
  token: string | null;
  role: string | null;
  email: string | null;
  name: string | null;
}

interface AuthContextType extends AuthState {
  login: (data: AuthState) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
};

const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [state, setState] = useState<AuthState>(() => {
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');
    const email = localStorage.getItem('email');
    const name = localStorage.getItem('name');
    return { token, role, email, name };
  });
  const login = (data: AuthState) => {
    setState(data);
    if (data.token) localStorage.setItem('token', data.token);
    if (data.role) localStorage.setItem('role', data.role);
    if (data.email) localStorage.setItem('email', data.email);
    if (data.name) localStorage.setItem('name', data.name);
  };
  const logout = () => {
    setState({ token: null, role: null, email: null, name: null });
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('email');
    localStorage.removeItem('name');
  };
  return (
    <AuthContext.Provider value={{ ...state, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

// Wrapper requiring the user to be authenticated
const RequireAuth: React.FC<{ children: JSX.Element }> = ({ children }) => {
  const auth = useAuth();
  if (!auth.token) {
    return <Navigate to="/login" replace />;
  }
  return children;
};

// Wrapper requiring the user to be an admin
const RequireAdmin: React.FC<{ children: JSX.Element }> = ({ children }) => {
  const auth = useAuth();
  if (auth.role !== 'Admin') {
    return <Navigate to="/" replace />;
  }
  return children;
};

const App: React.FC = () => {
  return (
    <AuthProvider>
      <TopNav />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route
          path="/dashboard"
          element={
            <RequireAuth>
              <Dashboard />
            </RequireAuth>
          }
        />
        <Route
          path="/builder">
          <Route
            index
            element={
              <RequireAuth>
                <Builder />
              </RequireAuth>
            }
          />
          <Route
            path=":id"
            element={
              <RequireAuth>
                <Builder />
              </RequireAuth>
            }
          />
        </Route>
        <Route
          path="/admin"
          element={
            <RequireAuth>
              <RequireAdmin>
                <Admin />
              </RequireAdmin>
            </RequireAuth>
          }
        />
        <Route path="*" element={<Navigate to="/" />} />
      </Routes>
    </AuthProvider>
  );
};

export default App;
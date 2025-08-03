import React from 'react';
import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom';
import { LoginView } from '../views/login/LoginView';
import { useAppSelector } from '../store';

// Componente para proteger rotas que precisam de autenticação
const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAppSelector(state => state.login);
  
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
};

// Componente para redirecionar usuários autenticados da página de login
const PublicRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAppSelector(state => state.login);
  
  return !isAuthenticated ? <>{children}</> : <Navigate to="/dashboard" replace />;
};

// Componente temporário para Dashboard (será implementado depois)
const DashboardView: React.FC = () => (
  <div style={{ 
    padding: '2rem', 
    textAlign: 'center',
    minHeight: '100vh',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center'
  }}>
    <h1>🎉 Dashboard - Login realizado com sucesso!</h1>
    <p>Esta é uma página temporária. O dashboard será implementado em breve.</p>
    <button 
      onClick={() => {
        localStorage.clear();
        window.location.reload();
      }}
      style={{
        padding: '0.75rem 1.5rem',
        background: '#ef4444',
        color: 'white',
        border: 'none',
        borderRadius: '0.5rem',
        cursor: 'pointer',
        marginTop: '1rem'
      }}
    >
      Logout
    </button>
  </div>
);

// Componente para página não encontrada
const NotFoundView: React.FC = () => (
  <div style={{ 
    padding: '2rem', 
    textAlign: 'center',
    minHeight: '100vh',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center'
  }}>
    <h1>404 - Página não encontrada</h1>
    <p>A página que você está procurando não existe.</p>
    <a href="/" style={{ color: '#3b82f6', textDecoration: 'none' }}>
      Voltar ao início
    </a>
  </div>
);

// Configuração das rotas
const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/login" replace />,
  },
  {
    path: '/login',
    element: (
      <PublicRoute>
        <LoginView />
      </PublicRoute>
    ),
  },
  {
    path: '/dashboard',
    element: (
      <ProtectedRoute>
        <DashboardView />
      </ProtectedRoute>
    ),
  },
  {
    path: '*',
    element: <NotFoundView />,
  },
]);

// Componente principal de rotas
export const AppRoutes: React.FC = () => {
  return <RouterProvider router={router} />;
};

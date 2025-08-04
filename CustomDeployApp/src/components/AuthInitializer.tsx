import { useEffect, useRef } from 'react';
import { useAppDispatch, useAppSelector } from '../store';
import { checkAuthState } from '../store/login/actions';

interface AuthInitializerProps {
  children: React.ReactNode;
}

// Variável global para evitar múltiplas execuções
let globalInitRan = false;

export const AuthInitializer: React.FC<AuthInitializerProps> = ({ children }) => {
  const dispatch = useAppDispatch();
  const { isValidatingToken, apiStatus, isAuthenticated } = useAppSelector(state => state.login);
  const isFirstRender = useRef(true);

  useEffect(() => {
    // Só executar na primeira renderização e se ainda não foi executado globalmente
    if (isFirstRender.current && !globalInitRan) {
      isFirstRender.current = false;
      globalInitRan = true;
      console.log('🚀 Inicializando verificação de autenticação...');
      dispatch(checkAuthState());
    }
  }, [dispatch]);

  // Renderizar aplicação normalmente se já está autenticado
  if (isAuthenticated) {
    console.log('✅ AuthInitializer: Usuário já autenticado, renderizando app...');
    return <>{children}</>;
  }

  // Mostrar tela de carregamento apenas durante validação ativa
  if (isValidatingToken) {
    return (
      <div style={{
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
        backgroundColor: '#f5f5f5',
        fontFamily: 'Arial, sans-serif'
      }}>
        <div style={{
          animation: 'spin 1s linear infinite',
          width: '40px',
          height: '40px',
          border: '4px solid #f3f3f3',
          borderTop: '4px solid #3b82f6',
          borderRadius: '50%',
          marginBottom: '20px'
        }} />
        <h2 style={{ color: '#333', margin: '0 0 10px 0' }}>
          Verificando autenticação...
        </h2>
        <p style={{ color: '#666', margin: '0', textAlign: 'center' }}>
          {apiStatus === 'checking' ? 'Verificando conexão com o servidor...' : 'Validando sessão...'}
        </p>
        
        <style>{`
          @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
          }
        `}</style>
      </div>
    );
  }

  // Mostrar erro se API estiver offline
  if (apiStatus === 'offline') {
    return (
      <div style={{
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
        backgroundColor: '#f5f5f5',
        fontFamily: 'Arial, sans-serif',
        padding: '20px'
      }}>
        <div style={{
          backgroundColor: '#fee2e2',
          border: '1px solid #fecaca',
          borderRadius: '8px',
          padding: '20px',
          maxWidth: '400px',
          textAlign: 'center'
        }}>
          <h2 style={{ color: '#dc2626', margin: '0 0 15px 0' }}>
            🚫 Servidor indisponível
          </h2>
          <p style={{ color: '#7f1d1d', margin: '0 0 20px 0' }}>
            Não foi possível conectar com o servidor. Verifique sua conexão e tente novamente.
          </p>
          <button
            onClick={() => {
              globalInitRan = false; // Permitir nova inicialização
              window.location.reload();
            }}
            style={{
              backgroundColor: '#dc2626',
              color: 'white',
              border: 'none',
              borderRadius: '6px',
              padding: '10px 20px',
              cursor: 'pointer',
              fontSize: '14px'
            }}
          >
            Tentar novamente
          </button>
        </div>
      </div>
    );
  }

  // Renderizar aplicação normalmente
  return <>{children}</>;
};

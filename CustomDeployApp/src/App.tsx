import { useEffect } from 'react';
import { Provider } from 'react-redux';
import { store } from './store';
import { AppRoutes } from './infra/routes';
import { tokenMonitor } from './services/tokenMonitor';
import { logoutUser } from './store/login/actions';
import './App.css';

function App() {
  useEffect(() => {
    // Iniciar monitoramento de token quando a aplicação carrega
    const handleTokenExpired = () => {
      console.log('🚨 Token expirado, fazendo logout automático');
      store.dispatch(logoutUser());
    };

    tokenMonitor.startMonitoring(handleTokenExpired);

    // Limpar monitoramento quando componente for desmontado
    return () => {
      tokenMonitor.stopMonitoring();
    };
  }, []);

  return (
    <Provider store={store}>
      <AppRoutes />
    </Provider>
  );
}

export default App;

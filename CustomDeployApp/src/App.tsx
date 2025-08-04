import { useEffect } from 'react';
import { Provider } from 'react-redux';
import { ToastContainer } from 'react-toastify';
import { store } from './store';
import { AppRoutes } from './infra/routes';
import { tokenMonitor } from './services/tokenMonitor';
import { logoutUser } from './store/login/actions';
import './App.css';
import 'react-toastify/dist/ReactToastify.css';

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
      <ToastContainer
        position="top-right"
        autoClose={5000}
        hideProgressBar={false}
        newestOnTop={false}
        closeOnClick
        rtl={false}
        pauseOnFocusLoss
        draggable
        pauseOnHover
        theme="light"
      />
    </Provider>
  );
}

export default App;

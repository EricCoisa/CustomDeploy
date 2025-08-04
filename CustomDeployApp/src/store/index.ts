import { configureStore } from '@reduxjs/toolkit';
import loginReducer from './login/reducers';
import iisReducer from './iis';
import publicationsReducer from './publications';
import deployReducer from './deploy';
import dashboardReducer from './dashboard';

// Configurar a store
export const store = configureStore({
  reducer: {
    login: loginReducer,
    iis: iisReducer,
    publications: publicationsReducer,
    deploy: deployReducer,
    dashboard: dashboardReducer,
    // Adicionar outros reducers aqui conforme necessário
  },
  devTools: import.meta.env.DEV, // Habilitar Redux DevTools apenas em desenvolvimento
});

// Tipos para TypeScript
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

// Hooks tipados para usar com React-Redux
import { useDispatch, useSelector } from 'react-redux';
import { type TypedUseSelectorHook } from 'react-redux';

export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;

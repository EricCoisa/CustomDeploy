import { configureStore } from '@reduxjs/toolkit';
import { persistStore, persistReducer } from 'redux-persist';
import storage from 'redux-persist/lib/storage';
import { combineReducers } from '@reduxjs/toolkit';
import loginReducer from './login/reducers';
import iisReducer from './iis';
import publicationsReducer from './publications';
import deployReducer from './deploy';
import dashboardReducer from './dashboard';
import type { DashboardState } from './dashboard/types';

// Configuração de persistência
const persistConfig = {
  key: 'root',
  storage,
  whitelist: ['login'], // Apenas login será persistido globalmente
};

// Configuração específica para dashboard (com TTL)
const dashboardPersistConfig = {
  key: 'dashboard',
  storage,
  whitelist: ['stats', 'systemStatus', 'recentDeployments', 'lastUpdated'], // Persistir todos os dados importantes
  transforms: [
    // Transform para limpar dados antigos (mais de 10 minutos)
    {
      in: (inboundState: DashboardState) => {
        if (inboundState?.lastUpdated) {
          const lastUpdate = new Date(inboundState.lastUpdated);
          const now = new Date();
          const diffMinutes = (now.getTime() - lastUpdate.getTime()) / (1000 * 60);
          
          // Se os dados são muito antigos (mais de 10 minutos), limpar apenas os dados específicos
          if (diffMinutes > 10) {
            console.log('⏰ Dados do dashboard expirados, limpando cache...');
            return {
              ...inboundState,
              stats: {
                totalDeployments: 0,
                totalSites: 0,
                totalApplications: 0,
                totalAppPools: 0,
              },
              systemStatus: {
                iisStatus: 'unknown',
                apiStatus: 'offline',
                adminStatus: 'unknown',
                githubStatus: 'unknown',
              },
              recentDeployments: [],
              lastUpdated: null,
            };
          }
        }
        return inboundState;
      },
      out: (outboundState: DashboardState) => outboundState,
    },
  ],
};

// Combinar reducers
const rootReducer = combineReducers({
  login: loginReducer,
  iis: iisReducer,
  publications: publicationsReducer,
  deploy: deployReducer,
  dashboard: persistReducer(dashboardPersistConfig, dashboardReducer),
});

// Reducer persistido
const persistedReducer = persistReducer(persistConfig, rootReducer);

// Configurar a store
export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: ['persist/PERSIST', 'persist/REHYDRATE'],
      },
    }),
  devTools: import.meta.env.DEV,
});

// Configurar persistor
export const persistor = persistStore(store);

// Tipos para TypeScript
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

// Hooks tipados para usar com React-Redux
import { useDispatch, useSelector } from 'react-redux';
import { type TypedUseSelectorHook } from 'react-redux';

export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;

import { type Dispatch } from 'redux';
import {
  LOGIN_REQUEST,
  LOGIN_SUCCESS,
  LOGIN_FAILURE,
  AUTO_LOGIN_SUCCESS,
  LOGOUT,
  TOKEN_VALIDATION_START,
  TOKEN_VALIDATION_SUCCESS,
  TOKEN_VALIDATION_FAILURE,
  API_STATUS_UPDATE,
  type LoginCredentials,
  type LoginActionTypes,
  type User,
} from '../types';
import { authService } from '../../../services/authService';

// Action Creators
export const loginRequest = (): LoginActionTypes => ({
  type: LOGIN_REQUEST,
});

export const loginSuccess = (user: User): LoginActionTypes => ({
  type: LOGIN_SUCCESS,
  payload: user,
});

export const autoLoginSuccess = (user: User): LoginActionTypes => ({
  type: AUTO_LOGIN_SUCCESS,
  payload: user,
});

export const loginFailure = (error: string): LoginActionTypes => ({
  type: LOGIN_FAILURE,
  payload: error,
});

export const logout = (): LoginActionTypes => ({
  type: LOGOUT,
});

export const tokenValidationStart = (): LoginActionTypes => ({
  type: TOKEN_VALIDATION_START,
});

export const tokenValidationSuccess = (user: User): LoginActionTypes => ({
  type: TOKEN_VALIDATION_SUCCESS,
  payload: user,
});

export const tokenValidationFailure = (error: string): LoginActionTypes => ({
  type: TOKEN_VALIDATION_FAILURE,
  payload: error,
});

export const updateApiStatus = (status: 'online' | 'offline' | 'checking'): LoginActionTypes => ({
  type: API_STATUS_UPDATE,
  payload: status,
});

// Thunk Action para login usando o authService
export const loginUser = (credentials: LoginCredentials) => {
  return async (dispatch: Dispatch<LoginActionTypes>) => {
    console.log('🔐 Iniciando login para:', credentials.username);
    dispatch(loginRequest());

    try {
      // Usar o authService para fazer login na API real
      console.log('📡 Fazendo requisição para API...');
      const response = await authService.login(credentials);
      console.log('📨 Resposta da API:', response);

      if (response.success && response.data) {
        // Criar objeto user com os dados retornados
        const user: User = {
          username: credentials.username,
          token: response.data.token,
          expiration: response.data.expiration,
        };

        console.log('✅ Login bem-sucedido para:', user.username);
        dispatch(loginSuccess(user));
      } else {
        // Login falhou
        const errorMessage = response.message || 'Credenciais inválidas';
        console.log('❌ Login falhou:', errorMessage);
        dispatch(loginFailure(errorMessage));
      }
    } catch (error) {
      // Erro de rede ou outro erro
      const errorMessage = error instanceof Error ? error.message : 'Erro de conexão com o servidor';
      console.error('🚨 Erro no login:', error);
      dispatch(loginFailure(errorMessage));
    }
  };
};

// Thunk Action para logout usando o authService
export const logoutUser = () => {
  return async (dispatch: Dispatch<LoginActionTypes>) => {
    try {
      await authService.logout();
    } catch (error) {
      // Mesmo se der erro no logout do servidor, limpar estado local
      console.warn('Erro no logout:', error);
    } finally {
      dispatch(logout());
    }
  };
};

// Action melhorada para verificar se usuário está logado e validar com a API
export const checkAuthState = () => {
  return async (dispatch: Dispatch<LoginActionTypes>): Promise<void> => {
    console.log('🔍 Iniciando verificação de autenticação...');
    dispatch(tokenValidationStart());
    
    try {
      // 1. Primeiro, verificar se a API está online
      console.log('📡 Verificando status da API...');
      dispatch(updateApiStatus('checking'));
      
      const healthResponse = await authService.checkHealth();
      if (!healthResponse.success) {
        console.log('❌ API está offline');
        dispatch(updateApiStatus('offline'));
        dispatch(tokenValidationFailure('Servidor indisponível no momento'));
        return;
      }
      
      console.log('✅ API está online');
      dispatch(updateApiStatus('online'));
      
      // 2. Verificar se há token localmente válido
      if (!authService.isTokenValid()) {
        console.log('❌ Token local inválido ou expirado');
        await authService.logout();
        dispatch(tokenValidationFailure('Token expirado'));
        return;
      }
      
      // 3. Validar token com a API
      console.log('🔐 Validando token com a API...');
      const validationResponse = await authService.validateToken();
      
      if (validationResponse.success) {
        // Token é válido
        const user = authService.getCurrentUser();
        if (user) {
          console.log('✅ Token válido, restaurando sessão para:', user.username);
          
          // Limpar dados do dashboard para refresh automático
          console.log('🧹 Limpando cache do dashboard...');
          const dashboardKey = 'persist:dashboard';
          localStorage.removeItem(dashboardKey);
          
          dispatch(tokenValidationSuccess(user));
        } else {
          console.log('❌ Dados do usuário não encontrados');
          await authService.logout();
          dispatch(tokenValidationFailure('Dados do usuário não encontrados'));
        }
      } else {
        // Token inválido na API
        console.log('❌ Token inválido na API');
        await authService.logout();
        dispatch(tokenValidationFailure('Token inválido'));
      }
    } catch (error) {
      // Erro na verificação
      console.error('🚨 Erro na verificação de autenticação:', error);
      
      // Se for erro de conexão, marcar API como offline
      if (error instanceof Error && (error.message.includes('Network Error') || error.message.includes('timeout'))) {
        dispatch(updateApiStatus('offline'));
        dispatch(tokenValidationFailure('Servidor indisponível no momento'));
      } else {
        // Outros erros, limpar sessão
        await authService.logout();
        dispatch(tokenValidationFailure('Erro na verificação de autenticação'));
      }
    }
  };
};
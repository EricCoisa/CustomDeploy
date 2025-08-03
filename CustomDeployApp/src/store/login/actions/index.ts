import { type Dispatch } from 'redux';
import {
  LOGIN_REQUEST,
  LOGIN_SUCCESS,
  LOGIN_FAILURE,
  LOGOUT,
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

export const loginFailure = (error: string): LoginActionTypes => ({
  type: LOGIN_FAILURE,
  payload: error,
});

export const logout = (): LoginActionTypes => ({
  type: LOGOUT,
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

// Action para verificar se usuário está logado (para restaurar estado)
export const checkAuthState = () => {
  return async (dispatch: Dispatch<LoginActionTypes>) => {
    try {
      // Verificar se há token válido e não expirado
      if (authService.isAuthenticated()) {
        const user = authService.getCurrentUser();
        
        if (user) {
          // Verificar na API se o token ainda é válido
          const isTokenValid = await authService.verifyToken();
          
          if (isTokenValid) {
            dispatch(loginSuccess(user));
          } else {
            // Token inválido, fazer logout
            await authService.logout();
            dispatch(logout());
          }
        } else {
          dispatch(logout());
        }
      } else {
        dispatch(logout());
      }
    } catch (error) {
      // Se der erro na verificação, fazer logout
      console.warn('Erro na verificação de autenticação:', error);
      await authService.logout();
      dispatch(logout());
    }
  };
};
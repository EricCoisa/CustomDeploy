import { api, type ApiResponse } from '../utils/api';

// Interfaces para autenticação (compatível com a API do backend)
export interface LoginCredentials {
  username: string; // Backend usa username, não email
  password: string;
}

export interface LoginResponse {
  token: string;
  expiration: string; // Data de expiração do token
}

export interface User {
  username: string; // Compatível com o backend
  token: string;
  expiration: string;
}

// Serviço de autenticação
class AuthService {
  // Login do usuário usando a API real
  async login(credentials: LoginCredentials): Promise<ApiResponse<LoginResponse>> {
    try {
      // Fazer requisição para a API real
      const response = await api.post<LoginResponse>('/auth/login', credentials);
      
      // A API wrapper agora já converte para o formato ApiResponse
      if (response.success && response.data) {
        const { token, expiration } = response.data;
        
        // Salvar token e dados do usuário
        api.setAuthToken(token);
        
        const user: User = {
          username: credentials.username,
          token,
          expiration,
        };
        
        localStorage.setItem('user', JSON.stringify(user));
        localStorage.setItem('tokenExpiration', expiration);
      }
      
      return response;
    } catch (error) {
      // Tratar erros da API
      console.error('Erro no authService.login:', error);
      const errorMessage = error instanceof Error ? error.message : 'Erro de conexão';
      
      return {
        success: false,
        data: {} as LoginResponse,
        message: errorMessage,
      };
    }
  }

  // Logout do usuário
  async logout(): Promise<void> {
    try {
      // Opcional: notificar o servidor sobre logout (se implementado)
      // await api.post('/auth/logout');
    } catch (error) {
      // Continuar mesmo se houver erro no servidor
      console.warn('Erro ao fazer logout no servidor:', error);
    } finally {
      // Sempre limpar dados locais
      api.removeAuthToken();
      localStorage.removeItem('user');
      localStorage.removeItem('tokenExpiration');
    }
  }

  // Verificar se usuário está autenticado
  isAuthenticated(): boolean {
    return this.isTokenValid();
  }

  // Obter dados do usuário logado
  getCurrentUser(): User | null {
    if (!this.isAuthenticated()) {
      return null;
    }
    
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        return JSON.parse(userStr) as User;
      } catch {
        return null;
      }
    }
    return null;
  }

  // Verificar validade do token localmente (sem endpoint no backend)
  isTokenValid(): boolean {
    const token = api.getAuthToken();
    const expiration = localStorage.getItem('tokenExpiration');
    
    if (!token || !expiration) {
      return false;
    }
    
    // Verificar se token não expirou
    const expirationDate = new Date(expiration);
    const now = new Date();
    
    // Adicionar margem de 1 minuto para evitar problemas de sincronização
    const expirationWithMargin = new Date(expirationDate.getTime() - 60 * 1000);
    
    if (now >= expirationWithMargin) {
      return false;
    }
    
    return true;
  }

  // Obter token atual
  getToken(): string | null {
    return api.getAuthToken();
  }

  // Verificar se token está próximo do vencimento
  isTokenExpiring(): boolean {
    const expiration = localStorage.getItem('tokenExpiration');
    if (!expiration) return true;
    
    const expirationDate = new Date(expiration);
    const now = new Date();
    const fiveMinutesFromNow = new Date(now.getTime() + 5 * 60 * 1000);
    
    return expirationDate <= fiveMinutesFromNow;
  }
}

// Exportar instância única
export const authService = new AuthService();

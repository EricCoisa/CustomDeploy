import axios, { type AxiosInstance, type AxiosRequestConfig, type AxiosResponse, AxiosError } from 'axios';
import { toast } from 'react-toastify';

// Configuração base da API
const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5092';

// Tipos para a API
export type RequestData = Record<string, unknown> | FormData | string | number | boolean | null | object;

// Interface para tipagem de respostas da API
export interface ApiResponse<T = unknown> {
  data: T;
  message?: string;
  success: boolean;
}

// Interface para erros da API
interface ApiErrorResponse {
  message: string;
  errors?: Record<string, string[]>;
}

// Criar instância do Axios
const apiClient: AxiosInstance = axios.create({
  baseURL: BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para adicionar token de autenticação automaticamente
apiClient.interceptors.request.use(
  (config) => {
    // Buscar token do localStorage
    const token = localStorage.getItem('authToken');
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

// Interceptor para tratar respostas e erros
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    return response;
  },
  (error: AxiosError) => {
    // Se token expirou ou inválido (401 Unauthorized)
    if (error.response?.status === 401) {
      console.log('🚨 Token inválido ou expirado (401), limpando dados locais');
      
      // Mostrar toast para erro de autenticação
      toast.warn('Sessão expirada. Você será redirecionado para o login.', {
        position: "top-center",
        autoClose: 3000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
      });
      
      // Limpar dados de autenticação
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
      localStorage.removeItem('tokenExpiration');
      
      // Se não estivermos na página de login, redirecionar
      if (window.location.pathname !== '/login' && window.location.pathname !== '/') {
        console.log('🔄 Redirecionando para login...');
        setTimeout(() => {
          window.location.href = '/login';
        }, 3000);
      }
    }
    
    return Promise.reject(error);
  }
);

// Classe API com métodos HTTP
class ApiService {
  // Método para mostrar toast de sucesso
  private showSuccessToast(message?: string): void {
    if (message && message !== 'Requisição realizada com sucesso') {
      toast.success(message, {
        position: "top-right",
        autoClose: 3000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
      });
    }
  }

  // GET
  async get<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await apiClient.get<T>(url, config);
      
      // Verificar se a resposta já está no formato ApiResponse
      if (response.data && typeof response.data === 'object' && 'success' in response.data) {
        return response.data as unknown as ApiResponse<T>;
      }
      
      // Se não está no formato ApiResponse, converter para o formato padrão
      return {
        success: true,
        data: response.data,
        message: 'Requisição realizada com sucesso',
      };
    } catch (error) {
      throw this.handleError(error as AxiosError);
    }
  }

  // POST
  async post<T = unknown>(url: string, data?: RequestData, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await apiClient.post<T>(url, data, config);
      
      // Verificar se a resposta já está no formato ApiResponse
      if (response.data && typeof response.data === 'object' && 'success' in response.data) {
        const apiResponse = response.data as unknown as ApiResponse<T>;
        this.showSuccessToast(apiResponse.message);
        return apiResponse;
      }
      
      // Se não está no formato ApiResponse, converter para o formato padrão
      const apiResponse = {
        success: true,
        data: response.data,
        message: 'Requisição realizada com sucesso',
      };
      this.showSuccessToast(apiResponse.message);
      return apiResponse;
    } catch (error) {
      throw this.handleError(error as AxiosError);
    }
  }

  // PUT
  async put<T = unknown>(url: string, data?: RequestData, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await apiClient.put<ApiResponse<T>>(url, data, config);
      this.showSuccessToast(response.data.message);
      return response.data;
    } catch (error) {
      throw this.handleError(error as AxiosError);
    }
  }

  // PATCH
  async patch<T = unknown>(url: string, data?: RequestData, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await apiClient.patch<ApiResponse<T>>(url, data, config);
      this.showSuccessToast(response.data.message);
      return response.data;
    } catch (error) {
      throw this.handleError(error as AxiosError);
    }
  }

  // DELETE
  async delete<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await apiClient.delete<ApiResponse<T>>(url, config);
      this.showSuccessToast(response.data.message);
      return response.data;
    } catch (error) {
      throw this.handleError(error as AxiosError);
    }
  }

  // Método para fazer upload de arquivos
  async upload<T = unknown>(url: string, formData: FormData, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    try {
      const response = await apiClient.post<ApiResponse<T>>(url, formData, {
        ...config,
        headers: {
          'Content-Type': 'multipart/form-data',
          ...config?.headers,
        },
      });
      this.showSuccessToast(response.data.message);
      return response.data;
    } catch (error) {
      throw this.handleError(error as AxiosError);
    }
  }

  // Métodos de autenticação
  setAuthToken(token: string): void {
    localStorage.setItem('authToken', token);
  }

  getAuthToken(): string | null {
    return localStorage.getItem('authToken');
  }

  removeAuthToken(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
  }

  isAuthenticated(): boolean {
    return !!this.getAuthToken();
  }

  // Tratamento de erros
  private handleError(error: AxiosError): Error {
    let errorMessage = '';
    
    // Debug detalhado
    console.error('🚨 Erro na API:', {
      message: error.message,
      code: error.code,
      request: !!error.request,
      response: !!error.response,
      responseStatus: error.response?.status,
      responseData: error.response?.data,
      config: {
        url: error.config?.url,
        method: error.config?.method,
        baseURL: error.config?.baseURL,
        timeout: error.config?.timeout
      }
    });
    
    if (error.response) {
      // Erro da API
      const errorData = error.response.data as ApiErrorResponse;
      errorMessage = errorData?.message || 'Erro na requisição';
      
      // Adicionar detalhes de erros de validação se existirem
      if (errorData?.errors) {
        const validationErrors = Object.values(errorData.errors).flat().join(', ');
        errorMessage += ` - ${validationErrors}`;
      }
    } else if (error.request) {
      // Erro de rede
      console.error('🔥 Erro de rede detectado:', {
        readyState: error.request.readyState,
        status: error.request.status,
        responseURL: error.request.responseURL
      });
      errorMessage = 'Erro de conexão com o servidor';
    } else {
      // Erro desconhecido
      errorMessage = 'Erro desconhecido';
    }

    // Exibir toast de erro (apenas se não for erro 401 - token expirado)
    if (error.response?.status !== 401) {
      toast.error(errorMessage, {
        position: "top-right",
        autoClose: 5000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
      });
    }

    return new Error(errorMessage);
  }
}

// Exportar instância única
export const api = new ApiService();

// Exportar cliente Axios para casos específicos
export { apiClient };
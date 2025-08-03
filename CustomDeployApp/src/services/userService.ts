import { api, type ApiResponse } from '../utils/api';

// Exemplo de interface para um usuário
interface User {
  id: string;
  name: string;
  email: string;
  createdAt: string;
}

// Exemplo de interface para criar usuário
interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
}

// Exemplo de interface para atualizar usuário
interface UpdateUserRequest {
  name?: string;
  email?: string;
}

// Exemplo de serviço de usuários usando a API
class UserService {
  // Listar usuários
  async getUsers(): Promise<ApiResponse<User[]>> {
    return await api.get<User[]>('/users');
  }

  // Buscar usuário por ID
  async getUserById(id: string): Promise<ApiResponse<User>> {
    return await api.get<User>(`/users/${id}`);
  }

  // Criar novo usuário
  async createUser(userData: CreateUserRequest): Promise<ApiResponse<User>> {
    return await api.post<User>('/users', userData);
  }

  // Atualizar usuário
  async updateUser(id: string, userData: UpdateUserRequest): Promise<ApiResponse<User>> {
    return await api.put<User>(`/users/${id}`, userData);
  }

  // Atualizar parcialmente usuário
  async patchUser(id: string, userData: Partial<UpdateUserRequest>): Promise<ApiResponse<User>> {
    return await api.patch<User>(`/users/${id}`, userData);
  }

  // Deletar usuário
  async deleteUser(id: string): Promise<ApiResponse<void>> {
    return await api.delete<void>(`/users/${id}`);
  }

  // Upload de avatar do usuário
  async uploadAvatar(userId: string, file: File): Promise<ApiResponse<{ avatarUrl: string }>> {
    const formData = new FormData();
    formData.append('avatar', file);
    
    return await api.upload<{ avatarUrl: string }>(`/users/${userId}/avatar`, formData);
  }

  // Buscar usuários com paginação e filtros
  async searchUsers(params: {
    page?: number;
    limit?: number;
    search?: string;
    role?: string;
  }): Promise<ApiResponse<{
    users: User[];
    total: number;
    page: number;
    totalPages: number;
  }>> {
    const queryParams = new URLSearchParams();
    
    if (params.page) queryParams.append('page', params.page.toString());
    if (params.limit) queryParams.append('limit', params.limit.toString());
    if (params.search) queryParams.append('search', params.search);
    if (params.role) queryParams.append('role', params.role);

    return await api.get(`/users/search?${queryParams.toString()}`);
  }
}

// Exemplo de como usar o serviço
export const userService = new UserService();

// Exemplo de uso em um componente:
/*
import { userService } from '../services/userService';

const MyComponent = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const response = await userService.getUsers();
      
      if (response.success) {
        setUsers(response.data);
      } else {
        setError(response.message || 'Erro ao carregar usuários');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro desconhecido');
    } finally {
      setLoading(false);
    }
  };

  const createUser = async (userData: CreateUserRequest) => {
    try {
      const response = await userService.createUser(userData);
      
      if (response.success) {
        // Atualizar lista ou redirecionar
        await fetchUsers();
      } else {
        setError(response.message || 'Erro ao criar usuário');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro desconhecido');
    }
  };

  return (
    // JSX do componente
  );
};
*/

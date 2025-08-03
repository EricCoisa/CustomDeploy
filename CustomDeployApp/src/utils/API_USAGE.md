# 📡 API Service - Guia de Uso

Este documento explica como usar o sistema de API configurado com Axios e autenticação automática.

## 🚀 Configuração

A API está configurada em `src/utils/api.ts` com:

- **Base URL**: `process.env.VITE_API_URL` ou `http://localhost:5000/api`
- **Timeout**: 10 segundos
- **Autenticação automática**: Token JWT do localStorage
- **Interceptors** para tratamento de erros e token expirado

## 🔐 Sistema de Autenticação

### Token Management
```typescript
import { api } from '../utils/api';

// Salvar token
api.setAuthToken('seu-jwt-token');

// Obter token
const token = api.getAuthToken();

// Remover token
api.removeAuthToken();

// Verificar se está autenticado
const isAuth = api.isAuthenticated();
```

### Interceptors Automáticos
- **Request**: Adiciona automaticamente `Authorization: Bearer {token}` em todas as requisições
- **Response**: Redireciona para `/login` automaticamente se receber erro 401

## 📝 Métodos HTTP Disponíveis

### GET
```typescript
// Simples
const users = await api.get<User[]>('/users');

// Com parâmetros
const user = await api.get<User>('/users/123');

// Com configuração personalizada
const response = await api.get<User[]>('/users', {
  headers: { 'Custom-Header': 'value' }
});
```

### POST
```typescript
// Criar usuário
const newUser = await api.post<User>('/users', {
  name: 'João Silva',
  email: 'joao@email.com'
});

// Login
const loginResponse = await api.post<LoginResponse>('/auth/login', {
  email: 'user@email.com',
  password: 'senha123'
});
```

### PUT
```typescript
// Atualizar usuário completo
const updatedUser = await api.put<User>('/users/123', {
  name: 'João Santos',
  email: 'joao.santos@email.com'
});
```

### PATCH
```typescript
// Atualizar usuário parcial
const updatedUser = await api.patch<User>('/users/123', {
  name: 'Novo Nome'
});
```

### DELETE
```typescript
// Deletar usuário
await api.delete('/users/123');
```

### UPLOAD
```typescript
// Upload de arquivo
const formData = new FormData();
formData.append('file', file);

const result = await api.upload<{url: string}>('/upload', formData);
```

## 🛡️ Tratamento de Erros

### Estrutura de Resposta
```typescript
interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}
```

### Exemplo de Uso com Tratamento de Erro
```typescript
try {
  const response = await api.get<User[]>('/users');
  
  if (response.success) {
    console.log('Usuários:', response.data);
  } else {
    console.error('Erro:', response.message);
  }
} catch (error) {
  console.error('Erro na requisição:', error.message);
}
```

## 🔧 Configuração de Ambiente

Crie um arquivo `.env` na raiz do projeto:

```env
VITE_API_URL=http://localhost:5000/api
```

## 📋 Exemplos de Serviços

### AuthService
```typescript
import { api } from '../utils/api';

class AuthService {
  async login(credentials: {email: string, password: string}) {
    const response = await api.post('/auth/login', credentials);
    
    if (response.success) {
      api.setAuthToken(response.data.token);
      localStorage.setItem('user', JSON.stringify(response.data.user));
    }
    
    return response;
  }

  async logout() {
    await api.post('/auth/logout');
    api.removeAuthToken();
  }
}

export const authService = new AuthService();
```

### UserService
```typescript
import { api } from '../utils/api';

class UserService {
  async getUsers() {
    return await api.get<User[]>('/users');
  }

  async createUser(userData: CreateUserRequest) {
    return await api.post<User>('/users', userData);
  }

  async updateUser(id: string, userData: UpdateUserRequest) {
    return await api.put<User>(`/users/${id}`, userData);
  }

  async deleteUser(id: string) {
    return await api.delete(`/users/${id}`);
  }
}

export const userService = new UserService();
```

## 🎯 Boas Práticas

1. **Sempre use tipos TypeScript** para as respostas da API
2. **Trate erros adequadamente** com try/catch
3. **Verifique `response.success`** antes de usar os dados
4. **Use serviços específicos** para cada domínio (auth, users, etc.)
5. **Configure variáveis de ambiente** para diferentes ambientes
6. **Mantenha tokens seguros** no localStorage/sessionStorage

## 🚨 Segurança

- Tokens são automaticamente incluídos nas requisições
- Redirecionamento automático para login em caso de token expirado
- Headers de segurança configurados
- Timeout para evitar requisições lentas

## 📱 Uso em React Components

```typescript
import { useState, useEffect } from 'react';
import { userService } from '../services/userService';

const UserList = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        const response = await userService.getUsers();
        
        if (response.success) {
          setUsers(response.data);
        } else {
          setError(response.message || 'Erro ao carregar usuários');
        }
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchUsers();
  }, []);

  if (loading) return <div>Carregando...</div>;
  if (error) return <div>Erro: {error}</div>;

  return (
    <div>
      {users.map(user => (
        <div key={user.id}>{user.name}</div>
      ))}
    </div>
  );
};
```

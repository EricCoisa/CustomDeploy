// Tipos para o estado de login (compatível com a API do backend)
export interface User {
  username: string; // Backend usa username
  token: string;
  expiration: string;
}

export interface LoginState {
  isLoading: boolean;
  error: string | null;
  isAuthenticated: boolean;
  user: User | null;
}

// Tipos para as actions (compatível com a API)
export interface LoginCredentials {
  username: string; // Mudou de email para username
  password: string;
}

// Action Types
export const LOGIN_REQUEST = 'LOGIN_REQUEST' as const;
export const LOGIN_SUCCESS = 'LOGIN_SUCCESS' as const;
export const LOGIN_FAILURE = 'LOGIN_FAILURE' as const;
export const LOGOUT = 'LOGOUT' as const;

// Action Interfaces
export interface LoginRequestAction {
  type: typeof LOGIN_REQUEST;
}

export interface LoginSuccessAction {
  type: typeof LOGIN_SUCCESS;
  payload: User;
}

export interface LoginFailureAction {
  type: typeof LOGIN_FAILURE;
  payload: string;
}

export interface LogoutAction {
  type: typeof LOGOUT;
}

export type LoginActionTypes =
  | LoginRequestAction
  | LoginSuccessAction
  | LoginFailureAction
  | LogoutAction;

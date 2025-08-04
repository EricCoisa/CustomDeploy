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
  isAutoLogin: boolean; // Indica se foi login automático por token
  isValidatingToken: boolean; // Indica se está validando token na inicialização
  apiStatus: 'online' | 'offline' | 'checking'; // Status da API
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
export const AUTO_LOGIN_SUCCESS = 'AUTO_LOGIN_SUCCESS' as const;
export const LOGOUT = 'LOGOUT' as const;
export const TOKEN_VALIDATION_START = 'TOKEN_VALIDATION_START' as const;
export const TOKEN_VALIDATION_SUCCESS = 'TOKEN_VALIDATION_SUCCESS' as const;
export const TOKEN_VALIDATION_FAILURE = 'TOKEN_VALIDATION_FAILURE' as const;
export const API_STATUS_UPDATE = 'API_STATUS_UPDATE' as const;

// Action Interfaces
export interface LoginRequestAction {
  type: typeof LOGIN_REQUEST;
}

export interface LoginSuccessAction {
  type: typeof LOGIN_SUCCESS;
  payload: User;
}

export interface AutoLoginSuccessAction {
  type: typeof AUTO_LOGIN_SUCCESS;
  payload: User;
}

export interface LoginFailureAction {
  type: typeof LOGIN_FAILURE;
  payload: string;
}

export interface LogoutAction {
  type: typeof LOGOUT;
}

export interface TokenValidationStartAction {
  type: typeof TOKEN_VALIDATION_START;
}

export interface TokenValidationSuccessAction {
  type: typeof TOKEN_VALIDATION_SUCCESS;
  payload: User;
}

export interface TokenValidationFailureAction {
  type: typeof TOKEN_VALIDATION_FAILURE;
  payload: string;
}

export interface ApiStatusUpdateAction {
  type: typeof API_STATUS_UPDATE;
  payload: 'online' | 'offline' | 'checking';
}

export type LoginActionTypes =
  | LoginRequestAction
  | LoginSuccessAction
  | AutoLoginSuccessAction
  | LoginFailureAction
  | LogoutAction
  | TokenValidationStartAction
  | TokenValidationSuccessAction
  | TokenValidationFailureAction
  | ApiStatusUpdateAction;

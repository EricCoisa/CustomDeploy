// 🌐 URLs da API
export const API_ENDPOINTS = {
  // Autenticação
  AUTH: {
    LOGIN: '/auth/login',
    LOGOUT: '/auth/logout',
    REFRESH: '/auth/refresh',
    VERIFY: '/auth/verify',
    PROFILE: '/auth/profile',
    CHANGE_PASSWORD: '/auth/change-password',
    FORGOT_PASSWORD: '/auth/forgot-password',
    RESET_PASSWORD: '/auth/reset-password',
  },
  
  // Usuários
  USERS: {
    LIST: '/users',
    DETAIL: (id: string) => `/users/${id}`,
    CREATE: '/users',
    UPDATE: (id: string) => `/users/${id}`,
    DELETE: (id: string) => `/users/${id}`,
    AVATAR: (id: string) => `/users/${id}/avatar`,
    SEARCH: '/users/search',
  },
  
  // Sistema (antigo Deploy Controller)
  SYSTEM: {
    LIST: '/system',
    CREATE: '/system',
    DETAIL: (id: string) => `/system/${id}`,
    STATUS: (id: string) => `/system/${id}/status`,
    LOGS: (id: string) => `/system/${id}/logs`,
    CREDENTIALS_STATUS: '/system/credentials/status',
    CREDENTIALS_TEST: '/system/credentials/test',
    REPOSITORY_VALIDATE: '/system/repository/validate',
  },
  
  // IIS (exemplo para o projeto CustomDeploy)
  IIS: {
    SITES: '/iis/sites',
    SITE_DETAIL: (name: string) => `/iis/sites/${name}`,
    APPLICATIONS: '/iis/applications',
    POOLS: '/iis/pools',
  },
} as const;

// 🔐 Configurações de Autenticação
export const AUTH_CONFIG = {
  TOKEN_KEY: 'authToken',
  USER_KEY: 'user',
  REFRESH_TOKEN_KEY: 'refreshToken',
  TOKEN_EXPIRY_KEY: 'tokenExpiry',
} as const;

// ⏱️ Timeouts e Intervalos
export const TIMEOUTS = {
  API_REQUEST: 10000, // 10 segundos
  FILE_UPLOAD: 30000, // 30 segundos
  REFRESH_TOKEN: 300000, // 5 minutos
} as const;

// 📱 Configurações da Aplicação
export const APP_CONFIG = {
  NAME: 'CustomDeploy',
  VERSION: '1.0.0',
  DEFAULT_PAGE_SIZE: 20,
  MAX_FILE_SIZE: 10 * 1024 * 1024, // 10MB
  SUPPORTED_FILE_TYPES: ['.zip', '.tar.gz', '.rar'] as const,
} as const;

// 🎨 Temas e UI
export const UI_CONFIG = {
  SIDEBAR_WIDTH: 250,
  HEADER_HEIGHT: 60,
  FOOTER_HEIGHT: 40,
  BREAKPOINTS: {
    MOBILE: 768,
    TABLET: 1024,
    DESKTOP: 1200,
  },
} as const;

// 📊 Status e Estados
export const STATUS = {
  DEPLOY: {
    PENDING: 'pending',
    IN_PROGRESS: 'in_progress',
    SUCCESS: 'success',
    FAILED: 'failed',
    CANCELLED: 'cancelled',
  },
  IIS: {
    RUNNING: 'running',
    STOPPED: 'stopped',
    STARTING: 'starting',
    STOPPING: 'stopping',
  },
  USER: {
    ACTIVE: 'active',
    INACTIVE: 'inactive',
    PENDING: 'pending',
    BLOCKED: 'blocked',
  },
} as const;

// 🌍 Configurações de Localização
export const LOCALE_CONFIG = {
  DEFAULT_LANGUAGE: 'pt-BR',
  SUPPORTED_LANGUAGES: ['pt-BR', 'en-US'] as const,
  DATE_FORMAT: 'DD/MM/YYYY HH:mm:ss',
  CURRENCY: 'BRL',
} as const;

// 🚨 Mensagens de Erro Padrão
export const ERROR_MESSAGES = {
  NETWORK: 'Erro de conexão com o servidor',
  UNAUTHORIZED: 'Acesso não autorizado',
  FORBIDDEN: 'Acesso negado',
  NOT_FOUND: 'Recurso não encontrado',
  INTERNAL_SERVER: 'Erro interno do servidor',
  VALIDATION: 'Dados inválidos',
  TIMEOUT: 'Tempo limite da requisição excedido',
  FILE_TOO_LARGE: 'Arquivo muito grande',
  INVALID_FILE_TYPE: 'Tipo de arquivo não suportado',
} as const;

// ✅ Mensagens de Sucesso Padrão
export const SUCCESS_MESSAGES = {
  LOGIN: 'Login realizado com sucesso',
  LOGOUT: 'Logout realizado com sucesso',
  SAVE: 'Dados salvos com sucesso',
  DELETE: 'Item removido com sucesso',
  UPDATE: 'Dados atualizados com sucesso',
  UPLOAD: 'Upload realizado com sucesso',
  DEPLOY: 'Deploy realizado com sucesso',
} as const;

// 🔄 Configurações de Refresh e Polling
export const POLLING_CONFIG = {
  DEPLOY_STATUS: 5000, // 5 segundos
  IIS_STATUS: 10000, // 10 segundos
  NOTIFICATIONS: 30000, // 30 segundos
} as const;

// 📋 Configurações de Formulários
export const FORM_CONFIG = {
  VALIDATION: {
    MIN_PASSWORD_LENGTH: 8,
    MAX_USERNAME_LENGTH: 50,
    EMAIL_REGEX: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
    PHONE_REGEX: /^\(\d{2}\)\s\d{4,5}-\d{4}$/,
  },
  DEBOUNCE_DELAY: 300, // 300ms
} as const;

// 🎯 Rotas da Aplicação
export const ROUTES = {
  HOME: '/',
  LOGIN: '/login',
  DASHBOARD: '/dashboard',
  USERS: '/users',
  DEPLOY: '/deploy',
  SYSTEM: '/system',
  IIS: '/iis',
  SETTINGS: '/settings',
  PROFILE: '/profile',
  NOT_FOUND: '/404',
} as const;

// 🔧 Configurações de Desenvolvimento
export const DEV_CONFIG = {
  ENABLE_CONSOLE_LOGS: import.meta.env.DEV,
  ENABLE_REDUX_DEVTOOLS: import.meta.env.DEV,
  API_MOCK_DELAY: 1000, // 1 segundo
} as const;

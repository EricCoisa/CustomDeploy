// Tipos para o gerenciamento IIS

export interface IISBinding {
  protocol: string;
  ipAddress: string;
  port: number;
  hostName: string;
  bindingInformation: string;
}

export interface IISSite {
  id: number;
  name: string;
  state: string;
  bindings: IISBinding[];
  physicalPath: string;
  appPoolName: string;
  applications?: IISApplication[];
}

export interface IISApplication {
  name: string;
  physicalPath: string;
  applicationPool: string;
  enabledProtocols: string;
  state?: string;
}

export interface IISAppPool {
  name: string;
  state: string;
  managedRuntimeVersion: string;
  managedPipelineMode: string;
  processModel: {
    identityType: string;
    maxProcesses: number;
    idleTimeout: string;
  };
  recycling: {
    regularTimeInterval: string;
    memory: {
      privateMemory: number;
      virtualMemory: number;
    };
  };
}

// Request types para criação e atualização
export interface CreateSiteRequest {
  siteName: string;
  bindingInformation: string;
  physicalPath: string;
  appPoolName: string;
}

export interface UpdateSiteRequest {
  bindingInformation?: string;
  physicalPath?: string;
  appPoolName?: string;
}

export interface CreateApplicationRequest {
  siteName: string;
  appPath: string;
  physicalPath: string;
  appPoolName: string;
}

export interface UpdateApplicationRequest {
  physicalPath?: string;
  appPoolName?: string;
}

export interface CreateAppPoolRequest {
  poolName: string;
  managedRuntimeVersion?: string;
  pipelineMode?: string;
  identityType?: string;
  maxProcesses?: number;
  idleTimeout?: string;
  regularTimeInterval?: string;
  privateMemory?: number;
  virtualMemory?: number;
}

export interface UpdateAppPoolRequest {
  managedRuntimeVersion?: string;
  pipelineMode?: string;
  identityType?: string;
  maxProcesses?: number;
  idleTimeout?: string;
  regularTimeInterval?: string;
  privateMemory?: number;
  virtualMemory?: number;
}

// Estado do Redux
export interface IISState {
  sites: IISSite[];
  appPools: IISAppPool[];
  loading: {
    sites: boolean;
    appPools: boolean;
    applications: boolean;
  };
  error: {
    sites: string | null;
    appPools: string | null;
    applications: string | null;
  };
  permissions: {
    canCreateFolders: boolean;
    canMoveFiles: boolean;
    canExecuteIISCommands: boolean;
    canManageIIS: boolean;
    allPermissionsGranted: boolean;
  } | null;
  isAdministrator: boolean | null;
}

// Tipos para respostas da API
export interface PermissionCheckResult {
  canCreateFolders: boolean;
  canMoveFiles: boolean;
  canExecuteIISCommands: boolean;
  canManageIIS: boolean;
  allPermissionsGranted: boolean;
  testDetails: string[];
  instructions: string[];
}

export interface AdminStatusResult {
  isAdministrator: boolean;
  currentUser: {
    name: string;
    domain: string;
    fullName: string;
  };
  canManageIIS: boolean;
  instructions: string[];
  message: string;
}

// Tipos para o gerenciamento de deploy

export interface DeployState {
  isDeploying: boolean;
  lastDeployResult: DeployResult | null;
  error: string | null;
}

export interface DeployResult {
  success: boolean;
  message: string;
  repository: string;
  branch: string;
  buildCommand: string;
  buildOutput: string;
  targetPath?: string;
  iisSiteName: string;
  timestamp: string;
  deployDetails?: Record<string, unknown>;
}


// Action types
export const DEPLOY_ACTION_TYPES = {
  // Deploy execution
  DEPLOY_START: 'deploy/deployStart',
  DEPLOY_SUCCESS: 'deploy/deploySuccess',
  DEPLOY_FAILURE: 'deploy/deployFailure',
  
  // Clear state
  CLEAR_DEPLOY_STATE: 'deploy/clearDeployState',
  CLEAR_ERROR: 'deploy/clearError',
} as const;

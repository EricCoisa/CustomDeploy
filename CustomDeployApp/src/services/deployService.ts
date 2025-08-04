import { api, type ApiResponse } from '../utils/api';

// Tipos para o DeployController
export interface DeployRequest {
  repoUrl: string;
  branch: string;
  buildCommand: string;
  buildOutput: string;
  iisSiteName: string;
  targetPath?: string;
  applicationPath?: string;
}

export interface DeployResponse {
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

class DeployService {
  // Executar deploy
  async executeDeploy(request: DeployRequest): Promise<ApiResponse<DeployResponse>> {
    return await api.post<DeployResponse>('/deploy', request, {timeout: 60000});
  }
}

// Exportar instância única do serviço
export const deployService = new DeployService();
export default deployService;

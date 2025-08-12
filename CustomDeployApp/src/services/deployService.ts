import { api, type ApiResponse } from '../utils/api';

// Tipos para o SystemController
export interface DeployRequest {
  repoUrl: string; // URL do repositório
  branch: string; // Branch do repositório
  buildCommands: string[]; // Comandos de build
  buildOutput: string; // Saída do build
  iisSiteName: string; // Nome do site IIS
  targetPath?: string; // Caminho de destino (opcional)
  applicationPath?: string; // Caminho da aplicação (opcional)
  plataforma?: string; // Plataforma (opcional)
}

export interface DeployResponse {
  message: string;
  repository: string;
  branch: string;
  buildCommand: string[];
  buildOutput: string;
  targetPath?: string;
  iisSiteName: string;
  timestamp: string;
  deployDetails?: Record<string, unknown>;
}

class DeployService {
  // Executar deploy
  async executeDeploy(request: DeployRequest): Promise<ApiResponse<DeployResponse>> {
    return await api.post<DeployResponse>('/deploys/executar', request, {timeout: 60000});
  }
}

// Exportar instância única do serviço
export const deployService = new DeployService();
export default deployService;

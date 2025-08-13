import { createAsyncThunk } from '@reduxjs/toolkit';
import { deployService, type BuildCommand, type DeployRequest } from '../../services/deployService';
import { type DeployResult } from './types';

// Executar deploy
export const executeDeploy = createAsyncThunk(
  'deploy/executeDeploy',
  async (request: DeployRequest, { rejectWithValue }) => {
    try {

      const response = await deployService.executeDeploy(request);

      if (!response.success) {
        return rejectWithValue(response.data || 'Erro ao executar deploy');
      }
      
      // Converter os dados da resposta para o formato esperado pelo DeployResult
      const result: DeployResult = {
        success: true,
        message: response.data.message,
        repository: response.data.repository,
        branch: response.data.branch,
        buildCommand: (response.data.buildCommand as BuildCommand[]) || [], // Garantir o tipo correto
        buildOutput: response.data.buildOutput,
        targetPath: response.data.targetPath,
        iisSiteName: response.data.iisSiteName,
        timestamp: response.data.timestamp,
        deployDetails: response.data.deployDetails,
      };
      return result;
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao executar deploy';
      return rejectWithValue(errorMessage);
    }
  }
);

// Limpar estado de deploy
export const clearDeployState = createAsyncThunk(
  'deploy/clearDeployState',
  async () => {
    return {};
  }
);

// Limpar erro
export const clearError = createAsyncThunk(
  'deploy/clearError',
  async () => {
    return {};
  }
);

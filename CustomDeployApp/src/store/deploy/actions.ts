import { createAsyncThunk } from '@reduxjs/toolkit';
import { deployService, type DeployRequest } from '../../services/deployService';

// Executar deploy
export const executeDeploy = createAsyncThunk(
  'deploy/executeDeploy',
  async (request: DeployRequest, { rejectWithValue }) => {
    try {

      const response = await deployService.executeDeploy(request);

      if (!response.success) {
        return rejectWithValue(response.data || 'Erro ao executar deploy');
      }
      
      return {
        success: true,
        message: response.data.message,
        repository: response.data.repository,
        branch: response.data.branch,
        buildCommand: response.data.buildCommand,
        buildOutput: response.data.buildOutput,
        targetPath: response.data.targetPath,
        iisSiteName: response.data.iisSiteName,
        timestamp: response.data.timestamp,
        deployDetails: response.data.deployDetails,
      };
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

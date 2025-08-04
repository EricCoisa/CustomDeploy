import { createAsyncThunk } from '@reduxjs/toolkit';
import { deployService } from '../../services/deployService';
import type { DeployFormData } from './types';

// Executar deploy
export const executeDeploy = createAsyncThunk(
  'deploy/executeDeploy',
  async (formData: DeployFormData, { rejectWithValue }) => {
    try {
      // Construir request para o backend
      const request = {
        repoUrl: formData.repoUrl,
        branch: formData.branch,
        buildCommand: formData.buildCommand,
        buildOutput: formData.buildOutput,
        iisSiteName: formData.siteName,
        applicationPath: formData.applicationPath || undefined,
      };

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

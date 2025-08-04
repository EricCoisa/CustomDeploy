import { createAsyncThunk } from '@reduxjs/toolkit';
import { publicationService } from '../../services/publicationService';
import type {
  CreateMetadataRequest,
  UpdateMetadataRequest
} from './types';

// Buscar todas as publicações
export const fetchPublications = createAsyncThunk(
  'publications/fetchPublications',
  async (_, { rejectWithValue }) => {
    try {
      const response = await publicationService.getPublications();
      
      if (!response.success) {
        return rejectWithValue(response.data || 'Erro ao buscar publicações');
      }
      
      return response.data;
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao buscar publicações';
      return rejectWithValue(errorMessage);
    }
  }
);

// Buscar uma publicação específica
export const fetchPublicationByName = createAsyncThunk(
  'publications/fetchPublicationByName',
  async (name: string, { rejectWithValue }) => {
    try {
      const response = await publicationService.getPublicationByName(name);
      
      if (!response.success) {
        return rejectWithValue(response.data || 'Erro ao buscar publicação');
      }
      
      return response.data;
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao buscar publicação';
      return rejectWithValue(errorMessage);
    }
  }
);

// Buscar estatísticas das publicações
export const fetchPublicationsStats = createAsyncThunk(
  'publications/fetchPublicationsStats',
  async (_, { rejectWithValue }) => {
    try {
      const response = await publicationService.getPublicationsStats();
      
      if (!response.success) {
        return rejectWithValue(response.data || 'Erro ao buscar estatísticas');
      }
      
      return response.data;
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao buscar estatísticas';
      return rejectWithValue(errorMessage);
    }
  }
);

// Buscar metadados de uma publicação
export const fetchPublicationMetadata = createAsyncThunk(
  'publications/fetchPublicationMetadata',
  async (name: string, { rejectWithValue }) => {
    try {
      const response = await publicationService.getPublicationMetadata(name);
      
      if (!response.success) {
        return rejectWithValue(response.data || 'Erro ao buscar metadados');
      }
      
      return response.data;
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao buscar metadados';
      return rejectWithValue(errorMessage);
    }
  }
);

// Criar metadados
export const createMetadata = createAsyncThunk(
  'publications/createMetadata',
  async (request: CreateMetadataRequest, { rejectWithValue }) => {
    try {
      const response = await publicationService.createMetadata(request);
      
      if (!response.success) {
        return rejectWithValue(response.data || 'Erro ao criar metadados');
      }
      
      return response.data;
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao criar metadados';
      return rejectWithValue(errorMessage);
    }
  }
);

// Atualizar metadados
export const updateMetadata = createAsyncThunk(
  'publications/updateMetadata',
  async ({ name, request }: { name: string; request: UpdateMetadataRequest }, { rejectWithValue }) => {
    try {
      const response = await publicationService.updateMetadata(name, request);
      
      if (!response.success) {
        return rejectWithValue(response.data || 'Erro ao atualizar metadados');
      }
      
      return response.data;
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao atualizar metadados';
      return rejectWithValue(errorMessage);
    }
  }
);

// Deletar publicação
export const deletePublication = createAsyncThunk(
  'publications/deletePublication',
  async (name: string, { rejectWithValue }) => {
    try {
      const response = await publicationService.deletePublication(name);
      
      if (!response.success) {
        return rejectWithValue('Erro ao deletar publicação');
      }
      
      return { name };
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao deletar publicação';
      return rejectWithValue(errorMessage);
    }
  }
);

// Deletar apenas metadados
export const deletePublicationMetadataOnly = createAsyncThunk(
  'publications/deletePublicationMetadataOnly',
  async (name: string, { rejectWithValue }) => {
    try {
      const response = await publicationService.deletePublicationMetadataOnly(name);
      
      if (!response.success) {
        return rejectWithValue('Erro ao deletar metadados');
      }
      
      return { name };
    } catch (error: unknown) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao deletar metadados';
      return rejectWithValue(errorMessage);
    }
  }
);

// Limpar erros
export const clearErrors = createAsyncThunk(
  'publications/clearErrors',
  async () => {
    return {};
  }
);

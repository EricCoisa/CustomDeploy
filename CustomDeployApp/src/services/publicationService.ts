import { api, type ApiResponse } from '../utils/api';
import type {
  Publication,
  PublicationStats,
  CreateMetadataRequest,
  UpdateMetadataRequest
} from '../store/publications/types';

class PublicationService {
  // Buscar todas as publicações
  async getPublications(): Promise<ApiResponse<{ publications: Publication[]; count: number }>> {
    // Aumentar timeout para operações que podem demorar (listagem de sites IIS)
    const response = await api.get<{ publications?: Publication[] }>('/publication/publications', {
      timeout: 30000 // 30 segundos para operações do IIS
    });

    // Se data for um array, adapta para o formato esperado
    if (Array.isArray(response.data)) {
      return {
        ...response,
        data: {
          publications: response.data,
          count: response.data.length
        }
      };
    }

    // Caso já venha no formato esperado
    return {
      ...response,
      data: {
        publications: response.data?.publications || [],
        count: response.data?.publications?.length || 0
      }
    };
  }

  // Buscar uma publicação específica
  async getPublicationByName(name: string): Promise<ApiResponse<{ publication: Publication }>> {
    return await api.get<{ publication: Publication }>(`/publication/publications/${encodeURIComponent(name)}`);
  }

  // Buscar estatísticas das publicações
  async getPublicationsStats(): Promise<ApiResponse<{ stats: PublicationStats }>> {
    return await api.get<{ stats: PublicationStats }>('/publication/publications/stats');
  }

  // Buscar metadados de uma publicação
  async getPublicationMetadata(name: string): Promise<ApiResponse<{ metadata: Publication }>> {
    return await api.get<{ metadata: Publication }>(`/publication/publications/${encodeURIComponent(name)}/metadata`);
  }

  // Criar metadados para uma nova publicação
  async createMetadata(request: CreateMetadataRequest): Promise<ApiResponse<Publication>> {
    return await api.post<Publication>('/publication/publications/metadata', request);
  }

  // Atualizar metadados de uma publicação
  async updateMetadata(name: string, request: UpdateMetadataRequest): Promise<ApiResponse<{ updatedMetadata: Publication }>> {
    return await api.patch<{ updatedMetadata: Publication }>(`/publication/publications/${encodeURIComponent(name)}/metadata`, request);
  }

  // Deletar publicação completamente (metadados + pasta física)
  async deletePublication(name: string): Promise<ApiResponse<void>> {
    return await api.delete<void>(`/publication/publications/${encodeURIComponent(name)}`);
  }

  // Deletar apenas metadados (manter pasta física)
  async deletePublicationMetadataOnly(name: string): Promise<ApiResponse<void>> {
    return await api.delete<void>(`/publication/publications/${encodeURIComponent(name)}/metadata-only`);
  }
}

// Exportar instância única do serviço
export const publicationService = new PublicationService();
export default publicationService;

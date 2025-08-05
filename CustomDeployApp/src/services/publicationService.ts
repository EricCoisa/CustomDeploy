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
    return await api.get<{ publications: Publication[]; count: number }>('/deploy/publications', {
      timeout: 30000 // 30 segundos para operações do IIS
    });
  }

  // Buscar uma publicação específica
  async getPublicationByName(name: string): Promise<ApiResponse<{ publication: Publication }>> {
    return await api.get<{ publication: Publication }>(`/deploy/publications/${encodeURIComponent(name)}`);
  }

  // Buscar estatísticas das publicações
  async getPublicationsStats(): Promise<ApiResponse<{ stats: PublicationStats }>> {
    return await api.get<{ stats: PublicationStats }>('/deploy/publications/stats');
  }

  // Buscar metadados de uma publicação
  async getPublicationMetadata(name: string): Promise<ApiResponse<{ metadata: Publication }>> {
    return await api.get<{ metadata: Publication }>(`/deploy/publications/${encodeURIComponent(name)}/metadata`);
  }

  // Criar metadados para uma nova publicação
  async createMetadata(request: CreateMetadataRequest): Promise<ApiResponse<Publication>> {
    return await api.post<Publication>('/deploy/publications/metadata', request);
  }

  // Atualizar metadados de uma publicação
  async updateMetadata(name: string, request: UpdateMetadataRequest): Promise<ApiResponse<{ updatedMetadata: Publication }>> {
    return await api.patch<{ updatedMetadata: Publication }>(`/deploy/publications/${encodeURIComponent(name)}/metadata`, request);
  }

  // Deletar publicação completamente (metadados + pasta física)
  async deletePublication(name: string): Promise<ApiResponse<void>> {
    return await api.delete<void>(`/deploy/publications/${encodeURIComponent(name)}`);
  }

  // Deletar apenas metadados (manter pasta física)
  async deletePublicationMetadataOnly(name: string): Promise<ApiResponse<void>> {
    return await api.delete<void>(`/deploy/publications/${encodeURIComponent(name)}/metadata-only`);
  }
}

// Exportar instância única do serviço
export const publicationService = new PublicationService();
export default publicationService;

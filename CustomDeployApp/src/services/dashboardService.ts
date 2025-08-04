import { api } from '../utils/api';
import { publicationService } from './publicationService';
import { iisService } from './iisService';
import type { Publication } from '../store/publications/types';

export interface DashboardStats {
  totalDeployments: number;
  totalSites: number;
  totalApplications: number;
  totalAppPools: number;
}

export interface DashboardData {
  stats: DashboardStats;
  recentDeployments: Publication[];
  isLoading: boolean;
  error?: string;
}

// Interfaces para respostas da API
interface AdminStatusResponse {
  isAdministrator?: boolean;
  currentUser?: {
    name: string;
    domain: string;
    fullName: string;
  };
  canManageIIS?: boolean;
  instructions?: string[];
  message?: string;
  timestamp?: string;
}

interface GitHubConnectivityResponse {
  success?: boolean;
  connected?: boolean;
  isConnected?: boolean;
  message?: string;
}

class DashboardService {
  // Buscar estatísticas do dashboard
  async getDashboardStats(): Promise<DashboardStats> {
    try {
      // Buscar publicações
      const publicationsResponse = await publicationService.getPublications();
      const totalDeployments = publicationsResponse.success 
        ? publicationsResponse.data.count 
        : 0;

      // Buscar sites IIS
      const sitesResponse = await iisService.getSites();
      const sites = sitesResponse.success ? sitesResponse.data.sites : [];
      const totalSites = sites.length;
      
      // Calcular total de aplicações (soma das aplicações de todos os sites)
      const totalApplications = sites.reduce((total, site) => {
        return total + (site.applications?.length || 0);
      }, 0);

      // Buscar pools de aplicação
      const appPoolsResponse = await iisService.getAppPools();
      const totalAppPools = appPoolsResponse.success 
        ? appPoolsResponse.data.appPools.length 
        : 0;

      return {
        totalDeployments,
        totalSites,
        totalApplications,
        totalAppPools,
        // totalPlatforms: 0, // TODO: Implementar quando endpoint estiver disponível
      };
    } catch (error) {
      console.error('Erro ao buscar estatísticas do dashboard:', error);
      throw error;
    }
  }

  // Buscar deployments recentes (limitado aos últimos 5)
  async getRecentDeployments(): Promise<Publication[]> {
    try {
      const response = await publicationService.getPublications();
      
      if (!response.success) {
        return [];
      }

      // Filtrar e ordenar por data de deploy (mais recentes primeiro)
      const deploymentsWithDate = response.data.publications
        .filter(pub => pub.deployedAt)
        .sort((a, b) => {
          const dateA = new Date(a.deployedAt!);
          const dateB = new Date(b.deployedAt!);
          return dateB.getTime() - dateA.getTime();
        });

      // Retornar apenas os 5 mais recentes
      return deploymentsWithDate.slice(0, 5);
    } catch (error) {
      console.error('Erro ao buscar deployments recentes:', error);
      throw error;
    }
  }

  // Buscar dados completos do dashboard
  async getDashboardData(): Promise<DashboardData> {
    try {
      const [stats, recentDeployments] = await Promise.all([
        this.getDashboardStats(),
        this.getRecentDeployments()
      ]);

      return {
        stats,
        recentDeployments,
        isLoading: false,
      };
    } catch (error) {
      console.error('Erro ao buscar dados do dashboard:', error);
      return {
        stats: {
          totalDeployments: 0,
          totalSites: 0,
          totalApplications: 0,
          totalAppPools: 0,
        },
        recentDeployments: [],
        isLoading: false,
        error: error instanceof Error ? error.message : 'Erro desconhecido',
      };
    }
  }

  // Método auxiliar para verificar status do sistema
  async getSystemStatus(): Promise<{
    iisStatus: 'online' | 'offline' | 'unknown';
    apiStatus: 'online' | 'offline';
    adminStatus?: 'admin' | 'not-admin' | 'unknown';
    githubStatus?: 'connected' | 'disconnected' | 'unknown';
  }> {
    try {
      let apiStatus: 'online' | 'offline' = 'offline';
      let iisStatus: 'online' | 'offline' | 'unknown' = 'unknown';
      let adminStatus: 'admin' | 'not-admin' | 'unknown' = 'unknown';
      let githubStatus: 'connected' | 'disconnected' | 'unknown' = 'unknown';

      // Testar se a API está respondendo através do endpoint de admin-status
      try {
        const adminResponse = await api.get('/iis/admin-status');
        if (adminResponse) {
          apiStatus = 'online';
          // Verificar se tem privilégios de admin
          const responseData = adminResponse.data as AdminStatusResponse;
          console.log('🔍 Verificando status do usuário:', responseData);
          if (responseData?.isAdministrator === true || responseData?.canManageIIS === true) {
            adminStatus = 'admin';
          } else {
            adminStatus = 'not-admin';
          }
        }
      } catch {
        apiStatus = 'offline';
      }

      // Se API está online, testar IIS através do endpoint de sites
      if (apiStatus === 'online') {
        try {
          const iisTest = await iisService.getSites();
          iisStatus = iisTest.success ? 'online' : 'offline';
        } catch {
          iisStatus = 'offline';
        }

        // Testar conectividade do GitHub
        try {
          const githubResponse = await api.get('/github/test-connectivity');
          const githubData = githubResponse?.data as GitHubConnectivityResponse;
          if (githubData?.success === true || githubData?.connected === true || githubData?.isConnected === true) {
            githubStatus = 'connected';
          } else {
            githubStatus = 'disconnected';
          }
        } catch {
          githubStatus = 'disconnected';
        }
      }

      return {
        iisStatus,
        apiStatus,
        adminStatus,
        githubStatus
      };
    } catch {
      return {
        iisStatus: 'unknown',
        apiStatus: 'offline',
        adminStatus: 'unknown',
        githubStatus: 'unknown'
      };
    }
  }
}

export const dashboardService = new DashboardService();

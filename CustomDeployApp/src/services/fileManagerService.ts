import { api, type ApiResponse } from '../utils/api';

// Interfaces para os dados do FileManager
export interface FileSystemItem {
  name: string;
  fullPath: string;
  isDirectory: boolean;
  size?: number;
  lastModified?: string;
  extension?: string;
  isAccessible: boolean;
  isHidden?: boolean;
}

export interface DirectoryContents {
  currentPath: string;
  parentPath?: string;
  isAccessible: boolean;
  errorMessage?: string;
  items: FileSystemItem[];
  directories: FileSystemItem[];
  files: FileSystemItem[];
  totalItems: number;
  timestamp: string;
}

export interface DriveInfo {
  name: string;
  fullPath: string;
  isAccessible: boolean;
  size?: number;
  freeSpace?: number;
  driveType?: string;
  // Campos adicionais baseados no controller que retorna informações processadas
  sizeGB?: number;
}

export interface SystemInfo {
  operatingSystem: string;
  machineName: string;
  userName: string;
  currentDirectory: string;
  systemDirectory: string;
  availableDrives: number;
  drives: DriveInfo[];
  timestamp: string;
}

// Serviço para gerenciamento de arquivos
class FileManagerService {
  private readonly baseUrl = '/FileManager';

  /**
   * Obtém o conteúdo de um diretório
   */
  async getDirectoryContents(
    path?: string,
    includeHidden: boolean = false,
    fileExtensionFilter?: string,
    sortBy: string = 'name',
    ascending: boolean = true
  ): Promise<ApiResponse<DirectoryContents>> {
    try {
      const params = new URLSearchParams();
      
      if (path) params.append('path', path);
      params.append('includeHidden', includeHidden.toString());
      if (fileExtensionFilter) params.append('fileExtensionFilter', fileExtensionFilter);
      params.append('sortBy', sortBy);
      params.append('ascending', ascending.toString());

      const response = await api.get<DirectoryContents>(`${this.baseUrl}?${params.toString()}`);
      
      return response;
    } catch (error) {
      console.error('Erro ao obter conteúdo do diretório:', error);
      return {
        success: false,
        data: {} as DirectoryContents,
        message: this.getErrorMessage(error),
      };
    }
  }

  /**
   * Obtém informações detalhadas de um item específico
   */
  async getItemInfo(path: string): Promise<ApiResponse<FileSystemItem>> {
    try {
      const params = new URLSearchParams();
      params.append('path', path);

      const response = await api.get<{ message: string; item: FileSystemItem; timestamp: string }>(`${this.baseUrl}/item?${params.toString()}`);
      
      if (response.success && response.data && response.data.item) {
        return {
          success: true,
          data: response.data.item,
          message: response.data.message || 'Informações obtidas com sucesso',
        };
      }

      return {
        success: false,
        data: {} as FileSystemItem,
        message: response.message || 'Erro ao obter informações do item',
      };
    } catch (error) {
      console.error('Erro ao obter informações do item:', error);
      return {
        success: false,
        data: {} as FileSystemItem,
        message: this.getErrorMessage(error),
      };
    }
  }

  /**
   * Obtém as unidades de disco disponíveis
   */
  async getAvailableDrives(): Promise<ApiResponse<DriveInfo[]>> {
    try {
      const response = await api.get<{ message: string; drives: DriveInfo[]; timestamp: string }>(`${this.baseUrl}/drives`);
      
      if (response.success && response.data && response.data.drives) {
        return {
          success: true,
          data: response.data.drives,
          message: response.data.message || 'Drives obtidos com sucesso',
        };
      }

      return {
        success: false,
        data: [],
        message: response.message || 'Erro ao obter drives',
      };
    } catch (error) {
      console.error('Erro ao obter drives disponíveis:', error);
      return {
        success: false,
        data: [],
        message: this.getErrorMessage(error),
      };
    }
  }

  /**
   * Valida se um caminho é válido e acessível
   */
  async validatePath(path: string): Promise<ApiResponse<{
    path: string;
    isValid: boolean;
    isBlocked: boolean;
    isAccessible: boolean;
    message: string;
    timestamp: string;
  }>> {
    try {
      const params = new URLSearchParams();
      params.append('path', path);

      const response = await api.get<{
        path: string;
        isValid: boolean;
        isBlocked: boolean;
        isAccessible: boolean;
        message: string;
        timestamp: string;
      }>(`${this.baseUrl}/validate?${params.toString()}`);
      
      return response;
    } catch (error) {
      console.error('Erro ao validar caminho:', error);
      return {
        success: false,
        data: {
          path,
          isValid: false,
          isBlocked: false,
          isAccessible: false,
          message: this.getErrorMessage(error),
          timestamp: new Date().toISOString(),
        },
        message: 'Erro na validação',
      };
    }
  }

  /**
   * Obtém informações do sistema
   */
  async getSystemInfo(): Promise<ApiResponse<SystemInfo>> {
    try {
      const response = await api.get<{ message: string; systemInfo: SystemInfo; timestamp: string }>(`${this.baseUrl}/system-info`);
      
      if (response.success && response.data && response.data.systemInfo) {
        return {
          success: true,
          data: response.data.systemInfo,
          message: response.data.message || 'Informações do sistema obtidas com sucesso',
        };
      }

      return {
        success: false,
        data: {} as SystemInfo,
        message: response.message || 'Erro ao obter informações do sistema',
      };
    } catch (error) {
      console.error('Erro ao obter informações do sistema:', error);
      return {
        success: false,
        data: {} as SystemInfo,
        message: this.getErrorMessage(error),
      };
    }
  }

  /**
   * Utilitário para formatar tamanho de arquivo
   */
  formatFileSize(bytes?: number): string {
    if (!bytes || bytes === 0) return '0 B';

    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  /**
   * Utilitário para verificar se o erro é de privilégios insuficientes
   */
  isPrivilegeError(error: unknown): boolean {
    if (error && typeof error === 'object' && 'response' in error) {
      const httpError = error as { response?: { status?: number } };
      if (httpError.response?.status === 403) return true;
    }
    
    if (error && typeof error === 'object' && 'message' in error) {
      const errorWithMessage = error as { message?: string };
      if (typeof errorWithMessage.message === 'string') {
        return errorWithMessage.message.toLowerCase().includes('privilégios') ||
               errorWithMessage.message.toLowerCase().includes('administrador') ||
               errorWithMessage.message.toLowerCase().includes('acesso negado');
      }
    }
    return false;
  }

  /**
   * Utilitário para obter mensagem de erro específica baseada no tipo de erro
   */
  getErrorMessage(error: unknown): string {
    if (this.isPrivilegeError(error)) {
      return 'Acesso negado: é necessário executar a aplicação como administrador para acessar o sistema de arquivos.';
    }

    if (error && typeof error === 'object' && 'response' in error) {
      const httpError = error as { 
        response?: { 
          status?: number; 
          data?: { message?: string } 
        } 
      };
      
      if (httpError.response?.status === 400) {
        return httpError.response.data?.message || 'Erro na solicitação - verifique os parâmetros fornecidos.';
      }

      if (httpError.response?.status === 404) {
        return 'Caminho não encontrado ou inacessível.';
      }

      if (httpError.response?.status === 500) {
        return httpError.response.data?.message || 'Erro interno do servidor.';
      }
    }

    return error instanceof Error ? error.message : 'Erro desconhecido ao acessar o sistema de arquivos.';
  }

  /**
   * Utilitário para obter ícone baseado na extensão do arquivo
   */
  getFileIcon(item: FileSystemItem): string {
    if (item.isDirectory) return '📁';

    const ext = item.extension?.toLowerCase();
    switch (ext) {
      case '.txt':
      case '.md':
      case '.log': return '📄';
      case '.js':
      case '.ts':
      case '.jsx':
      case '.tsx': return '📜';
      case '.json': return '📋';
      case '.html':
      case '.htm': return '🌐';
      case '.css': return '🎨';
      case '.png':
      case '.jpg':
      case '.jpeg':
      case '.gif':
      case '.bmp': return '🖼️';
      case '.mp4':
      case '.avi':
      case '.mov': return '🎬';
      case '.mp3':
      case '.wav':
      case '.flac': return '🎵';
      case '.zip':
      case '.rar':
      case '.7z': return '📦';
      case '.exe':
      case '.msi': return '⚙️';
      case '.pdf': return '📕';
      case '.doc':
      case '.docx': return '📘';
      case '.xls':
      case '.xlsx': return '📗';
      default: return '📄';
    }
  }
}

// Exportar instância única
export const fileManagerService = new FileManagerService();

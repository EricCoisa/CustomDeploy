import { api, type ApiResponse } from '../utils/api';
import type {
  IISSite,
  IISAppPool,
  IISApplication,
  CreateSiteRequest,
  UpdateSiteRequest,
  CreateApplicationRequest,
  UpdateApplicationRequest,
  CreateAppPoolRequest,
  UpdateAppPoolRequest,
  PermissionCheckResult,
  AdminStatusResult
} from '../store/iis/types';

class IISService {
  // Permission Management
  async getPermissions(): Promise<ApiResponse<{ permissions: PermissionCheckResult }>> {
    return await api.get<{ permissions: PermissionCheckResult }>('/iis/request-permissions');
  }

  async getAdminStatus(): Promise<ApiResponse<AdminStatusResult>> {
    return await api.get<AdminStatusResult>('/iis/admin-status');
  }

  async requestAdminPrivileges(): Promise<ApiResponse<{ success: boolean; message: string }>> {
    return await api.post<{ success: boolean; message: string }>('/iis/request-admin');
  }

  // Sites CRUD
  async getSites(): Promise<ApiResponse<{ sites: IISSite[] }>> {
    return await api.get<{ sites: IISSite[] }>('/iis/sites', {
      timeout: 30000 // 30 segundos para operações do IIS
    });
  }

  async getSiteInfo(siteName: string): Promise<ApiResponse<{ site: IISSite }>> {
    return await api.get<{ site: IISSite }>(`/iis/sites/${encodeURIComponent(siteName)}`);
  }

  async createSite(siteData: CreateSiteRequest): Promise<ApiResponse<{ site: IISSite }>> {
    return await api.post<{ site: IISSite }>('/iis/sites', siteData);
  }

  async updateSite(siteName: string, siteData: UpdateSiteRequest): Promise<ApiResponse<{ site: IISSite }>> {
    return await api.put<{ site: IISSite }>(`/iis/sites/${encodeURIComponent(siteName)}`, siteData);
  }

  async deleteSite(siteName: string): Promise<ApiResponse<void>> {
    return await api.delete<void>(`/iis/sites/${encodeURIComponent(siteName)}`);
  }

  // Applications CRUD
  async getApplications(siteName: string): Promise<ApiResponse<{ applications: IISApplication[] }>> {
    return await api.get<{ applications: IISApplication[] }>(`/iis/sites/${encodeURIComponent(siteName)}/applications`);
  }

  async createApplication(appData: CreateApplicationRequest): Promise<ApiResponse<{ application: IISApplication }>> {
    return await api.post<{ application: IISApplication }>('/iis/applications', appData);
  }

  async updateApplication(
    siteName: string, 
    appPath: string, 
    appData: UpdateApplicationRequest
  ): Promise<ApiResponse<{ application: IISApplication }>> {
    const encodedAppPath = encodeURIComponent(appPath.replace(/^\//, ''));
    return await api.put<{ application: IISApplication }>(
      `/iis/sites/${encodeURIComponent(siteName)}/applications/${encodedAppPath}`, 
      appData
    );
  }

  async deleteApplication(siteName: string, appPath: string): Promise<ApiResponse<void>> {
    const encodedAppPath = encodeURIComponent(appPath.replace(/^\//, ''));
    return await api.delete<void>(
      `/iis/sites/${encodeURIComponent(siteName)}/applications/${encodedAppPath}`
    );
  }

  async checkApplication(siteName: string, appPath: string): Promise<ApiResponse<{
    applicationExists: boolean;
    applicationInfo?: IISApplication;
  }>> {
    const encodedAppPath = encodeURIComponent(appPath.replace(/^\//, ''));
    return await api.get<{
      applicationExists: boolean;
      applicationInfo?: IISApplication;
    }>(`/iis/sites/${encodeURIComponent(siteName)}/applications/check/${encodedAppPath}`);
  }

  // Application Pools CRUD
  async getAppPools(): Promise<ApiResponse<{ appPools: IISAppPool[] }>> {
    return await api.get<{ appPools: IISAppPool[] }>('/iis/app-pools', {
      timeout: 30000 // 30 segundos para operações do IIS
    });
  }

  async createAppPool(poolData: CreateAppPoolRequest): Promise<ApiResponse<{ appPool: IISAppPool }>> {
    return await api.post<{ appPool: IISAppPool }>('/iis/app-pools', poolData);
  }

  async updateAppPool(poolName: string, poolData: UpdateAppPoolRequest): Promise<ApiResponse<{ appPool: IISAppPool }>> {
    return await api.put<{ appPool: IISAppPool }>(`/iis/app-pools/${encodeURIComponent(poolName)}`, poolData);
  }

  async deleteAppPool(poolName: string): Promise<ApiResponse<void>> {
    return await api.delete<void>(`/iis/app-pools/${encodeURIComponent(poolName)}`);
  }

  // Site Management (Start/Stop)
  async startSite(siteName: string): Promise<ApiResponse<{ message: string; siteName: string; status: string }>> {
    return await api.post<{ message: string; siteName: string; status: string }>(
      `/iis/sites/${encodeURIComponent(siteName)}/start`
    );
  }

  async stopSite(siteName: string): Promise<ApiResponse<{ message: string; siteName: string; status: string }>> {
    return await api.post<{ message: string; siteName: string; status: string }>(
      `/iis/sites/${encodeURIComponent(siteName)}/stop`
    );
  }

  // Application Management (Start/Stop)
  async startApplication(siteName: string, appPath: string): Promise<ApiResponse<{ 
    message: string; 
    siteName: string; 
    appPath: string; 
    status: string 
  }>> {
    const encodedAppPath = encodeURIComponent(appPath.replace(/^\//, ''));
    return await api.post<{ 
      message: string; 
      siteName: string; 
      appPath: string; 
      status: string 
    }>(`/iis/sites/${encodeURIComponent(siteName)}/applications/start/${encodedAppPath}`);
  }

  async stopApplication(siteName: string, appPath: string): Promise<ApiResponse<{ 
    message: string; 
    siteName: string; 
    appPath: string; 
    status: string 
  }>> {
    const encodedAppPath = encodeURIComponent(appPath.replace(/^\//, ''));
    return await api.post<{ 
      message: string; 
      siteName: string; 
      appPath: string; 
      status: string 
    }>(`/iis/sites/${encodeURIComponent(siteName)}/applications/stop/${encodedAppPath}`);
  }

  // Application Pool Management (Start/Stop)
  async startAppPool(poolName: string): Promise<ApiResponse<{ message: string; poolName: string; status: string }>> {
    return await api.post<{ message: string; poolName: string; status: string }>(
      `/iis/app-pools/${encodeURIComponent(poolName)}/start`
    );
  }

  async stopAppPool(poolName: string): Promise<ApiResponse<{ message: string; poolName: string; status: string }>> {
    return await api.post<{ message: string; poolName: string; status: string }>(
      `/iis/app-pools/${encodeURIComponent(poolName)}/stop`
    );
  }
}

export const iisService = new IISService();

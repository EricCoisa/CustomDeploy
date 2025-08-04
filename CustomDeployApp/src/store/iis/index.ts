import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { iisService } from '../../services/iisService';
import type { 
  IISState, 
  CreateSiteRequest,
  UpdateSiteRequest,
  CreateApplicationRequest,
  UpdateApplicationRequest,
  CreateAppPoolRequest,
  UpdateAppPoolRequest
} from './types';

// Estado inicial
const initialState: IISState = {
  sites: [],
  appPools: [],
  loading: {
    sites: false,
    appPools: false,
    applications: false,
  },
  error: {
    sites: null,
    appPools: null,
    applications: null,
  },
  permissions: null,
  isAdministrator: null,
};

// Thunks assíncronos
export const fetchPermissions = createAsyncThunk(
  'iis/fetchPermissions',
  async () => {
    const response = await iisService.getPermissions();
    return response.data;
  }
);

export const fetchAdminStatus = createAsyncThunk(
  'iis/fetchAdminStatus',
  async () => {
    const response = await iisService.getAdminStatus();
    return response.data;
  }
);

export const requestAdminPrivileges = createAsyncThunk(
  'iis/requestAdminPrivileges',
  async () => {
    const response = await iisService.requestAdminPrivileges();
    return response.data;
  }
);

// Sites
export const fetchSites = createAsyncThunk(
  'iis/fetchSites',
  async () => {
    const response = await iisService.getSites();
    return response.data.sites;
  }
);

export const createSite = createAsyncThunk(
  'iis/createSite',
  async (siteData: CreateSiteRequest) => {
    const response = await iisService.createSite(siteData);
    return response.data.site;
  }
);

export const updateSite = createAsyncThunk(
  'iis/updateSite',
  async ({ siteName, siteData }: { siteName: string; siteData: UpdateSiteRequest }) => {
    const response = await iisService.updateSite(siteName, siteData);
    return response.data.site;
  }
);

export const deleteSite = createAsyncThunk(
  'iis/deleteSite',
  async (siteName: string) => {
    await iisService.deleteSite(siteName);
    return siteName;
  }
);

// Applications
export const fetchApplications = createAsyncThunk(
  'iis/fetchApplications',
  async (siteName: string) => {
    const response = await iisService.getApplications(siteName);
    return { siteName, applications: response.data.applications };
  }
);

export const createApplication = createAsyncThunk(
  'iis/createApplication',
  async (appData: CreateApplicationRequest) => {
    const response = await iisService.createApplication(appData);
    return { ...response.data.application, siteName: appData.siteName };
  }
);

export const updateApplication = createAsyncThunk(
  'iis/updateApplication',
  async ({ 
    siteName, 
    appPath, 
    appData 
  }: { 
    siteName: string; 
    appPath: string; 
    appData: UpdateApplicationRequest 
  }) => {
    const response = await iisService.updateApplication(siteName, appPath, appData);
    return response.data.application;
  }
);

export const deleteApplication = createAsyncThunk(
  'iis/deleteApplication',
  async ({ siteName, appPath }: { siteName: string; appPath: string }) => {
    await iisService.deleteApplication(siteName, appPath);
    return { siteName, appPath };
  }
);

// App Pools
export const fetchAppPools = createAsyncThunk(
  'iis/fetchAppPools',
  async () => {
    const response = await iisService.getAppPools();
    return response.data.appPools;
  }
);

export const createAppPool = createAsyncThunk(
  'iis/createAppPool',
  async (poolData: CreateAppPoolRequest) => {
    const response = await iisService.createAppPool(poolData);
    return response.data.appPool;
  }
);

export const updateAppPool = createAsyncThunk(
  'iis/updateAppPool',
  async ({ poolName, poolData }: { poolName: string; poolData: UpdateAppPoolRequest }) => {
    const response = await iisService.updateAppPool(poolName, poolData);
    return response.data.appPool;
  }
);

export const deleteAppPool = createAsyncThunk(
  'iis/deleteAppPool',
  async (poolName: string) => {
    await iisService.deleteAppPool(poolName);
    return poolName;
  }
);

// Site Management (Start/Stop)
export const startSite = createAsyncThunk(
  'iis/startSite',
  async (siteName: string) => {
    const response = await iisService.startSite(siteName);
    return { siteName, status: 'Started', message: response.data.message };
  }
);

export const stopSite = createAsyncThunk(
  'iis/stopSite',
  async (siteName: string) => {
    const response = await iisService.stopSite(siteName);
    return { siteName, status: 'Stopped', message: response.data.message };
  }
);

// Application Management (Start/Stop)
export const startApplication = createAsyncThunk(
  'iis/startApplication',
  async ({ siteName, appPath }: { siteName: string; appPath: string }) => {
    const response = await iisService.startApplication(siteName, appPath);
    return { siteName, appPath, status: 'Started', message: response.data.message };
  }
);

export const stopApplication = createAsyncThunk(
  'iis/stopApplication',
  async ({ siteName, appPath }: { siteName: string; appPath: string }) => {
    const response = await iisService.stopApplication(siteName, appPath);
    return { siteName, appPath, status: 'Stopped', message: response.data.message };
  }
);

// Application Pool Management (Start/Stop)
export const startAppPool = createAsyncThunk(
  'iis/startAppPool',
  async (poolName: string) => {
    const response = await iisService.startAppPool(poolName);
    return { poolName, status: 'Started', message: response.data.message };
  }
);

export const stopAppPool = createAsyncThunk(
  'iis/stopAppPool',
  async (poolName: string) => {
    const response = await iisService.stopAppPool(poolName);
    return { poolName, status: 'Stopped', message: response.data.message };
  }
);

// Slice
const iisSlice = createSlice({
  name: 'iis',
  initialState,
  reducers: {
    clearErrors: (state) => {
      state.error = {
        sites: null,
        appPools: null,
        applications: null,
      };
    },
  },
  extraReducers: (builder) => {
    // Permissions
    builder
      .addCase(fetchPermissions.fulfilled, (state, action) => {
        state.permissions = action.payload.permissions;
      })
      .addCase(fetchAdminStatus.fulfilled, (state, action) => {
        state.isAdministrator = action.payload.isAdministrator;
      });

    // Sites
    builder
      .addCase(fetchSites.pending, (state) => {
        state.loading.sites = true;
        state.error.sites = null;
      })
      .addCase(fetchSites.fulfilled, (state, action) => {
        state.loading.sites = false;
        state.sites = action.payload;
      })
      .addCase(fetchSites.rejected, (state, action) => {
        state.loading.sites = false;
        state.error.sites = action.error.message || 'Erro ao buscar sites';
      })
      .addCase(createSite.fulfilled, (state, action) => {
        state.sites.push(action.payload);
      })
      .addCase(updateSite.fulfilled, (state, action) => {
        const index = state.sites.findIndex(site => site.name === action.payload.name);
        if (index !== -1) {
          state.sites[index] = action.payload;
        }
      })
      .addCase(deleteSite.fulfilled, (state, action) => {
        state.sites = state.sites.filter(site => site.name !== action.payload);
      });

    // Applications
    builder
      .addCase(fetchApplications.pending, (state) => {
        state.loading.applications = true;
        state.error.applications = null;
      })
      .addCase(fetchApplications.fulfilled, (state, action) => {
        state.loading.applications = false;
        const { siteName, applications } = action.payload;
        const siteIndex = state.sites.findIndex(site => site.name === siteName);
        if (siteIndex !== -1) {
          state.sites[siteIndex].applications = applications;
        }
      })
      .addCase(fetchApplications.rejected, (state, action) => {
        state.loading.applications = false;
        state.error.applications = action.error.message || 'Erro ao buscar aplicações';
      })
      .addCase(createApplication.fulfilled, (state, action) => {
        const { siteName } = action.payload;
        const siteIndex = state.sites.findIndex(site => site.name === siteName);
        if (siteIndex !== -1) {
          if (!state.sites[siteIndex].applications) {
            state.sites[siteIndex].applications = [];
          }
          state.sites[siteIndex].applications!.push(action.payload);
        }
      })
      .addCase(deleteApplication.fulfilled, (state, action) => {
        const { siteName, appPath } = action.payload;
        const siteIndex = state.sites.findIndex(site => site.name === siteName);
        if (siteIndex !== -1 && state.sites[siteIndex].applications) {
          state.sites[siteIndex].applications = state.sites[siteIndex].applications!.filter(
            app => app.name !== appPath
          );
        }
      });

    // App Pools
    builder
      .addCase(fetchAppPools.pending, (state) => {
        state.loading.appPools = true;
        state.error.appPools = null;
      })
      .addCase(fetchAppPools.fulfilled, (state, action) => {
        state.loading.appPools = false;
        state.appPools = action.payload;
      })
      .addCase(fetchAppPools.rejected, (state, action) => {
        state.loading.appPools = false;
        state.error.appPools = action.error.message || 'Erro ao buscar app pools';
      })
      .addCase(createAppPool.fulfilled, (state, action) => {
        state.appPools.push(action.payload);
      })
      .addCase(updateAppPool.fulfilled, (state, action) => {
        const index = state.appPools.findIndex(pool => pool.name === action.payload.name);
        if (index !== -1) {
          state.appPools[index] = action.payload;
        }
      })
      .addCase(deleteAppPool.fulfilled, (state, action) => {
        state.appPools = state.appPools.filter(pool => pool.name !== action.payload);
      });

    // Site Management (Start/Stop)
    builder
      .addCase(startSite.fulfilled, (state, action) => {
        const siteIndex = state.sites.findIndex(site => site.name === action.payload.siteName);
        if (siteIndex !== -1) {
          state.sites[siteIndex].state = action.payload.status;
        }
      })
      .addCase(stopSite.fulfilled, (state, action) => {
        const siteIndex = state.sites.findIndex(site => site.name === action.payload.siteName);
        if (siteIndex !== -1) {
          state.sites[siteIndex].state = action.payload.status;
        }
      });

    // Application Management (Start/Stop)
    builder
      .addCase(startApplication.fulfilled, (state, action) => {
        const { siteName, appPath, status } = action.payload;
        const siteIndex = state.sites.findIndex(site => site.name === siteName);
        if (siteIndex !== -1 && state.sites[siteIndex].applications) {
          const appIndex = state.sites[siteIndex].applications!.findIndex(app => app.name === appPath);
          if (appIndex !== -1) {
            state.sites[siteIndex].applications![appIndex].state = status;
          }
        }
      })
      .addCase(stopApplication.fulfilled, (state, action) => {
        const { siteName, appPath, status } = action.payload;
        const siteIndex = state.sites.findIndex(site => site.name === siteName);
        if (siteIndex !== -1 && state.sites[siteIndex].applications) {
          const appIndex = state.sites[siteIndex].applications!.findIndex(app => app.name === appPath);
          if (appIndex !== -1) {
            state.sites[siteIndex].applications![appIndex].state = status;
          }
        }
      });

    // Application Pool Management (Start/Stop)
    builder
      .addCase(startAppPool.fulfilled, (state, action) => {
        const poolIndex = state.appPools.findIndex(pool => pool.name === action.payload.poolName);
        if (poolIndex !== -1) {
          state.appPools[poolIndex].state = action.payload.status;
        }
      })
      .addCase(stopAppPool.fulfilled, (state, action) => {
        const poolIndex = state.appPools.findIndex(pool => pool.name === action.payload.poolName);
        if (poolIndex !== -1) {
          state.appPools[poolIndex].state = action.payload.status;
        }
      });
  },
});

export const { clearErrors } = iisSlice.actions;
export default iisSlice.reducer;

import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { dashboardService } from '../../services/dashboardService';
import type { DashboardState } from './types';

const initialState: DashboardState = {
  stats: {
    totalDeployments: 0,
    totalSites: 0,
    totalApplications: 0,
    totalAppPools: 0,
  },
  recentDeployments: [],
  systemStatus: {
    iisStatus: 'unknown',
    apiStatus: 'offline',
  },
  isLoading: false,
  error: null,
  lastUpdated: null,
};

// Async Thunks
export const fetchDashboardData = createAsyncThunk(
  'dashboard/fetchDashboardData',
  async () => {
    const [dashboardData, systemStatus] = await Promise.all([
      dashboardService.getDashboardData(),
      dashboardService.getSystemStatus(),
    ]);

    if (dashboardData.error) {
      throw new Error(dashboardData.error);
    }

    return {
      stats: dashboardData.stats,
      recentDeployments: dashboardData.recentDeployments,
      systemStatus,
    };
  }
);

export const fetchDashboardStats = createAsyncThunk(
  'dashboard/fetchDashboardStats',
  async () => {
    return await dashboardService.getDashboardStats();
  }
);

export const fetchRecentDeployments = createAsyncThunk(
  'dashboard/fetchRecentDeployments',
  async () => {
    return await dashboardService.getRecentDeployments();
  }
);

export const fetchSystemStatus = createAsyncThunk(
  'dashboard/fetchSystemStatus',
  async () => {
    return await dashboardService.getSystemStatus();
  }
);

const dashboardSlice = createSlice({
  name: 'dashboard',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch Dashboard Data
    builder
      .addCase(fetchDashboardData.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchDashboardData.fulfilled, (state, action) => {
        state.isLoading = false;
        state.error = null;
        state.stats = action.payload.stats;
        state.recentDeployments = action.payload.recentDeployments;
        state.systemStatus = action.payload.systemStatus;
        state.lastUpdated = new Date().toISOString();
      })
      .addCase(fetchDashboardData.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Erro ao carregar dados do dashboard';
      })

      // Fetch Dashboard Stats
      .addCase(fetchDashboardStats.fulfilled, (state, action) => {
        state.stats = action.payload;
        state.lastUpdated = new Date().toISOString();
      })
      .addCase(fetchDashboardStats.rejected, (state, action) => {
        state.error = action.error.message || 'Erro ao carregar estatísticas';
      })

      // Fetch Recent Deployments
      .addCase(fetchRecentDeployments.fulfilled, (state, action) => {
        state.recentDeployments = action.payload;
        state.lastUpdated = new Date().toISOString();
      })
      .addCase(fetchRecentDeployments.rejected, (state, action) => {
        state.error = action.error.message || 'Erro ao carregar deployments recentes';
      })

      // Fetch System Status
      .addCase(fetchSystemStatus.fulfilled, (state, action) => {
        state.systemStatus = action.payload;
        state.lastUpdated = new Date().toISOString();
      })
      .addCase(fetchSystemStatus.rejected, (state, action) => {
        state.error = action.error.message || 'Erro ao verificar status do sistema';
      });
  },
});

export const { clearError } = dashboardSlice.actions;
export default dashboardSlice.reducer;

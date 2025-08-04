import type { Dispatch } from '@reduxjs/toolkit';
import { dashboardService } from '../../services/dashboardService';
import {
  DASHBOARD_ACTIONS,
  type DashboardAction,
  type DashboardStats,
  type SystemStatus,
} from './types';
import type { Publication } from '../publications/types';

// Action Creators
export const fetchDashboardStart = (): DashboardAction => ({
  type: DASHBOARD_ACTIONS.FETCH_DASHBOARD_START,
});

export const fetchDashboardSuccess = (
  stats: DashboardStats,
  recentDeployments: Publication[],
  systemStatus: SystemStatus
): DashboardAction => ({
  type: DASHBOARD_ACTIONS.FETCH_DASHBOARD_SUCCESS,
  payload: {
    stats,
    recentDeployments,
    systemStatus,
  },
});

export const fetchDashboardError = (error: string): DashboardAction => ({
  type: DASHBOARD_ACTIONS.FETCH_DASHBOARD_ERROR,
  payload: error,
});

export const fetchStatsSuccess = (stats: DashboardStats): DashboardAction => ({
  type: DASHBOARD_ACTIONS.FETCH_STATS_SUCCESS,
  payload: stats,
});

export const fetchRecentDeploymentsSuccess = (deployments: Publication[]): DashboardAction => ({
  type: DASHBOARD_ACTIONS.FETCH_RECENT_DEPLOYMENTS_SUCCESS,
  payload: deployments,
});

export const fetchSystemStatusSuccess = (status: SystemStatus): DashboardAction => ({
  type: DASHBOARD_ACTIONS.FETCH_SYSTEM_STATUS_SUCCESS,
  payload: status,
});

export const clearError = (): DashboardAction => ({
  type: DASHBOARD_ACTIONS.CLEAR_ERROR,
});

// Thunk Actions
export const fetchDashboardData = () => {
  return async (dispatch: Dispatch<DashboardAction>) => {
    try {
      dispatch(fetchDashboardStart());

      // Buscar dados do dashboard em paralelo
      const [dashboardData, systemStatus] = await Promise.all([
        dashboardService.getDashboardData(),
        dashboardService.getSystemStatus(),
      ]);

      if (dashboardData.error) {
        dispatch(fetchDashboardError(dashboardData.error));
        return;
      }

      dispatch(fetchDashboardSuccess(
        dashboardData.stats,
        dashboardData.recentDeployments,
        systemStatus
      ));
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao carregar dados do dashboard';
      dispatch(fetchDashboardError(errorMessage));
    }
  };
};

export const fetchDashboardStats = () => {
  return async (dispatch: Dispatch<DashboardAction>) => {
    try {
      const stats = await dashboardService.getDashboardStats();
      dispatch(fetchStatsSuccess(stats));
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao carregar estatísticas';
      dispatch(fetchDashboardError(errorMessage));
    }
  };
};

export const fetchRecentDeployments = () => {
  return async (dispatch: Dispatch<DashboardAction>) => {
    try {
      const deployments = await dashboardService.getRecentDeployments();
      dispatch(fetchRecentDeploymentsSuccess(deployments));
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao carregar deployments recentes';
      dispatch(fetchDashboardError(errorMessage));
    }
  };
};

export const fetchSystemStatus = () => {
  return async (dispatch: Dispatch<DashboardAction>) => {
    try {
      const status = await dashboardService.getSystemStatus();
      dispatch(fetchSystemStatusSuccess(status));
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Erro ao verificar status do sistema';
      dispatch(fetchDashboardError(errorMessage));
    }
  };
};

export const refreshDashboard = () => {
  return async (dispatch: Dispatch<DashboardAction>) => {
    // Limpar erros anteriores
    dispatch(clearError());
    
    // Recarregar todos os dados
    const thunkAction = fetchDashboardData();
    await thunkAction(dispatch);
  };
};

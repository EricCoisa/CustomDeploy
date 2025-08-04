import type { Publication } from '../publications/types';

export interface DashboardStats {
  totalDeployments: number;
  totalSites: number;
  totalApplications: number;
  totalAppPools: number;
}

export interface SystemStatus {
  iisStatus: 'online' | 'offline' | 'unknown';
  apiStatus: 'online' | 'offline';
}

export interface DashboardState {
  stats: DashboardStats;
  recentDeployments: Publication[];
  systemStatus: SystemStatus;
  isLoading: boolean;
  error: string | null;
  lastUpdated: string | null;
}

// Action Types
export const DASHBOARD_ACTIONS = {
  FETCH_DASHBOARD_START: 'dashboard/fetchStart',
  FETCH_DASHBOARD_SUCCESS: 'dashboard/fetchSuccess',
  FETCH_DASHBOARD_ERROR: 'dashboard/fetchError',
  FETCH_STATS_SUCCESS: 'dashboard/fetchStatsSuccess',
  FETCH_RECENT_DEPLOYMENTS_SUCCESS: 'dashboard/fetchRecentDeploymentsSuccess',
  FETCH_SYSTEM_STATUS_SUCCESS: 'dashboard/fetchSystemStatusSuccess',
  CLEAR_ERROR: 'dashboard/clearError',
} as const;

export type DashboardActionType = typeof DASHBOARD_ACTIONS[keyof typeof DASHBOARD_ACTIONS];

// Action Interfaces
export interface FetchDashboardStartAction {
  type: typeof DASHBOARD_ACTIONS.FETCH_DASHBOARD_START;
}

export interface FetchDashboardSuccessAction {
  type: typeof DASHBOARD_ACTIONS.FETCH_DASHBOARD_SUCCESS;
  payload: {
    stats: DashboardStats;
    recentDeployments: Publication[];
    systemStatus: SystemStatus;
  };
}

export interface FetchDashboardErrorAction {
  type: typeof DASHBOARD_ACTIONS.FETCH_DASHBOARD_ERROR;
  payload: string;
}

export interface FetchStatsSuccessAction {
  type: typeof DASHBOARD_ACTIONS.FETCH_STATS_SUCCESS;
  payload: DashboardStats;
}

export interface FetchRecentDeploymentsSuccessAction {
  type: typeof DASHBOARD_ACTIONS.FETCH_RECENT_DEPLOYMENTS_SUCCESS;
  payload: Publication[];
}

export interface FetchSystemStatusSuccessAction {
  type: typeof DASHBOARD_ACTIONS.FETCH_SYSTEM_STATUS_SUCCESS;
  payload: SystemStatus;
}

export interface ClearErrorAction {
  type: typeof DASHBOARD_ACTIONS.CLEAR_ERROR;
}

export type DashboardAction =
  | FetchDashboardStartAction
  | FetchDashboardSuccessAction
  | FetchDashboardErrorAction
  | FetchStatsSuccessAction
  | FetchRecentDeploymentsSuccessAction
  | FetchSystemStatusSuccessAction
  | ClearErrorAction;

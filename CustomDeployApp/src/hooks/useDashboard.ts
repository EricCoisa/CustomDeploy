import { useCallback } from 'react';
import { useAppDispatch } from '../store';
import { fetchDashboardData } from '../store/dashboard';

export const useDashboard = () => {
  const dispatch = useAppDispatch();

  const loadDashboardData = useCallback(async () => {
    return dispatch(fetchDashboardData());
  }, [dispatch]);

  return {
    loadDashboardData,
  };
};

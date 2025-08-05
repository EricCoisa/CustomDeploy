import React from 'react';
import styled from 'styled-components';
import { StatsCards } from './StatsCards';
import type { DashboardStats, SystemStatus } from '../../store/dashboard/types';

interface RightSidebarProps {
  stats: DashboardStats;
  systemStatus: SystemStatus;
  isLoading?: boolean;
  onDeploymentsClick?: () => void;
  onSitesClick?: () => void;
  onApplicationsClick?: () => void;
  onAppPoolsClick?: () => void;
}

const SidebarContainer = styled.div`
  display: flex;
  flex-direction: column;
  gap: 1rem;
  height: 100%;
  overflow: hidden;
  
  @media (max-width: 768px) {
    gap: 0.75rem;
  }
`;

const SystemStatusCard = styled.div`
  background: white;
  border-radius: 0.75rem;
  padding: 1.25rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06);
  border: 1px solid #f3f4f6;
  flex-shrink: 0;
  display: grid;
  grid-template-rows: auto 1fr;
  gap: 1rem;
  
  @media (max-width: 768px) {
    padding: 1rem;
    gap: 0.75rem;
  }
  
  @media (max-width: 480px) {
    padding: 0.875rem;
    gap: 0.5rem;
  }
`;

const StatusTitle = styled.h3`
  font-size: 1rem;
  font-weight: 600;
  color: #111827;
  margin: 0;
  
  @media (max-width: 480px) {
    font-size: 0.9rem;
  }
`;

const StatusGrid = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: 1fr 1fr;
  gap: 0.75rem;
  
  @media (max-width: 768px) {
    gap: 0.5rem;
  }
  
  @media (max-width: 480px) {
    grid-template-columns: 1fr;
    grid-template-rows: auto;
  }
`;

const StatusIndicator = styled.div<{ status: 'online' | 'offline' | 'unknown' }>`
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 1rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  min-width: 0;
  text-align: center;
  min-height: 80px;
  
  ${props => {
    switch (props.status) {
      case 'online':
        return `
          background-color: #dcfce7;
          color: #166534;
        `;
      case 'offline':
        return `
          background-color: #fee2e2;
          color: #991b1b;
        `;
      case 'unknown':
        return `
          background-color: #fef3c7;
          color: #92400e;
        `;
      default:
        return `
          background-color: #f3f4f6;
          color: #6b7280;
        `;
    }
  }}
  
  &::before {
    content: '';
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background-color: currentColor;
    flex-shrink: 0;
  }
  
  span:first-of-type {
    font-weight: 600;
    font-size: 0.8rem;
    opacity: 0.8;
  }
  
  span:last-of-type {
    font-weight: 700;
    font-size: 0.9rem;
  }
  
  @media (max-width: 768px) {
    padding: 0.875rem;
    min-height: 70px;
    
    &::before {
      width: 8px;
      height: 8px;
    }
  }
  
  @media (max-width: 480px) {
    font-size: 0.75rem;
    padding: 0.75rem;
    min-height: 60px;
    
    &::before {
      width: 6px;
      height: 6px;
    }
    
    span:first-of-type {
      font-size: 0.7rem;
    }
    
    span:last-of-type {
      font-size: 0.8rem;
    }
  }
`;

const StatsContainer = styled.div`
  flex: 1;
  min-height: 0;
  overflow: hidden;
`;

export const RightSidebar: React.FC<RightSidebarProps> = ({
  stats,
  systemStatus,
  isLoading = false,
  onDeploymentsClick,
  onSitesClick,
  onApplicationsClick,
  onAppPoolsClick
}) => {
  return (
    <SidebarContainer>
      {/* Card de Status do Sistema */}
      <SystemStatusCard>
        <StatusTitle>🖥️ Status do Sistema</StatusTitle>
        <StatusGrid>
          <StatusIndicator status={systemStatus.apiStatus}>
            <span>API</span>
            <span>{systemStatus.apiStatus === 'online' ? 'Online' : 'Offline'}</span>
          </StatusIndicator>
          
          <StatusIndicator status={systemStatus.iisStatus}>
            <span>IIS</span>
            <span>
              {systemStatus.iisStatus === 'online' ? 'Online' : 
               systemStatus.iisStatus === 'offline' ? 'Offline' : 'Desconhecido'}
            </span>
          </StatusIndicator>
          
          {systemStatus.adminStatus && (
            <StatusIndicator 
              status={systemStatus.adminStatus === 'admin' ? 'online' : 'offline'}
            >
              <span>Admin</span>
              <span>{systemStatus.adminStatus === 'admin' ? 'Sim' : 'Não'}</span>
            </StatusIndicator>
          )}
          
          {systemStatus.githubStatus && (
            <StatusIndicator 
              status={systemStatus.githubStatus === 'connected' ? 'online' : 'offline'}
            >
              <span>GitHub</span>
              <span>{systemStatus.githubStatus === 'connected' ? 'Conectado' : 'Desconectado'}</span>
            </StatusIndicator>
          )}
        </StatusGrid>
      </SystemStatusCard>

      {/* Cards de Estatísticas */}
      <StatsContainer>
        <StatsCards
          stats={stats}
          isLoading={isLoading}
          onDeploymentsClick={onDeploymentsClick}
          onSitesClick={onSitesClick}
          onApplicationsClick={onApplicationsClick}
          onAppPoolsClick={onAppPoolsClick}
        />
      </StatsContainer>
    </SidebarContainer>
  );
};

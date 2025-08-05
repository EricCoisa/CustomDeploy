import React from 'react';
import styled from 'styled-components';
import type { DashboardStats } from '../../store/dashboard/types';

interface StatsCardsProps {
  stats: DashboardStats;
  isLoading?: boolean;
  onDeploymentsClick?: () => void;
  onSitesClick?: () => void;
  onApplicationsClick?: () => void;
  onAppPoolsClick?: () => void;
}

const StatsContainer = styled.div`
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.75rem;
  flex-shrink: 0;
  max-width: 100%;
  
  @media (max-width: 768px) {
    grid-template-columns: repeat(2, 1fr);
    gap: 0.5rem;
  }
  
  @media (max-width: 480px) {
    grid-template-columns: repeat(2, 1fr);
    gap: 0.5rem;
  }
`;

const StatCard = styled.div<{ color: string; hasClick?: boolean }>`
  background: white;
  border-radius: 0.75rem;
  padding: 1rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06);
  border: 1px solid #f3f4f6;
  transition: all 0.2s ease;
  cursor: ${props => props.hasClick ? 'pointer' : 'default'};
  position: relative;
  overflow: hidden;
  min-width: 0;
  
  &::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 3px;
    background: ${props => props.color};
  }
  
  &:hover {
    transform: ${props => props.hasClick ? 'translateY(-1px)' : 'none'};
    box-shadow: ${props => props.hasClick 
      ? '0 4px 6px rgba(0, 0, 0, 0.1), 0 2px 4px rgba(0, 0, 0, 0.06)' 
      : '0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06)'
    };
  }
  
  @media (max-width: 768px) {
    padding: 0.875rem;
  }
  
  @media (max-width: 480px) {
    padding: 0.75rem;
  }
`;

const StatHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.5rem;
`;

const StatIcon = styled.div<{ color: string }>`
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border-radius: 0.5rem;
  background: ${props => `${props.color}15`};
  color: ${props => props.color};
  font-size: 1rem;
  
  @media (max-width: 768px) {
    width: 1.75rem;
    height: 1.75rem;
    font-size: 0.875rem;
  }
  
  @media (max-width: 480px) {
    width: 1.5rem;
    height: 1.5rem;
    font-size: 0.75rem;
  }
`;

const StatTitle = styled.h3`
  font-size: 0.75rem;
  font-weight: 500;
  color: #6b7280;
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  flex: 1;
  text-align: left;
  
  @media (max-width: 768px) {
    font-size: 0.7rem;
  }
`;

const StatValue = styled.div<{ color: string }>`
  font-size: 1.5rem;
  font-weight: 700;
  color: ${props => props.color};
  line-height: 1;
  word-break: break-all;
  
  @media (max-width: 768px) {
    font-size: 1.375rem;
  }
  
  @media (max-width: 480px) {
    font-size: 1.25rem;
  }
`;

const LoadingSkeleton = styled.div`
  height: 1.75rem;
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
  border-radius: 0.25rem;

  @keyframes loading {
    0% {
      background-position: 200% 0;
    }
    100% {
      background-position: -200% 0;
    }
  }
  
  @media (max-width: 768px) {
    height: 1.5rem;
  }
`;

interface StatItemProps {
  title: string;
  value: number;
  icon: string;
  color: string;
  isLoading?: boolean;
  onClick?: () => void;
}

const StatItem: React.FC<StatItemProps> = ({
  title,
  value,
  icon,
  color,
  isLoading = false,
  onClick
}) => (
  <StatCard color={color} hasClick={!!onClick} onClick={onClick}>
    <StatHeader>
      <StatTitle>{title}</StatTitle>
      <StatIcon color={color}>{icon}</StatIcon>
    </StatHeader>
    
    {isLoading ? (
      <LoadingSkeleton />
    ) : (
      <StatValue color={color}>
        {value.toLocaleString('pt-BR')}
      </StatValue>
    )}
  </StatCard>
);

export const StatsCards: React.FC<StatsCardsProps> = ({
  stats,
  isLoading = false,
  onDeploymentsClick,
  onSitesClick,
  onApplicationsClick,
  onAppPoolsClick
}) => {
  return (
    <StatsContainer>
      <StatItem
        title="Total de Deployments"
        value={stats.totalDeployments}
        icon="🚀"
        color="#10b981"
        isLoading={isLoading}
        onClick={onDeploymentsClick}
      />
      
      <StatItem
        title="Sites IIS"
        value={stats.totalSites}
        icon="🌐"
        color="#3b82f6"
        isLoading={isLoading}
        onClick={onSitesClick}
      />
      
      <StatItem
        title="Aplicações"
        value={stats.totalApplications}
        icon="📱"
        color="#8b5cf6"
        isLoading={isLoading}
        onClick={onApplicationsClick}
      />
      
      <StatItem
        title="Pools de Aplicação"
        value={stats.totalAppPools}
        icon="🔧"
        color="#f59e0b"
        isLoading={isLoading}
        onClick={onAppPoolsClick}
      />
    </StatsContainer>
  );
};

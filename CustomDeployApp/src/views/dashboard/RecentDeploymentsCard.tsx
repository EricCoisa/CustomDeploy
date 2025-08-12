import React from 'react';
import styled from 'styled-components';
import { Button } from '../../components';
import type { Publication } from '../../store/publications/types';

interface RecentDeploymentsCardProps {
  deployments: Publication[];
  isLoading?: boolean;
  onRedeploy?: (deployment: Publication) => void;
}

const MainCard = styled.div`
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 1rem;
  padding: 1.5rem;
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1), 0 4px 6px rgba(0, 0, 0, 0.04);
  color: white;
  height: 100%;
  display: flex;
  flex-direction: column;
  min-height: 0;
  
  @media (max-width: 768px) {
    padding: 1.25rem;
    margin-bottom: 1rem;
  }
  
  @media (max-width: 480px) {
    padding: 1rem;
  }
`;

const CardHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
  flex-shrink: 0;
  
  @media (max-width: 768px) {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }
`;

const CardTitle = styled.h2`
  font-size: 1.375rem;
  font-weight: 700;
  margin: 0;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  
  @media (max-width: 768px) {
    font-size: 1.25rem;
  }
  
  @media (max-width: 480px) {
    font-size: 1.125rem;
  }
`;

const ViewAllButton = styled(Button)`
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: white;
  font-size: 0.875rem;
  padding: 0.5rem 1rem;
  
  &:hover {
    background: rgba(255, 255, 255, 0.3);
    transform: translateY(-1px);
  }
`;

const DeploymentsList = styled.div`
  display: grid;
  gap: 0.75rem;
  max-width: 100%;
  overflow-y: auto;
  flex: 1;
  align-content: start;
  padding-right: 0.5rem;
  
  /* Scrollbar styling */
  &::-webkit-scrollbar {
    width: 6px;
  }
  
  &::-webkit-scrollbar-track {
    background: rgba(255, 255, 255, 0.1);
    border-radius: 3px;
  }
  
  &::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.3);
    border-radius: 3px;
  }
  
  &::-webkit-scrollbar-thumb:hover {
    background: rgba(255, 255, 255, 0.5);
  }
  
  @media (min-width: 1200px) {
    grid-template-columns: repeat(2, 1fr);
  }
  
  @media (max-width: 1199px) and (min-width: 768px) {
    grid-template-columns: 1fr;
  }
  
  @media (max-width: 767px) {
    grid-template-columns: 1fr;
    overflow-y: visible;
    padding-right: 0;
  }
`;

const DeploymentItem = styled.div`
  background: rgba(255, 255, 255, 0.15);
  backdrop-filter: blur(10px);
  border-radius: 0.75rem;
  padding: 1rem;
  border: 1px solid rgba(255, 255, 255, 0.2);
  transition: all 0.2s ease;
  min-width: 0;
  overflow: hidden;
  
  &:hover {
    background: rgba(255, 255, 255, 0.2);
    transform: translateY(-1px);
  }
  
  @media (max-width: 480px) {
    padding: 0.875rem;
  }
`;

const DeploymentHeader = styled.div`
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: 0.75rem;
  gap: 1rem;
`;

const DeploymentInfo = styled.div`
  flex: 1;
  min-width: 0;
`;

const ProjectName = styled.h3`
  font-size: 1.125rem;
  font-weight: 600;
  margin: 0 0 0.25rem 0;
  color: white;
  word-break: break-word;
  overflow-wrap: break-word;
  hyphens: auto;
`;

const SubPath = styled.div`
  font-size: 0.875rem;
  color: rgba(255, 255, 255, 0.8);
  margin-bottom: 0.5rem;
`;

const DeploymentMeta = styled.div`
  display: flex;
  align-items: center;
  gap: 1rem;
  font-size: 0.875rem;
  color: rgba(255, 255, 255, 0.9);
  margin-bottom: 1rem;
  flex-wrap: wrap;
`;

const StatusBadge = styled.span<{ status: 'success' | 'warning' | 'error' }>`
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  backdrop-filter: blur(10px);
  
  ${props => {
    switch (props.status) {
      case 'success':
        return `
          background: rgba(34, 197, 94, 0.2);
          color: #bbf7d0;
          border: 1px solid rgba(34, 197, 94, 0.3);
        `;
      case 'warning':
        return `
          background: rgba(251, 191, 36, 0.2);
          color: #fef3c7;
          border: 1px solid rgba(251, 191, 36, 0.3);
        `;
      case 'error':
        return `
          background: rgba(239, 68, 68, 0.2);
          color: #fecaca;
          border: 1px solid rgba(239, 68, 68, 0.3);
        `;
      default:
        return `
          background: rgba(156, 163, 175, 0.2);
          color: rgba(255, 255, 255, 0.8);
          border: 1px solid rgba(156, 163, 175, 0.3);
        `;
    }
  }}
  
  &::before {
    content: '';
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background-color: currentColor;
  }
`;

const RedeployButton = styled(Button)`
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: white;
  font-size: 0.75rem;
  padding: 0.375rem 0.75rem;
  min-width: auto;
  
  &:hover {
    background: rgba(255, 255, 255, 0.3);
    transform: translateY(-1px);
  }
  
  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
`;

const EmptyState = styled.div`
  text-align: center;
  padding: 3rem 1rem;
  color: rgba(255, 255, 255, 0.8);
`;

const LoadingCard = styled.div`
  background: rgba(255, 255, 255, 0.15);
  backdrop-filter: blur(10px);
  border-radius: 0.75rem;
  padding: 1.25rem;
  border: 1px solid rgba(255, 255, 255, 0.2);
`;

const LoadingSkeleton = styled.div`
  height: 1rem;
  background: linear-gradient(90deg, 
    rgba(255, 255, 255, 0.1) 25%, 
    rgba(255, 255, 255, 0.2) 50%, 
    rgba(255, 255, 255, 0.1) 75%
  );
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
  border-radius: 0.25rem;
  margin-bottom: 0.75rem;

  @keyframes loading {
    0% {
      background-position: 200% 0;
    }
    100% {
      background-position: -200% 0;
    }
  }
  
  &:last-child {
    margin-bottom: 0;
  }
  
  &.wide {
    width: 60%;
  }
  
  &.narrow {
    width: 40%;
  }
`;

export const RecentDeploymentsCard: React.FC<RecentDeploymentsCardProps> = ({
  deployments,
  isLoading = false,
  onRedeploy
}) => {
  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getDeploymentStatus = (deployment: Publication): 'success' | 'warning' | 'error' => {
    if (!deployment.exists) return 'error';
    if (!deployment.hasMetadata) return 'warning';
    return 'success';
  };

  const getStatusText = (deployment: Publication): string => {
    if (!deployment.exists) return 'Não encontrado';
    if (!deployment.hasMetadata) return 'Sem metadados';
    return 'Sucesso';
  };

  const handleRedeploy = (deployment: Publication) => {
    if (onRedeploy) {
      onRedeploy(deployment);
    }
  };

  // Mostrar apenas os 4 primeiros deployments
  const displayDeployments = deployments.slice(0, 4);
  console.log("Deployments a serem exibidos:", displayDeployments);
  return (
    <MainCard>
      <CardHeader>
        <CardTitle>
          🚀 Últimos Deployments
        </CardTitle>
        <ViewAllButton onClick={() => {}}>
          Ver Todos ({deployments.length})
        </ViewAllButton>
      </CardHeader>

      <DeploymentsList>
        {isLoading ? (
          // Loading state - mostrar 3 cards de loading
          Array.from({ length: 3 }).map((_, index) => (
            <LoadingCard key={index}>
              <LoadingSkeleton className="wide" />
              <LoadingSkeleton className="narrow" />
              <LoadingSkeleton />
              <LoadingSkeleton className="wide" />
            </LoadingCard>
          ))
        ) : displayDeployments.length === 0 ? (
          // Empty state
          <EmptyState>
            <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>📭</div>
            <h3 style={{ margin: '0 0 0.5rem 0', fontSize: '1.25rem' }}>
              Nenhum deployment encontrado
            </h3>
            <p style={{ margin: 0, opacity: 0.8 }}>
              Os deployments aparecerão aqui após serem executados
            </p>
          </EmptyState>
        ) : (
          // Data cards
          displayDeployments.map((deployment, index) => (
            <DeploymentItem key={deployment.name + index}>
              <DeploymentHeader>
                <DeploymentInfo>
                  <ProjectName>{deployment.name}</ProjectName>
                  {deployment.subApplication && (
                    <SubPath>/{deployment.subApplication}</SubPath>
                  )}
                </DeploymentInfo>
                <RedeployButton 
                  onClick={() => handleRedeploy(deployment)}
                  disabled={!deployment.exists}
                >
                  🔄 Re-deploy
                </RedeployButton>
              </DeploymentHeader>

              <DeploymentMeta>
                <div>
                  📅 {deployment.deployedAt 
                    ? formatDate(deployment.deployedAt)
                    : 'Não definido'
                  }
                </div>
                <div>
                  📦 {deployment.sizeMB > 0 
                    ? `${deployment.sizeMB.toFixed(1)} MB`
                    : 'N/A'
                  }
                </div>
              </DeploymentMeta>

              <StatusBadge status={getDeploymentStatus(deployment)}>
                {getStatusText(deployment)}
              </StatusBadge>
            </DeploymentItem>
          ))
        )}
      </DeploymentsList>
    </MainCard>
  );
};

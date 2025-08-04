import React from 'react';
import { Button } from '../../components';
import type { IISAppPool } from '../../store/iis/types';
import styled from 'styled-components';

const AppPoolsHeader = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  
  @media (max-width: 768px) {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
`;

const AppPoolsTitle = styled.h3`
  margin: 0;
  color: #1f2937;
  font-size: 1.25rem;
`;

const AppPoolsList = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
`;

const AppPoolItem = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  margin-bottom: 0.5rem;
  background: #fafafa;

  &:last-child {
    margin-bottom: 0;
  }
`;

const AppPoolInfo = styled.div`
  flex: 1;
`;

const AppPoolName = styled.div`
  font-weight: bold;
  color: #1f2937;
  margin-bottom: 0.25rem;
`;

const AppPoolDetails = styled.div`
  font-size: 0.9rem;
  color: #6b7280;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 0.5rem;
  
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
`;

const StatusBadge = styled.span<{ status: string }>`
  padding: 0.25rem 0.5rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: bold;
  background-color: ${props => 
    props.status === 'Started' ? '#28a745' : 
    props.status === 'Stopped' ? '#dc3545' : '#6c757d'};
  color: white;
`;

const ActionButtons = styled.div`
  display: flex;
  gap: 0.5rem;
  
  @media (max-width: 768px) {
    justify-content: center;
    flex-wrap: wrap;
  }
`;

const EmptyState = styled.div`
  text-align: center;
  padding: 2rem;
  color: #6b7280;
  font-style: italic;
`;

interface AppPoolsListComponentProps {
  appPools: IISAppPool[];
  onCreateAppPool: () => void;
  onEditAppPool: (appPool: IISAppPool) => void;
  onDeleteAppPool: (poolName: string) => void;
  onStartAppPool: (poolName: string) => void;
  onStopAppPool: (poolName: string) => void;
  loading?: boolean;
}

export const AppPoolsListComponent: React.FC<AppPoolsListComponentProps> = ({
  appPools,
  onCreateAppPool,
  onEditAppPool,
  onDeleteAppPool,
  onStartAppPool,
  onStopAppPool,
  loading = false
}) => {
  if (loading) {
    return (
      <div>
        <AppPoolsHeader>
          <AppPoolsTitle>Application Pools</AppPoolsTitle>
        </AppPoolsHeader>
        <EmptyState>Carregando...</EmptyState>
      </div>
    );
  }

  return (
    <div>
      <AppPoolsHeader>
        <AppPoolsTitle>Application Pools ({appPools.length})</AppPoolsTitle>
        <Button onClick={onCreateAppPool}>
          + Novo Application Pool
        </Button>
      </AppPoolsHeader>

      <AppPoolsList>
        {appPools.length > 0 ? (
          appPools.map((pool) => (
            <AppPoolItem key={pool.name}>
              <AppPoolInfo>
                <AppPoolName>{pool.name}</AppPoolName>
                <AppPoolDetails>
                  <div>
                    <strong>Status:</strong> <StatusBadge status={pool.state}>{pool.state}</StatusBadge>
                  </div>
                  <div><strong>.NET Version:</strong> {pool.managedRuntimeVersion || 'No Managed Code'}</div>
                  <div><strong>Pipeline:</strong> {pool.managedPipelineMode}</div>
                  <div><strong>Identity:</strong> {pool.processModel?.identityType || 'N/A'}</div>
                  <div><strong>Max Processes:</strong> {pool.processModel?.maxProcesses || 'N/A'}</div>
                  <div><strong>Idle Timeout:</strong> {pool.processModel?.idleTimeout || 'N/A'}</div>
                </AppPoolDetails>
              </AppPoolInfo>
              
              <ActionButtons>
                {pool.state === 'Started' ? (
                  <Button 
                    size="small" 
                    variant="secondary"
                    onClick={() => onStopAppPool(pool.name)}
                  >
                    ⏹ Parar
                  </Button>
                ) : (
                  <Button 
                    size="small" 
                    variant="primary"
                    onClick={() => onStartAppPool(pool.name)}
                  >
                    ▶ Iniciar
                  </Button>
                )}
                <Button 
                  size="small" 
                  variant="secondary"
                  onClick={() => onEditAppPool(pool)}
                >
                  Editar
                </Button>
                <Button 
                  size="small" 
                  variant="danger"
                  onClick={() => onDeleteAppPool(pool.name)}
                >
                  Excluir
                </Button>
              </ActionButtons>
            </AppPoolItem>
          ))
        ) : (
          <EmptyState>
            Nenhum Application Pool encontrado
          </EmptyState>
        )}
      </AppPoolsList>
    </div>
  );
};

import React from 'react';
import styled from 'styled-components';
import type { PublicationStats } from '../../../store/publications/types';

interface StatsCardProps {
  stats: PublicationStats;
}

const StatsContainer = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
`;

const StatCard = styled.div`
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 1.25rem;
  border-radius: 0.75rem;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.2);
  text-align: center;
  transition: all 0.3s ease;

  &:hover {
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
    transform: translateY(-2px);
  }
`;

const StatValue = styled.div`
  font-size: 2rem;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 0.5rem;
`;

const StatLabel = styled.div`
  font-size: 0.875rem;
  color: #6b7280;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.05em;
`;

const StatSubValue = styled.div`
  font-size: 0.75rem;
  color: #9ca3af;
  margin-top: 0.25rem;
`;

const SummaryCard = styled.div`
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 1.5rem;
  border-radius: 1rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
  border: 1px solid rgba(255, 255, 255, 0.2);
`;

const SummaryTitle = styled.h3`
  margin: 0 0 1rem 0;
  color: #1f2937;
  font-size: 1.125rem;
  font-weight: 600;
`;

const SummaryGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
`;

const SummaryItem = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
`;

const SummaryLabel = styled.span`
  font-size: 0.75rem;
  font-weight: 600;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
`;

const SummaryValue = styled.span`
  font-size: 0.875rem;
  color: #1f2937;
  
  &.empty {
    color: #9ca3af;
    font-style: italic;
  }
`;

export const StatsCard: React.FC<StatsCardProps> = ({ stats }) => {
  const formatSize = (sizeMB: number) => {
    if (sizeMB >= 1024) {
      return `${(sizeMB / 1024).toFixed(1)} GB`;
    }
    return `${sizeMB.toFixed(1)} MB`;
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('pt-BR');
  };

  return (
    <>
      <StatsContainer>
        <StatCard>
          <StatValue>{stats.totalPublications}</StatValue>
          <StatLabel>Total de Publicações</StatLabel>
          <StatSubValue>
            {stats.sites} sites • {stats.applications} aplicações
          </StatSubValue>
        </StatCard>

        <StatCard>
          <StatValue>{stats.withMetadata}</StatValue>
          <StatLabel>Com Metadados</StatLabel>
          <StatSubValue>
            {stats.withoutMetadata} sem metadados
          </StatSubValue>
        </StatCard>

        <StatCard>
          <StatValue>{formatSize(stats.totalSizeMB)}</StatValue>
          <StatLabel>Tamanho Total</StatLabel>
          <StatSubValue>
            Média: {formatSize(stats.averageSizeMB)}
          </StatSubValue>
        </StatCard>
      </StatsContainer>

      {(stats.latestDeployment || stats.oldestDeployment || stats.largestPublication || stats.smallestPublication) && (
        <SummaryCard>
          <SummaryTitle>Resumo Detalhado</SummaryTitle>
          <SummaryGrid>
            {stats.latestDeployment && (
              <SummaryItem>
                <SummaryLabel>Último Deploy</SummaryLabel>
                <SummaryValue>
                  {stats.latestDeployment.name}
                </SummaryValue>
                <SummaryValue style={{ fontSize: '0.75rem', color: '#6b7280' }}>
                  {formatDate(stats.latestDeployment.deployedAt)}
                </SummaryValue>
              </SummaryItem>
            )}

            {stats.oldestDeployment && (
              <SummaryItem>
                <SummaryLabel>Deploy Mais Antigo</SummaryLabel>
                <SummaryValue>
                  {stats.oldestDeployment.name}
                </SummaryValue>
                <SummaryValue style={{ fontSize: '0.75rem', color: '#6b7280' }}>
                  {formatDate(stats.oldestDeployment.deployedAt)}
                </SummaryValue>
              </SummaryItem>
            )}

            {stats.largestPublication && (
              <SummaryItem>
                <SummaryLabel>Maior Publicação</SummaryLabel>
                <SummaryValue>
                  {stats.largestPublication.name}
                </SummaryValue>
                <SummaryValue style={{ fontSize: '0.75rem', color: '#6b7280' }}>
                  {formatSize(stats.largestPublication.sizeMB)}
                </SummaryValue>
              </SummaryItem>
            )}

            {stats.smallestPublication && (
              <SummaryItem>
                <SummaryLabel>Menor Publicação</SummaryLabel>
                <SummaryValue>
                  {stats.smallestPublication.name}
                </SummaryValue>
                <SummaryValue style={{ fontSize: '0.75rem', color: '#6b7280' }}>
                  {formatSize(stats.smallestPublication.sizeMB)}
                </SummaryValue>
              </SummaryItem>
            )}
          </SummaryGrid>
        </SummaryCard>
      )}
    </>
  );
};

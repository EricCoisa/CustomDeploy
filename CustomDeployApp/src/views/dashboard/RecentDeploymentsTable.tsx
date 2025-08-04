import React from 'react';
import styled from 'styled-components';
import type { Publication } from '../../store/publications/types';

interface RecentDeploymentsTableProps {
  deployments: Publication[];
  isLoading?: boolean;
}

const TableContainer = styled.div`
  background: #ffffff;
  border-radius: 0.75rem;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06);
  border: 1px solid #e5e7eb;
`;

const TableHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #e5e7eb;
`;

const TableTitle = styled.h3`
  font-size: 1.125rem;
  font-weight: 600;
  color: #111827;
  margin: 0;
`;

const Table = styled.table`
  width: 100%;
  border-collapse: collapse;
`;

const TableHead = styled.thead`
  background-color: #f9fafb;
`;

const TableHeadRow = styled.tr`
  border-bottom: 1px solid #e5e7eb;
`;

const TableHeaderCell = styled.th`
  padding: 0.75rem;
  text-align: left;
  font-size: 0.75rem;
  font-weight: 500;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
`;

const TableBody = styled.tbody``;

const TableRow = styled.tr`
  border-bottom: 1px solid #f3f4f6;
  
  &:hover {
    background-color: #f9fafb;
  }

  &:last-child {
    border-bottom: none;
  }
`;

const TableCell = styled.td`
  padding: 0.75rem;
  font-size: 0.875rem;
  color: #374151;
`;

const ProjectName = styled.div`
  font-weight: 500;
  color: #111827;
`;

const SubPath = styled.div`
  font-size: 0.75rem;
  color: #6b7280;
  margin-top: 0.25rem;
`;

const StatusBadge = styled.span<{ status: 'success' | 'warning' | 'error' }>`
  display: inline-flex;
  align-items: center;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  
  ${props => {
    switch (props.status) {
      case 'success':
        return `
          background-color: #dcfce7;
          color: #166534;
        `;
      case 'warning':
        return `
          background-color: #fef3c7;
          color: #92400e;
        `;
      case 'error':
        return `
          background-color: #fee2e2;
          color: #991b1b;
        `;
      default:
        return `
          background-color: #f3f4f6;
          color: #6b7280;
        `;
    }
  }}
`;

const EmptyState = styled.div`
  text-align: center;
  padding: 3rem 1rem;
  color: #6b7280;
`;

const LoadingRow = styled.tr`
  td {
    padding: 1rem 0.75rem;
  }
`;

const LoadingSkeleton = styled.div`
  height: 1rem;
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
`;

export const RecentDeploymentsTable: React.FC<RecentDeploymentsTableProps> = ({
  deployments,
  isLoading = false
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

  return (
    <TableContainer>
      <TableHeader>
        <TableTitle>📋 Últimos Deployments</TableTitle>
      </TableHeader>

      <Table>
        <TableHead>
          <TableHeadRow>
            <TableHeaderCell>Projeto</TableHeaderCell>
            <TableHeaderCell>Data/Hora</TableHeaderCell>
            <TableHeaderCell>Status</TableHeaderCell>
            <TableHeaderCell>Tamanho</TableHeaderCell>
          </TableHeadRow>
        </TableHead>
        <TableBody>
          {isLoading ? (
            // Loading state
            Array.from({ length: 3 }).map((_, index) => (
              <LoadingRow key={index}>
                <TableCell><LoadingSkeleton /></TableCell>
                <TableCell><LoadingSkeleton /></TableCell>
                <TableCell><LoadingSkeleton /></TableCell>
                <TableCell><LoadingSkeleton /></TableCell>
              </LoadingRow>
            ))
          ) : deployments.length === 0 ? (
            // Empty state
            <TableRow>
              <TableCell colSpan={4}>
                <EmptyState>
                  <div>📭</div>
                  <p>Nenhum deployment encontrado</p>
                </EmptyState>
              </TableCell>
            </TableRow>
          ) : (
            // Data rows
            deployments.map((deployment, index) => (
              <TableRow key={deployment.name + index}>
                <TableCell>
                  <ProjectName>{deployment.name}</ProjectName>
                  {deployment.subApplication && (
                    <SubPath>/{deployment.subApplication}</SubPath>
                  )}
                </TableCell>
                <TableCell>
                  {deployment.deployedAt 
                    ? formatDate(deployment.deployedAt)
                    : '-'
                  }
                </TableCell>
                <TableCell>
                  <StatusBadge status={getDeploymentStatus(deployment)}>
                    {getStatusText(deployment)}
                  </StatusBadge>
                </TableCell>
                <TableCell>
                  {deployment.sizeMB > 0 
                    ? `${deployment.sizeMB.toFixed(1)} MB`
                    : '-'
                  }
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

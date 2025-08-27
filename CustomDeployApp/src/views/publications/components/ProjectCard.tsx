import React from 'react';
import styled from 'styled-components';
import { Button } from '../../../components';
import type { Publication } from '../../../store/publications/types';

interface ProjectCardProps {
  publication: Publication;
  onEdit?: (publication: Publication) => void;
  onReDeploy?: (publication: Publication) => void;
}

const Card = styled.div`
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 1.5rem;
  border-radius: 1rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  margin-bottom: 1rem;
  border: 1px solid rgba(255, 255, 255, 0.2);
  transition: all 0.3s ease;

  &:hover {
    box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15);
    transform: translateY(-2px);
  }
`;

const CardHeader = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
  
  @media (max-width: 768px) {
    flex-direction: column;
    gap: 1rem;
  }
`;

const ProjectName = styled.h3`
  margin: 0;
  color: #1f2937;
  font-size: 1.25rem;
  font-weight: 600;
`;

const StatusBadge = styled.span<{ exists: boolean; hasMetadata: boolean }>`
  padding: 0.25rem 0.75rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: white;
  background: ${props => {
    if (!props.exists) return '#dc2626'; // Vermelho - não existe
    if (!props.hasMetadata) return '#f59e0b'; // Amarelo - sem metadados
    return '#10b981'; // Verde - OK
  }};
`;

const CardContent = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
`;

const InfoSection = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
`;

const InfoLabel = styled.span`
  font-size: 0.75rem;
  font-weight: 600;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
`;

const InfoValue = styled.span`
  font-size: 0.875rem;
  color: #1f2937;
  word-break: break-all;
  
  &.empty {
    color: #9ca3af;
    font-style: italic;
  }
`;

const PathInfo = styled.div`
  background: #f3f4f6;
  padding: 0.75rem;
  border-radius: 0.5rem;
  margin: 1rem 0;
`;

const ButtonGroup = styled.div`
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
  
  @media (max-width: 768px) {
    flex-direction: column;
  }
`;

const ProjectTypeTag = styled.span<{ isSubProject: boolean }>`
  display: inline-block;
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
  font-weight: 500;
  border-radius: 0.25rem;
  color: white;
  background: ${props => props.isSubProject ? '#8b5cf6' : '#3b82f6'};
  margin-left: 0.5rem;
`;

export const ProjectCard: React.FC<ProjectCardProps> = ({
  publication,
  onEdit,
  onReDeploy
}) => {
  const getStatusText = () => {
    if (!publication.exists) return 'Não encontrado no IIS';
    if (!publication.hasMetadata) return 'Sem metadados';
    return 'Ativo';
  };

  const isActive = () => {
    return publication.exists && publication.hasMetadata;
  };

  const formatSize = (sizeMB: number) => {
    if (sizeMB >= 1024) {
      return `${(sizeMB / 1024).toFixed(1)} GB`;
    }
    return `${sizeMB.toFixed(1)} MB`;
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleString('pt-BR');
  };

  return (
    <Card>
      <CardHeader>
        <div>
          <ProjectName>
            {publication.name}
            <ProjectTypeTag isSubProject={publication.isSubProject}>
              {publication.isSubProject ? 'Sub-aplicação' : 'Site'}
            </ProjectTypeTag>
          </ProjectName>
        </div>
        <StatusBadge exists={publication.exists} hasMetadata={publication.hasMetadata}>
          {getStatusText()}
        </StatusBadge>
      </CardHeader>

      <CardContent>
        <InfoSection>
          <InfoLabel>Repositório</InfoLabel>
          <InfoValue className={!publication.repoUrl ? 'empty' : ''}>
            {publication.repoUrl || publication.repository || 'Não configurado'}
          </InfoValue>
        </InfoSection>

        <InfoSection>
          <InfoLabel>Branch</InfoLabel>
          <InfoValue className={!publication.branch ? 'empty' : ''}>
            {publication.branch || 'Não configurado'}
          </InfoValue>
        </InfoSection>

        <InfoSection>
          <InfoLabel>Comandos Build</InfoLabel>
          <InfoValue className={!publication.buildCommand?.length ? 'empty' : ''}>
            {Array.isArray(publication.buildCommand) && publication.buildCommand.length > 0
              ? publication.buildCommand.map((cmd, index) => (
                  <div key={index}>
                    {cmd.comando} (Terminal: {cmd.terminalId})
                  </div>
                ))
              : 'Não configurado'}
          </InfoValue>
        </InfoSection>

        <InfoSection>
          <InfoLabel>Saída do Build</InfoLabel>
          <InfoValue className={!publication.buildOutput ? 'empty' : ''}>
            {publication.buildOutput || 'Não configurado'}
          </InfoValue>
        </InfoSection>

        <InfoSection>
          <InfoLabel>Tamanho</InfoLabel>
          <InfoValue>{formatSize(publication.sizeMB)}</InfoValue>
        </InfoSection>

        <InfoSection>
          <InfoLabel>Último Deploy</InfoLabel>
          <InfoValue>{formatDate(publication.deployedAt)}</InfoValue>
        </InfoSection>
      </CardContent>

      <PathInfo>
        <InfoLabel>Caminho de Destino</InfoLabel>
        <InfoValue>{publication.targetPath}</InfoValue>
      </PathInfo>

      <ButtonGroup>
        {isActive() && onReDeploy && (
          <Button
            size="small"
            variant="primary"
            onClick={() => onReDeploy(publication)}
          >
            🚀 Re-Deploy
          </Button>
        )}

        {!isActive() && onEdit && (
          <Button
            size="small"
            variant="secondary"
            onClick={() => onEdit(publication)}
          >
            ⚙️ Configurar Deploy
          </Button>
        )}
      </ButtonGroup>
    </Card>
  );
};

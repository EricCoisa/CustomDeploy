import React, { useEffect, useState } from 'react';
import { ProtectedLayout, Button } from '../../components';
import { ProjectCard, StatsCard, ReDeployModal } from './components';
import { useAppDispatch, useAppSelector } from '../../store';
import {
  fetchPublications,
  fetchPublicationsStats,
  clearErrors
} from '../../store/publications/actions';
import type { Publication } from '../../store/publications/types';
import { toast } from 'react-toastify';
import styled from 'styled-components';
import { ConfigureDeployModal } from './components/ConfigureDeployModal';

const ContentCard = styled.div`
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 1.5rem;
  border-radius: 1rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  margin-bottom: 1rem;
  width: 100%;
  box-sizing: border-box;
`;

const SectionHeader = styled.div`
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

const SectionTitle = styled.h2`
  margin: 0;
  color: #1f2937;
  font-size: 1.25rem;
`;

const LoadingState = styled.div`
  text-align: center;
  padding: 2rem;
  color: #6b7280;
  font-style: italic;
`;

const ErrorState = styled.div`
  background-color: #fef2f2;
  border: 1px solid #fecaca;
  color: #dc2626;
  padding: 1rem;
  border-radius: 0.5rem;
  margin-bottom: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  
  @media (max-width: 768px) {
    flex-direction: column;
    gap: 0.5rem;
    align-items: stretch;
  }
`;

const EmptyState = styled.div`
  text-align: center;
  padding: 3rem;
  color: #6b7280;
`;

const EmptyStateIcon = styled.div`
  font-size: 4rem;
  margin-bottom: 1rem;
  color: #d1d5db;
`;

const EmptyStateTitle = styled.h3`
  font-size: 1.25rem;
  color: #374151;
  margin-bottom: 0.5rem;
`;

const EmptyStateText = styled.p`
  color: #6b7280;
  margin-bottom: 2rem;
`;

const FilterContainer = styled.div`
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
  
  @media (max-width: 768px) {
    flex-direction: column;
  }
`;

const FilterButton = styled.button<{ active: boolean }>`
  padding: 0.5rem 1rem;
  border: 1px solid ${props => props.active ? '#3b82f6' : '#d1d5db'};
  background: ${props => props.active ? '#3b82f6' : 'white'};
  color: ${props => props.active ? 'white' : '#374151'};
  cursor: pointer;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: ${props => props.active ? '600' : '400'};
  transition: all 0.2s ease;

  &:hover {
    background: ${props => props.active ? '#2563eb' : '#f9fafb'};
    border-color: ${props => props.active ? '#2563eb' : '#9ca3af'};
  }
`;

const RefreshButton = styled(Button)`
  display: flex;
  align-items: center;
  gap: 0.5rem;
`;

type FilterType = 'all' | 'withMetadata' | 'withoutMetadata' | 'notExists' | 'subProjects' | 'sites';

export const PublicationsView: React.FC = () => {
  const dispatch = useAppDispatch();
  const { 
    publications, 
    stats, 
    loading, 
    error 
  } = useAppSelector(state => state.publications);

  const [filter, setFilter] = useState<FilterType>('all');
  const [reDeployModal, setReDeployModal] = useState<{
    isOpen: boolean;
    publication: Publication | null;
  }>({
    isOpen: false,
    publication: null
  });

  const [configureDeployModal, setConfigureDeployModal] = useState<{
    isOpen: boolean;
    publication: Publication | null;
  }>({
    isOpen: false,
    publication: null
  });

  useEffect(() => {
    // Carregar dados ao montar o componente
    dispatch(fetchPublications());
    dispatch(fetchPublicationsStats());
  }, [dispatch]);

  // Filtrar publicações com base no filtro selecionado
  const filteredPublications = publications.filter((publication) => {
    switch (filter) {
      case 'withMetadata':
        return publication.hasMetadata;
      case 'withoutMetadata':
        return !publication.hasMetadata;
      case 'notExists':
        return !publication.exists;
      case 'subProjects':
        return publication.isSubProject;
      case 'sites':
        return !publication.isSubProject;
      default:
        return true;
    }
  });

  // Handlers
  const handleRefresh = () => {
    dispatch(fetchPublications());
    dispatch(fetchPublicationsStats());
  };

  const handleEditPublication = (publication: Publication) => {
    setConfigureDeployModal({
      isOpen: true,
      publication
    });
  };

  const handleConfigureDeployModalClose = () => {
    setConfigureDeployModal({
      isOpen: false,
      publication: null
    });
  };

  const handleConfigureDeploySuccess = (result: Record<string, unknown>) => {
    toast.success(`Configuração salva com sucesso! ${result.message || ''}`);
    handleConfigureDeployModalClose();
    // Atualizar dados após configuração bem-sucedida
    dispatch(fetchPublications());
    dispatch(fetchPublicationsStats());
  };

  const handleConfigureDeployError = (error: string) => {
    toast.error(`Erro ao salvar configuração: ${error}`);
  };



  const handleReDeploy = (publication: Publication) => {
    setReDeployModal({
      isOpen: true,
      publication
    });
  };

  const handleReDeployModalClose = () => {
    setReDeployModal({
      isOpen: false,
      publication: null
    });
  };

  const handleReDeploySuccess = (result: Record<string, unknown>) => {
    toast.success(`Re-Deploy executado com sucesso! ${result.message || ''}`);
    // Atualizar dados após deploy bem-sucedido
    dispatch(fetchPublications());
    dispatch(fetchPublicationsStats());
  };

  const handleReDeployError = (error: string) => {
    toast.error(`Erro no re-deploy: ${error}`);
  };

  const getFilterLabel = (filterType: FilterType) => {
    switch (filterType) {
      case 'all':
        return `Todas (${publications.length})`;
      case 'withMetadata':
        return `Com Metadados (${publications.filter(p => p.hasMetadata).length})`;
      case 'withoutMetadata':
        return `Sem Metadados (${publications.filter(p => !p.hasMetadata).length})`;
      case 'notExists':
        return `Não Existem (${publications.filter(p => !p.exists).length})`;
      case 'subProjects':
        return `Sub-aplicações (${publications.filter(p => p.isSubProject).length})`;
      case 'sites':
        return `Sites (${publications.filter(p => !p.isSubProject).length})`;
      default:
        return '';
    }
  };

  return (
    <ProtectedLayout title="Gerenciamento de Publicações">
      {/* Estatísticas */}
      {stats && (
        <ContentCard>
          <StatsCard stats={stats} />
        </ContentCard>
      )}

      {/* Mensagens de Erro */}
      {error.publications && (
        <ErrorState>
          <strong>Erro:</strong> {error.publications}
          <Button 
            size="small" 
            variant="secondary" 
            onClick={() => dispatch(clearErrors())}
            style={{ marginLeft: '1rem' }}
          >
            Dispensar
          </Button>
        </ErrorState>
      )}

      {error.stats && (
        <ErrorState>
          <strong>Erro nas Estatísticas:</strong> {error.stats}
          <Button 
            size="small" 
            variant="secondary" 
            onClick={() => dispatch(clearErrors())}
            style={{ marginLeft: '1rem' }}
          >
            Dispensar
          </Button>
        </ErrorState>
      )}

      {/* Seção Principal */}
      <ContentCard>
        <SectionHeader>
          <SectionTitle>Publicações ({filteredPublications.length})</SectionTitle>
          <RefreshButton 
            onClick={handleRefresh}
            disabled={loading.publications}
            variant="secondary"
          >
            {loading.publications ? '🔄' : '↻'} Atualizar
          </RefreshButton>
        </SectionHeader>

        {/* Filtros */}
        <FilterContainer>
          {(['all', 'withMetadata', 'withoutMetadata', 'notExists', 'subProjects', 'sites'] as FilterType[]).map((filterType) => (
            <FilterButton
              key={filterType}
              active={filter === filterType}
              onClick={() => setFilter(filterType)}
            >
              {getFilterLabel(filterType)}
            </FilterButton>
          ))}
        </FilterContainer>

        {/* Lista de Publicações */}
        {loading.publications ? (
          <LoadingState>Carregando publicações...</LoadingState>
        ) : filteredPublications.length > 0 ? (
          filteredPublications.map((publication) => (
            <ProjectCard
              key={publication.name}
              publication={publication}
              onEdit={handleEditPublication}
              onReDeploy={handleReDeploy}
            />
          ))
        ) : publications.length === 0 ? (
          <EmptyState>
            <EmptyStateIcon>📦</EmptyStateIcon>
            <EmptyStateTitle>Nenhuma publicação encontrada</EmptyStateTitle>
            <EmptyStateText>
              Não há publicações configuradas no sistema.
              <br />
              Publicações são detectadas automaticamente através do IIS.
            </EmptyStateText>
            <Button onClick={handleRefresh} variant="primary">
              Verificar Novamente
            </Button>
          </EmptyState>
        ) : (
          <EmptyState>
            <EmptyStateIcon>🔍</EmptyStateIcon>
            <EmptyStateTitle>Nenhuma publicação corresponde ao filtro</EmptyStateTitle>
            <EmptyStateText>
              Tente ajustar os filtros para ver mais resultados.
            </EmptyStateText>
          </EmptyState>
        )}
      </ContentCard>

      {/* Modal de Re-Deploy */}
      {reDeployModal.publication && (
        <ReDeployModal
          publication={reDeployModal.publication}
          isOpen={reDeployModal.isOpen}
          onClose={handleReDeployModalClose}
          onSuccess={handleReDeploySuccess}
          onError={handleReDeployError}
        />
      )}

      {/* Modal de Configuração de Deploy */}
      {configureDeployModal.publication && (
        <ConfigureDeployModal
          publication={configureDeployModal.publication}
          isOpen={configureDeployModal.isOpen}
          onClose={handleConfigureDeployModalClose}
          onSuccess={handleConfigureDeploySuccess}
          onError={handleConfigureDeployError}
        />
      )}
    </ProtectedLayout>
  );
};

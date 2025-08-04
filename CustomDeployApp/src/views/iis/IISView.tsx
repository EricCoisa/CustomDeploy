import React, { useEffect, useState } from 'react';
import { ProtectedLayout, Button } from '../../components';
import {
  SiteFormModal,
  ApplicationFormModal,
  AppPoolFormModal,
  SiteCardComponent,
  AppPoolsListComponent,
  PermissionsStatus,
  ConfirmationModal
} from '../../components/iis';
import { useAppDispatch, useAppSelector } from '../../store';
import {
  fetchSites,
  fetchAppPools,
  fetchPermissions,
  fetchAdminStatus,
  requestAdminPrivileges,
  createSite,
  deleteSite,
  createApplication,
  deleteApplication,
  createAppPool,
  deleteAppPool,
  startSite,
  stopSite,
  startApplication,
  stopApplication,
  startAppPool,
  stopAppPool,
  clearErrors
} from '../../store/iis';
import type { 
  IISSite, 
  IISApplication, 
  IISAppPool,
  CreateSiteRequest,
  CreateApplicationRequest,
  CreateAppPoolRequest
} from '../../store/iis/types';
import styled from 'styled-components';

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

const TabContainer = styled.div`
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
  
  @media (max-width: 768px) {
    flex-direction: column;
  }
`;

const Tab = styled.button<{ active: boolean }>`
  padding: 0.75rem 1.5rem;
  border: 1px solid ${props => props.active ? '#3b82f6' : '#d1d5db'};
  background: ${props => props.active ? '#3b82f6' : 'white'};
  color: ${props => props.active ? 'white' : '#374151'};
  cursor: pointer;
  border-radius: 0.5rem;
  font-weight: ${props => props.active ? '600' : '400'};
  transition: all 0.2s ease;
  flex: 1;

  &:hover {
    background: ${props => props.active ? '#2563eb' : '#f9fafb'};
    border-color: ${props => props.active ? '#2563eb' : '#9ca3af'};
  }
  
  @media (max-width: 768px) {
    padding: 1rem;
    text-align: center;
  }
`;

export const IISView: React.FC = () => {
  const dispatch = useAppDispatch();
  const { 
    sites, 
    appPools, 
    loading, 
    error, 
    permissions, 
    isAdministrator 
  } = useAppSelector(state => state.iis);

  // Estados dos modais
  const [showSiteModal, setShowSiteModal] = useState(false);
  const [showAppModal, setShowAppModal] = useState(false);
  const [showAppPoolModal, setShowAppPoolModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  
  // Estado para controlar a aba ativa
  const [activeTab, setActiveTab] = useState<'sites' | 'appPools'>('sites');
  
  // Estados para edição/exclusão
  const [selectedSite, setSelectedSite] = useState<string>('');
  const [deleteType, setDeleteType] = useState<'site' | 'application' | 'appPool'>('site');
  const [deleteTarget, setDeleteTarget] = useState<{ name: string; appPath?: string }>({ name: '' });
  console.log('IISView loaded', sites);
  useEffect(() => {
    // Verificar permissões e status de admin ao carregar
    dispatch(fetchPermissions());
    dispatch(fetchAdminStatus());
    
    // Carregar dados do IIS
    dispatch(fetchSites());
    dispatch(fetchAppPools());
  }, [dispatch]);

  // Handlers para permissões
  const handleCheckPermissions = () => {
    dispatch(fetchPermissions());
    dispatch(fetchAdminStatus());
  };

  const handleRequestAdmin = () => {
    dispatch(requestAdminPrivileges());
  };

  // Handlers para sites
  const handleCreateSite = async (siteData: CreateSiteRequest) => {
    const result = await dispatch(createSite(siteData));
    
    // Se a criação foi bem-sucedida, recarregar a lista de sites
    if (result.meta.requestStatus === 'fulfilled') {
      dispatch(fetchSites());
    }
  };

  const handleEditSite = (site: IISSite) => {
    // TODO: Implementar modal de edição
    console.log('Edit site:', site);
  };

  const handleDeleteSite = (siteName: string) => {
    setDeleteType('site');
    setDeleteTarget({ name: siteName });
    setShowDeleteModal(true);
  };

  // Handlers para aplicações
  const handleAddApplication = (siteName: string) => {
    setSelectedSite(siteName);
    setShowAppModal(true);
  };

  const handleCreateApplication = async (appData: CreateApplicationRequest) => {
    const result = await dispatch(createApplication(appData));
    
    // Se a criação foi bem-sucedida, recarregar a lista de sites
    if (result.meta.requestStatus === 'fulfilled') {
      dispatch(fetchSites());
    }
  };

  const handleEditApplication = (siteName: string, appPath: string, application: IISApplication) => {
    // TODO: Implementar modal de edição
    console.log('Edit application:', { siteName, appPath, application });
  };

  const handleDeleteApplication = (siteName: string, appPath: string) => {
    setDeleteType('application');
    setDeleteTarget({ name: siteName, appPath });
    setShowDeleteModal(true);
  };

  // Handlers para app pools
  const handleCreateAppPool = async (poolData: CreateAppPoolRequest) => {
    const result = await dispatch(createAppPool(poolData));
    
    // Se a criação foi bem-sucedida, recarregar a lista de app pools e sites
    if (result.meta.requestStatus === 'fulfilled') {
      dispatch(fetchAppPools());
      dispatch(fetchSites()); // Atualizar sites também para refletir o novo pool disponível
    }
  };

  const handleEditAppPool = (appPool: IISAppPool) => {
    // TODO: Implementar modal de edição
    console.log('Edit app pool:', appPool);
  };

  const handleDeleteAppPool = (poolName: string) => {
    setDeleteType('appPool');
    setDeleteTarget({ name: poolName });
    setShowDeleteModal(true);
  };

  // Handlers para start/stop sites
  const handleStartSite = (siteName: string) => {
    console.log('Starting site:', siteName);
    dispatch(startSite(siteName));
  };

  const handleStopSite = (siteName: string) => {
    console.log('Stopping site:', siteName);
    dispatch(stopSite(siteName));
  };

  // Handlers para start/stop aplicações
  const handleStartApplication = (siteName: string, appPath: string) => {
    console.log('Starting application:', { siteName, appPath });
    dispatch(startApplication({ siteName, appPath }));
  };

  const handleStopApplication = (siteName: string, appPath: string) => {
    console.log('Stopping application:', { siteName, appPath });
    dispatch(stopApplication({ siteName, appPath }));
  };

  // Handlers para start/stop app pools
  const handleStartAppPool = (poolName: string) => {
    console.log('Starting app pool:', poolName);
    dispatch(startAppPool(poolName));
  };

  const handleStopAppPool = (poolName: string) => {
    dispatch(stopAppPool(poolName));
  };

  // Handler para confirmação de exclusão
  const handleConfirmDelete = async () => {
    let result;
    
    switch (deleteType) {
      case 'site':
        result = await dispatch(deleteSite(deleteTarget.name));
        if (result.meta.requestStatus === 'fulfilled') {
          dispatch(fetchSites());
        }
        break;
      case 'application':
        if (deleteTarget.appPath) {
          result = await dispatch(deleteApplication({ siteName: deleteTarget.name, appPath: deleteTarget.appPath }));
          if (result.meta.requestStatus === 'fulfilled') {
            dispatch(fetchSites());
          }
        }
        break;
      case 'appPool':
        result = await dispatch(deleteAppPool(deleteTarget.name));
        if (result.meta.requestStatus === 'fulfilled') {
          dispatch(fetchAppPools());
          dispatch(fetchSites()); // Atualizar sites também caso algum estivesse usando o pool deletado
        }
        break;
    }
    
    setShowDeleteModal(false);
    setDeleteTarget({ name: '' });
  };

  const getDeleteMessage = () => {
    switch (deleteType) {
      case 'site':
        return `Tem certeza que deseja excluir o site "${deleteTarget.name}"? Esta ação não pode ser desfeita.`;
      case 'application':
        return `Tem certeza que deseja excluir a aplicação "${deleteTarget.appPath}" do site "${deleteTarget.name}"?`;
      case 'appPool':
        return `Tem certeza que deseja excluir o Application Pool "${deleteTarget.name}"? Certifique-se de que não está em uso.`;
      default:
        return 'Tem certeza que deseja continuar?';
    }
  };

  // Obter lista de nomes dos app pools para os formulários
  const appPoolNames = appPools.map(pool => pool.name);

  return (
    <ProtectedLayout title="Gerenciamento IIS">
      {/* Status de Permissões */}
      <ContentCard>
        <PermissionsStatus
          permissions={permissions}
          isAdministrator={isAdministrator}
          onCheckPermissions={handleCheckPermissions}
          onRequestAdmin={handleRequestAdmin}
          loading={loading.sites}
        />
      </ContentCard>

      {/* Mensagens de Erro */}
      {error.sites && (
          <ErrorState>
            <strong>Erro nos Sites:</strong> {error.sites}
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

        {error.appPools && (
          <ErrorState>
            <strong>Erro nos App Pools:</strong> {error.appPools}
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

      {/* Tabs de Navegação */}
      <ContentCard>
        <TabContainer>
          <Tab 
            active={activeTab === 'sites'} 
            onClick={() => setActiveTab('sites')}
          >
            Sites IIS
          </Tab>
          <Tab 
            active={activeTab === 'appPools'} 
            onClick={() => setActiveTab('appPools')}
          >
            Application Pools
          </Tab>
        </TabContainer>

        {/* Conteúdo das Tabs */}
        {activeTab === 'sites' && (
          <div>
            <SectionHeader>
              <SectionTitle>Sites IIS ({sites.length})</SectionTitle>
              <Button onClick={() => setShowSiteModal(true)}>
                + Novo Site
              </Button>
            </SectionHeader>

            {loading.sites ? (
              <LoadingState>Carregando sites...</LoadingState>
            ) : sites.length > 0 ? (
              sites.map((site) => (
                <SiteCardComponent
                  key={site.name}
                  site={site}
                  onEditSite={handleEditSite}
                  onDeleteSite={handleDeleteSite}
                  onAddApplication={handleAddApplication}
                  onEditApplication={handleEditApplication}
                  onDeleteApplication={handleDeleteApplication}
                  onStartSite={handleStartSite}
                  onStopSite={handleStopSite}
                  onStartApplication={handleStartApplication}
                  onStopApplication={handleStopApplication}
                />
              ))
            ) : (
              <LoadingState>Nenhum site encontrado</LoadingState>
            )}
          </div>
        )}

        {activeTab === 'appPools' && (
          <AppPoolsListComponent
            appPools={appPools}
            onCreateAppPool={() => setShowAppPoolModal(true)}
            onEditAppPool={handleEditAppPool}
            onDeleteAppPool={handleDeleteAppPool}
            onStartAppPool={handleStartAppPool}
            onStopAppPool={handleStopAppPool}
            loading={loading.appPools}
          />
        )}
      </ContentCard>

        {/* Modais */}
        <SiteFormModal
          isOpen={showSiteModal}
          onClose={() => setShowSiteModal(false)}
          onSubmit={handleCreateSite}
          appPools={appPoolNames}
        />

        <ApplicationFormModal
          isOpen={showAppModal}
          onClose={() => setShowAppModal(false)}
          onSubmit={handleCreateApplication}
          siteName={selectedSite}
          appPools={appPoolNames}
        />

        <AppPoolFormModal
          isOpen={showAppPoolModal}
          onClose={() => setShowAppPoolModal(false)}
          onSubmit={handleCreateAppPool}
        />

        {/* Modal de Confirmação de Exclusão */}
        <ConfirmationModal
          isOpen={showDeleteModal}
          onClose={() => setShowDeleteModal(false)}
          onConfirm={handleConfirmDelete}
          title="Confirmar Exclusão"
          message={getDeleteMessage()}
          confirmText="Excluir"
          cancelText="Cancelar"
          variant="danger"
        />
    </ProtectedLayout>
  );
};

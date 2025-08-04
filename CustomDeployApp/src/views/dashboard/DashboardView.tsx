import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { ProtectedLayout, Modal, Button, FileBrowser, Card } from '../../components';
import { RecentDeploymentsTable } from './RecentDeploymentsTable';
import { useAppSelector } from '../../store';
import { useAppDispatch } from '../../store';
import { clearError, fetchDashboardData } from '../../store/dashboard';
import {
  DashboardContainer,
  WelcomeCard,
  DashboardContent,
  CardsGrid,
  Section,
  SectionTitle,
  SystemStatusContainer,
  StatusIndicator,
  ErrorMessage,
  RefreshButton,
} from './Styled';

export const DashboardView: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const hasInitialized = useRef(false);
  
  // Redux state
  const { 
    stats, 
    recentDeployments, 
    systemStatus, 
    isLoading, 
    error 
  } = useAppSelector(state => state.dashboard);

  // Local state para modais de exemplo
  const [showModal, setShowModal] = useState(false);
  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [showFileBrowser, setShowFileBrowser] = useState(false);
  const [selectedPath, setSelectedPath] = useState<string>('');

  // Carregar dados do dashboard ao montar o componente
  useEffect(() => {
    if (!hasInitialized.current) {
      const thunkAction = fetchDashboardData();
      dispatch(thunkAction);
      hasInitialized.current = true;
    }
  }, [dispatch]);

  const handleRefresh = () => {
    const thunkAction = fetchDashboardData();
    dispatch(thunkAction);
  };

  const handleConfirm = () => {
    alert('Ação confirmada!');
    setShowConfirmModal(false);
  };

  const handleFileSelect = (path: string) => {
    setSelectedPath(path);
    alert(`Arquivo/pasta selecionado: ${path}`);
  };

  const handleClearError = () => {
    dispatch(clearError());
  };

  return (
    <ProtectedLayout title="CustomDeploy Dashboard">
      <DashboardContainer>
        <WelcomeCard>
          <h2>🎉 Dashboard CustomDeploy</h2>
          <p>
            Visão geral do sistema de deploy. Acompanhe estatísticas, 
            deployments recentes e o status dos serviços.
          </p>
          
          {/* Status do Sistema */}
          <SystemStatusContainer>
            <StatusIndicator status={systemStatus.apiStatus}>
              API {systemStatus.apiStatus === 'online' ? 'Online' : 'Offline'}
            </StatusIndicator>
            <StatusIndicator status={systemStatus.iisStatus}>
              IIS {systemStatus.iisStatus === 'online' ? 'Online' : 
                   systemStatus.iisStatus === 'offline' ? 'Offline' : 'Desconhecido'}
            </StatusIndicator>
          </SystemStatusContainer>
          
          {/* Botão de atualizar */}
          <div style={{ marginTop: '1rem' }}>
            <RefreshButton 
              onClick={handleRefresh} 
              disabled={isLoading}
            >
              {isLoading ? '🔄 Atualizando...' : '🔄 Atualizar Dados'}
            </RefreshButton>
          </div>
          
          {/* Exemplos de componentes (manter para demonstração) */}
          <div style={{ 
            marginTop: '1.5rem', 
            display: 'flex', 
            gap: '1rem', 
            justifyContent: 'center', 
            flexWrap: 'wrap' 
          }}>
            <Button onClick={() => setShowModal(true)}>
              Modal Informativo
            </Button>
            <Button onClick={() => setShowConfirmModal(true)}>
              Modal de Confirmação
            </Button>
            <Button onClick={() => setShowFileBrowser(true)}>
              📁 Navegador de Arquivos
            </Button>
            <Button onClick={() => navigate('/iis')}>
              🖥️ Gerenciar IIS
            </Button>
            <Button onClick={() => navigate('/publications')}>
              📦 Gerenciar Publicações
            </Button>
          </div>
          
          {selectedPath && (
            <div style={{ 
              marginTop: '1rem', 
              padding: '0.75rem', 
              background: '#e0f2fe', 
              borderRadius: '0.5rem',
              fontSize: '0.875rem'
            }}>
              <strong>Último arquivo selecionado:</strong><br />
              <code style={{ wordBreak: 'break-all' }}>{selectedPath}</code>
            </div>
          )}
        </WelcomeCard>

        {/* Exibir erro se houver */}
        {error && (
          <ErrorMessage>
            <strong>Erro:</strong> {error}
            <Button 
              onClick={handleClearError}
              style={{ 
                marginLeft: '1rem', 
                background: 'transparent', 
                color: 'inherit',
                fontSize: '0.875rem',
                padding: '0.25rem 0.5rem'
              }}
            >
              ✕ Fechar
            </Button>
          </ErrorMessage>
        )}

        <DashboardContent>
          {/* Cards de Estatísticas */}
          <Section>
            <SectionTitle>📊 Estatísticas do Sistema</SectionTitle>
            <CardsGrid>
              <Card
                title="Total de Deployments"
                value={stats.totalDeployments}
                icon="🚀"
                color="#10b981"
                isLoading={isLoading}
                onClick={() => navigate('/publications')}
              />
              
              <Card
                title="Sites IIS"
                value={stats.totalSites}
                icon="🌐"
                color="#3b82f6"
                isLoading={isLoading}
                onClick={() => navigate('/iis')}
              />
              
              <Card
                title="Aplicações"
                value={stats.totalApplications}
                icon="📱"
                color="#8b5cf6"
                isLoading={isLoading}
                onClick={() => navigate('/iis')}
              />
              
              <Card
                title="Pools de Aplicação"
                value={stats.totalAppPools}
                icon="🔧"
                color="#f59e0b"
                isLoading={isLoading}
                onClick={() => navigate('/iis')}
              />
            </CardsGrid>
          </Section>

          {/* Tabela de Deployments Recentes */}
          <Section>
            <RecentDeploymentsTable
              deployments={recentDeployments}
              isLoading={isLoading}
            />
          </Section>

          {/* Informações do Sistema */}
          <Section>
            <div style={{ 
              textAlign: 'center', 
              marginTop: '2rem',
              padding: '1rem',
              backgroundColor: '#f3f4f6',
              borderRadius: '0.5rem'
            }}>
              <p><strong>Funcionalidades disponíveis:</strong></p>
              <ul style={{ 
                listStyle: 'none', 
                padding: 0,
                display: 'flex',
                flexWrap: 'wrap',
                gap: '1rem',
                justifyContent: 'center'
              }}>
                <li>📦 Gerenciamento de Publicações</li>
                <li>🖥️ Controle do IIS</li>
                <li>🚀 Deploy Automático</li>
                <li>📊 Monitoramento em Tempo Real</li>
              </ul>
            </div>
          </Section>
        </DashboardContent>

        {/* Modais de exemplo */}
        <Modal 
          isOpen={showModal} 
          onClose={() => setShowModal(false)} 
          title="Modal Informativo"
        >
          <p>Este é um exemplo de modal informativo com título.</p>
          <p>Você pode incluir qualquer conteúdo aqui:</p>
          <ul>
            <li>Textos</li>
            <li>Formulários</li>
            <li>Imagens</li>
            <li>Botões personalizados</li>
          </ul>
          <div style={{ marginTop: '1rem', textAlign: 'right' }}>
            <Button onClick={() => setShowModal(false)}>
              Entendi
            </Button>
          </div>
        </Modal>

        <Modal 
          isOpen={showConfirmModal} 
          onClose={() => setShowConfirmModal(false)} 
          title="Confirmar Ação"
        >
          <p>Tem certeza que deseja prosseguir com esta ação?</p>
          <p><strong>Esta operação não pode ser desfeita.</strong></p>
          
          <div style={{ 
            marginTop: '1.5rem', 
            display: 'flex', 
            gap: '1rem', 
            justifyContent: 'flex-end' 
          }}>
            <Button 
              onClick={() => setShowConfirmModal(false)}
              style={{ background: '#6b7280' }}
            >
              Cancelar
            </Button>
            <Button 
              onClick={handleConfirm}
              style={{ background: '#ef4444' }}
            >
              Confirmar
            </Button>
          </div>
        </Modal>

        {/* FileBrowser de exemplo */}
        <FileBrowser
          isOpen={showFileBrowser}
          onClose={() => setShowFileBrowser(false)}
          onSelect={handleFileSelect}
          selectType="both"
          initialPath="C:/"
        />
      </DashboardContainer>
    </ProtectedLayout>
  );
};

import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { ProtectedLayout, Modal, Button, FileBrowser } from '../../components';
import { RecentDeploymentsCard } from './RecentDeploymentsCard';
import { RightSidebar } from './RightSidebar';
import { useAppSelector } from '../../store';
import { useAppDispatch } from '../../store';
import { clearError, fetchDashboardData } from '../../store/dashboard';
import type { Publication } from '../../store/publications/types';
import {
  DashboardContainer,
  DashboardContent,
  ErrorMessage,
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
    error,
    lastUpdated
  } = useAppSelector(state => state.dashboard);
  
  const { isAutoLogin } = useAppSelector(state => state.login);

  // Local state para modais de exemplo
  const [showModal, setShowModal] = useState(false);
  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [showFileBrowser, setShowFileBrowser] = useState(false);

  // Carregar dados do dashboard ao montar o componente
  useEffect(() => {
    // Verificar se temos dados válidos persistidos
    const hasValidPersistedData = () => {
      // Se foi auto login, sempre forçar reload dos dados
      if (isAutoLogin) {
        console.log('🔄 Auto login detectado, forçando reload dos dados');
        return false;
      }
      
      // Se temos lastUpdated, significa que os dados foram carregados anteriormente
      if (lastUpdated) {
        const lastUpdate = new Date(lastUpdated);
        const now = new Date();
        const diffMinutes = (now.getTime() - lastUpdate.getTime()) / (1000 * 60);
        
        // Se os dados são recentes (menos de 10 minutos), usar dados persistidos
        if (diffMinutes < 10) {
          console.log(`✅ Usando dados persistidos (${diffMinutes.toFixed(1)} min atrás)`);
          return true;
        }
      }
      return false;
    };

    // Só carregar se não temos dados válidos persistidos OU se nunca foi inicializado
    if (!hasInitialized.current && !hasValidPersistedData()) {
      console.log('🔄 Carregando dados do dashboard pela primeira vez...');
      const thunkAction = fetchDashboardData();
      dispatch(thunkAction);
      hasInitialized.current = true;
    } else if (hasValidPersistedData()) {
      console.log('✅ Utilizando dados persistidos válidos');
      hasInitialized.current = true;
    }
  }, [dispatch, lastUpdated, isAutoLogin]);

  // Efeito separado para verificar dados persistidos
  useEffect(() => {
    if (hasInitialized.current && systemStatus) {
      if (systemStatus.apiStatus !== 'offline' || systemStatus.iisStatus !== 'unknown') {
        console.log('✅ Status do sistema carregado do cache');
      }
    }
  }, [systemStatus]);

  const handleRefresh = () => {
    const thunkAction = fetchDashboardData();
    dispatch(thunkAction);
  };

  const handleRedeploy = async (deployment: Publication) => {
    try {
      // Aqui você pode chamar uma action ou API para re-fazer o deploy
      console.log('🔄 Iniciando re-deploy de:', deployment.name);
      
      // Exemplo de implementação:
      // await dispatch(redeployPublication(deployment.name));
      
      alert(`🚀 Re-deploy iniciado para: ${deployment.name}`);
    } catch (error) {
      console.error('❌ Erro no re-deploy:', error);
      alert('❌ Erro ao iniciar o re-deploy');
    }
  };

  const handleConfirm = () => {
    alert('Ação confirmada!');
    setShowConfirmModal(false);
  };

  const handleFileSelect = (path: string) => {
    alert(`Arquivo/pasta selecionado: ${path}`);
  };

  const handleClearError = () => {
    dispatch(clearError());
  };

  return (
    <ProtectedLayout title="CustomDeploy Dashboard">
      <DashboardContainer>
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
          {/* Card Principal de Deployments Recentes - Coluna Esquerda (70%) */}
          <RecentDeploymentsCard
            deployments={recentDeployments}
            isLoading={isLoading}
            onRedeploy={handleRedeploy}
          />

          {/* Sidebar Direita com Stats e Status - Coluna Direita (30%) */}
          <RightSidebar
            stats={stats}
            systemStatus={systemStatus}
            isLoading={isLoading}
            onDeploymentsClick={() => navigate('/publications')}
            onSitesClick={() => navigate('/iis')}
            onApplicationsClick={() => navigate('/iis')}
            onAppPoolsClick={() => navigate('/iis')}
          />
        </DashboardContent>

        {/* Modais essenciais */}
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

        {/* FileBrowser */}
        <FileBrowser
          isOpen={showFileBrowser}
          onClose={() => setShowFileBrowser(false)}
          onSelect={handleFileSelect}
          selectType="both"
          initialPath="C:/"
        />

        {/* Modal informativo (menos usado) */}
        {showModal && (
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
        )}
      </DashboardContainer>
    </ProtectedLayout>
  );
};

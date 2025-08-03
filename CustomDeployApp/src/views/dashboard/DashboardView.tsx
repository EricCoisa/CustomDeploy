import React, { useState } from 'react';
import { ProtectedLayout, Modal, Button } from '../../components';
import {
  WelcomeCard,
  StatsGrid,
  StatCard,
  StatNumber,
  StatLabel,
} from './Styled';

export const DashboardView: React.FC = () => {
  const [showModal, setShowModal] = useState(false);
  const [showConfirmModal, setShowConfirmModal] = useState(false);

  const handleConfirm = () => {
    alert('Ação confirmada!');
    setShowConfirmModal(false);
  };

  return (
    <ProtectedLayout title="CustomDeploy Dashboard">
      <WelcomeCard>
        <h2>🎉 Login realizado com sucesso!</h2>
        <p>
          Você está agora no dashboard do CustomDeploy. 
          Esta é uma implementação inicial que será expandida com funcionalidades de deploy.
        </p>
        
        {/* Exemplos de uso do Modal */}
        <div style={{ marginTop: '1.5rem', display: 'flex', gap: '1rem', justifyContent: 'center' }}>
          <Button onClick={() => setShowModal(true)}>
            Abrir Modal Informativo
          </Button>
          <Button onClick={() => setShowConfirmModal(true)}>
            Abrir Modal de Confirmação
          </Button>
        </div>
      </WelcomeCard>

        <StatsGrid>
          <StatCard>
            <StatNumber>0</StatNumber>
            <StatLabel>Deploys Realizados</StatLabel>
          </StatCard>
          
          <StatCard>
            <StatNumber>0</StatNumber>
            <StatLabel>Projetos Ativos</StatLabel>
          </StatCard>
          
          <StatCard>
            <StatNumber>1</StatNumber>
            <StatLabel>Usuários Online</StatLabel>
          </StatCard>
          
          <StatCard>
            <StatNumber>100%</StatNumber>
            <StatLabel>Sistema Online</StatLabel>
          </StatCard>
        </StatsGrid>

        <div style={{ 
          textAlign: 'center', 
          marginTop: '2rem',
          padding: '1rem',
          backgroundColor: '#f3f4f6',
          borderRadius: '0.5rem'
        }}>
          <p><strong>Próximas funcionalidades:</strong></p>
          <ul style={{ 
            listStyle: 'none', 
            padding: 0,
            display: 'flex',
            flexWrap: 'wrap',
            gap: '1rem',
            justifyContent: 'center'
          }}>
            <li>📦 Gerenciamento de Projetos</li>
            <li>🚀 Deploy Automático</li>
            <li>📊 Relatórios</li>
            <li>⚙️ Configurações</li>
          </ul>
        </div>

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
    </ProtectedLayout>
  );
};

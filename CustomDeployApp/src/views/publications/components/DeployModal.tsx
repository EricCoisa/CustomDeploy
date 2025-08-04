import React from 'react';
import { Modal, DeployForm } from '../../../components';
import type { Publication } from '../../../store/publications/types';

interface DeployModalProps {
  isOpen: boolean;
  onClose: () => void;
  publication?: Publication;
}

export const DeployModal: React.FC<DeployModalProps> = ({
  isOpen,
  onClose,
  publication
}) => {
  const handleDeploySuccess = (result: Record<string, unknown>) => {
    console.log('Deploy successful for publication:', publication?.name, result);
    // Fechar modal após sucesso
    onClose();
    // Aqui você pode adicionar uma notificação de sucesso
  };

  const handleDeployError = (error: string) => {
    console.error('Deploy error for publication:', publication?.name, error);
    // Manter modal aberto para permitir correções
  };

  // Dados iniciais baseados na publicação
  const initialData = publication ? {
    siteName: publication.siteName || '',
    applicationPath: publication.isSubProject ? publication.name.split('/').slice(1).join('/') : '',
    repoUrl: publication.repoUrl || publication.repository || '',
    branch: publication.branch || 'main',
    buildCommand: publication.buildCommand || 'npm install && npm run build',
    buildOutput: publication.buildOutput || 'dist',
  } : undefined;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`Deploy: ${publication?.name || 'Nova Aplicação'}`}
    >
      <DeployForm
        title={`Executar Deploy - ${publication?.name || 'Nova Aplicação'}`}
        onSuccess={handleDeploySuccess}
        onError={handleDeployError}
        initialData={initialData}
        showPreview={true}
      />
    </Modal>
  );
};

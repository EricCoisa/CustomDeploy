import React from 'react';

import { Modal } from '../../../components/Modal';
import { DeployForm } from '../../../components/DeployForm';
import type { Publication } from '../../../store/publications/types';

interface ConfigureDeployModalProps {
  isOpen: boolean;
  publication?: Publication;
  onClose: () => void;
  onSuccess: (result: Record<string, unknown>) => void;
  onError: (error: string) => void;
}

export const ConfigureDeployModal: React.FC<ConfigureDeployModalProps> = ({
  isOpen,
  publication,
  onClose,
  onSuccess,
  onError,
}) => {
  // Mapear os dados da publicação para o formato esperado pelo DeployForm
  const initialData = publication ? {
    repoUrl: publication.repoUrl || publication.repository,
    branch: publication.branch,
    buildCommand: publication.buildCommand || [{ comando: '', terminalId: '1' }],
    buildOutput: publication.buildOutput,
    siteName: publication.name,
    targetPath: publication.targetPath
  } : undefined;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={publication ? `Configurar Deploy: ${publication.name}` : 'Novo Deploy'}
    >

        <DeployForm
          title={publication ? 'Configurar Deploy' : 'Novo Deploy'}
          initialData={initialData}
          onSuccess={onSuccess}
          onError={onError}
        />
  
    </Modal>
  );
};

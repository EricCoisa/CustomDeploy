import React from 'react';
import styled from 'styled-components';
import { DeployForm } from '../../../components';
import type { Publication } from '../../../store/publications/types';

interface ReDeployModalProps {
  publication: Publication;
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: (result: Record<string, unknown>) => void;
  onError?: (error: string) => void;
}

const ModalOverlay = styled.div<{ isOpen: boolean }>`
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  display: ${props => props.isOpen ? 'flex' : 'none'};
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
`;

const ModalContent = styled.div`
  background: white;
  border-radius: 1rem;
  max-width: 800px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: modalSlideIn 0.3s ease-out;

  @keyframes modalSlideIn {
    from {
      opacity: 0;
      transform: translateY(-50px) scale(0.95);
    }
    to {
      opacity: 1;
      transform: translateY(0) scale(1);
    }
  }
`;

const ModalHeader = styled.div`
  padding: 1.5rem 1.5rem 0 1.5rem;
  border-bottom: 1px solid #e5e7eb;
  margin-bottom: 1rem;
`;

const ModalTitle = styled.h2`
  margin: 0 0 0.5rem 0;
  color: #1f2937;
  font-size: 1.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
`;

const ModalSubtitle = styled.p`
  margin: 0 0 1rem 0;
  color: #6b7280;
  font-size: 0.875rem;
`;

const CloseButton = styled.button`
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #6b7280;
  padding: 0.5rem;
  border-radius: 0.5rem;
  transition: all 0.2s ease;

  &:hover {
    background: #f3f4f6;
    color: #374151;
  }
`;

const ModalBody = styled.div`
  padding: 0 1.5rem 1.5rem 1.5rem;
`;

export const ReDeployModal: React.FC<ReDeployModalProps> = ({
  publication,
  isOpen,
  onClose,
  onSuccess,
  onError
}) => {
  const handleSuccess = (result: Record<string, unknown>) => {
    onSuccess?.(result);
    onClose();
  };

  const handleError = (error: string) => {
    onError?.(error);
  };

  const handleOverlayClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  const getInitialData = () => {
    return {
      siteName: publication.siteName || publication.name,
      subPath: publication.subApplication || '',
      repoUrl: publication.repository || publication.repoUrl || '',
      branch: publication.branch || 'main',
      buildCommand: publication.buildCommand || 'npm install && npm run build',
      buildOutput: publication.buildOutput || 'dist'
    };
  };

  return (
    <ModalOverlay isOpen={isOpen} onClick={handleOverlayClick}>
      <ModalContent>
        <CloseButton onClick={onClose} aria-label="Fechar modal">
          ×
        </CloseButton>
        
        <ModalHeader>
          <ModalTitle>
            🚀 Re-Deploy: {publication.name}
          </ModalTitle>
          <ModalSubtitle>
            Atualize o deploy desta publicação. Os dados foram preenchidos automaticamente 
            com base nos metadados existentes, mas você pode alterá-los conforme necessário.
          </ModalSubtitle>
        </ModalHeader>

        <ModalBody>
          <DeployForm
            title="Configurações do Re-Deploy"
            onSuccess={handleSuccess}
            onError={handleError}
            initialData={getInitialData()}
            showPreview={false}
          />
        </ModalBody>
      </ModalContent>
    </ModalOverlay>
  );
};

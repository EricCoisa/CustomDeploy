import styled, { keyframes } from 'styled-components';

// Animações
const fadeIn = keyframes`
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
`;

const slideIn = keyframes`
  from {
    opacity: 0;
    transform: translate(-50%, -50%) scale(0.9);
  }
  to {
    opacity: 1;
    transform: translate(-50%, -50%) scale(1);
  }
`;

// Overlay que escurece o fundo
export const ModalOverlay = styled.div<{ isOpen: boolean }>`
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  
  /* Animações */
  animation: ${fadeIn} 0.2s ease-out;
  
  /* Suporte a animação de saída (opcional, para implementação futura) */
  transition: opacity 0.2s ease-out;
`;

// Container do modal
export const ModalContainer = styled.div<{ isOpen: boolean }>`
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  max-width: 90vw;
  max-height: 90vh;
  min-width: 320px;
  z-index: 1001;
  
  /* Animação de entrada */
  animation: ${slideIn} 0.2s ease-out;
`;

// Conteúdo principal do modal
export const ModalContent = styled.div`
  background: white;
  border-radius: 1rem;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  overflow: hidden;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
`;

// Header do modal
export const ModalHeader = styled.div`
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.5rem 2rem 1rem 2rem;
  border-bottom: 1px solid #e5e7eb;
  background: #f9fafb;
`;

// Título do modal
export const ModalTitle = styled.h2`
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
  flex: 1;
`;

// Botão de fechar
export const CloseButton = styled.button`
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border: none;
  background: transparent;
  color: #6b7280;
  cursor: pointer;
  border-radius: 0.5rem;
  transition: all 0.2s;
  
  &:hover {
    background: #e5e7eb;
    color: #374151;
  }
  
  &:focus {
    outline: none;
    box-shadow: 0 0 0 2px #3b82f6;
  }
  
  svg {
    width: 1.25rem;
    height: 1.25rem;
  }
`;

// Corpo do modal
export const ModalBody = styled.div<{ hasHeader: boolean }>`
  padding: ${props => props.hasHeader ? '1.5rem 2rem 2rem 2rem' : '2rem'};
  overflow-y: auto;
  flex: 1;
  
  /* Scrollbar personalizada */
  &::-webkit-scrollbar {
    width: 6px;
  }
  
  &::-webkit-scrollbar-track {
    background: #f1f5f9;
    border-radius: 3px;
  }
  
  &::-webkit-scrollbar-thumb {
    background: #cbd5e1;
    border-radius: 3px;
  }
  
  &::-webkit-scrollbar-thumb:hover {
    background: #94a3b8;
  }
`;

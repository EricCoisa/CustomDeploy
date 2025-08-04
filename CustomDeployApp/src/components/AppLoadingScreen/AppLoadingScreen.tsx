import React from 'react';
import styled, { keyframes } from 'styled-components';

const spin = keyframes`
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
`;

const LoadingContainer = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
`;

const LoadingSpinner = styled.div`
  width: 50px;
  height: 50px;
  border: 4px solid rgba(255, 255, 255, 0.3);
  border-top: 4px solid white;
  border-radius: 50%;
  animation: ${spin} 1s linear infinite;
  margin-bottom: 1rem;
`;

const LoadingText = styled.h2`
  margin: 0;
  font-size: 1.5rem;
  font-weight: 300;
`;

const LoadingSubtext = styled.p`
  margin: 0.5rem 0 0 0;
  font-size: 1rem;
  opacity: 0.8;
`;

export const AppLoadingScreen: React.FC = () => {
  return (
    <LoadingContainer>
      <LoadingSpinner />
      <LoadingText>CustomDeploy</LoadingText>
      <LoadingSubtext>Iniciando aplicação...</LoadingSubtext>
    </LoadingContainer>
  );
};

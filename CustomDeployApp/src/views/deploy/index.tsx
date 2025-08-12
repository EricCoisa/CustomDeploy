import React from 'react';
import { ProtectedLayout, DeployForm } from '../../components';
import { toast } from 'react-toastify';
import styled from 'styled-components';

const ContentWrapper = styled.div`
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 2rem;
  max-width: 800px;
  margin: 0 auto;
  width: 100%;
  box-sizing: border-box;
`;

const Description = styled.div`
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 1.5rem;
  border-radius: 1rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
  width: 100%;
  text-align: center;
`;

const DescriptionTitle = styled.h2`
  margin: 0 0 1rem 0;
  color: #1f2937;
  font-size: 1.5rem;
`;

const DescriptionText = styled.p`
  margin: 0;
  color: #6b7280;
  line-height: 1.6;
`;

export const DeployView: React.FC = () => {
  const handleDeploySuccess = (result: Record<string, unknown>) => {
    console.log('Deploy successful:', result);
    toast.success(`Deploy executado com sucesso! ${result.message || ''}`);
  };

  const handleDeployError = (error: string) => {
    console.error('Deploy error:', error);
    toast.error(`Erro no deploy: ${error}`);
  };

  return (
    <ProtectedLayout 
      title="Deploy de Aplicações"
      subtitle="Execute deploys automatizados para seus projetos"
    >
      <ContentWrapper>
        <Description>
          <DescriptionTitle>🚀 Sistema de Deploy Automatizado</DescriptionTitle>
          <DescriptionText>
            Utilize este formulário para executar deploys automatizados de suas aplicações.
            O sistema irá clonar o repositório, executar o build e implantar os arquivos
            no site IIS especificado.
          </DescriptionText>
        </Description>

        <DeployForm
          title="Novo Deploy"
          onSuccess={handleDeploySuccess}
          onError={handleDeployError}
          showPreview={true}
          initialData={{
            branch: 'main',
            BuildCommands: ['npm install && npm run build'],
            buildOutput: 'dist'
          }}
        />
      </ContentWrapper>
    </ProtectedLayout>
  );
};

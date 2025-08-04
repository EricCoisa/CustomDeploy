import React from 'react';
import { Header } from '../Header';
import styled from 'styled-components';

const LayoutContainer = styled.div`
  min-height: 100vh;
  height: 100vh;
  width: 100%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 1rem;
  box-sizing: border-box;
  overflow-y: auto;
`;

const ContentWrapper = styled.main`
  max-width: 1400px;
  width: 100%;
  margin: 0 auto;
  height: calc(100vh - 2rem);
  display: flex;
  flex-direction: column;
`;

interface ProtectedLayoutProps {
  children: React.ReactNode;
  title?: string;
  subtitle?: string;
}

export const ProtectedLayout: React.FC<ProtectedLayoutProps> = ({
  children,
  title = "CustomDeploy",
  subtitle = "Sistema de Deploy Automatizado",
}) => {
  return (
    <LayoutContainer>
      <Header title={title} subtitle={subtitle} />
      <ContentWrapper>
        {children}
      </ContentWrapper>
    </LayoutContainer>
  );
};

import React from 'react';
import { Header } from '../Header';
import styled from 'styled-components';

const LayoutContainer = styled.div`
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem;
`;

const ContentWrapper = styled.main`
  max-width: 1200px;
  margin: 0 auto;
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

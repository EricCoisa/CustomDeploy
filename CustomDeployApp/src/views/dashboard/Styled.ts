import styled from 'styled-components';

export const DashboardContainer = styled.div`
  padding: 0;
  width: 100%;
  margin: 0;
  background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
  box-sizing: border-box;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  
  @media (max-width: 768px) {
    height: calc(100vh - 50px); /* Header menor no mobile */
  }
`;

export const WelcomeCard = styled.div`
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border: 1px solid #e2e8f0;
  padding: 1rem;
  margin: 1rem;
  border-radius: 1rem;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05), 0 1px 2px rgba(0, 0, 0, 0.1);
  text-align: center;
  flex-shrink: 0;
  
  h2 {
    color: #1e293b;
    margin: 0 0 0.5rem 0;
    font-size: 1.375rem;
    font-weight: 700;
  }
  
  p {
    color: #64748b;
    line-height: 1.4;
    margin: 0;
    font-size: 0.95rem;
  }
  
  @media (max-width: 768px) {
    margin: 0.75rem;
    padding: 0.875rem;
    
    h2 {
      font-size: 1.25rem;
    }
    
    p {
      font-size: 0.9rem;
    }
  }
  
  @media (max-width: 480px) {
    margin: 0.5rem;
    padding: 0.75rem;
    
    h2 {
      font-size: 1.125rem;
    }
    
    p {
      font-size: 0.85rem;
    }
  }
`;

export const StatsGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
  
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
`;

export const StatCard = styled.div`
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 2rem;
  border-radius: 1rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  text-align: center;
  transition: transform 0.2s;
  
  &:hover {
    transform: translateY(-2px);
  }
  
  @media (max-width: 768px) {
    padding: 1.5rem;
  }
`;

export const StatNumber = styled.div`
  font-size: 2.5rem;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 0.5rem;
  background: linear-gradient(135deg, #667eea, #764ba2);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  
  @media (max-width: 768px) {
    font-size: 2rem;
  }
`;

export const StatLabel = styled.div`
  font-size: 0.95rem;
  color: #6b7280;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  
  @media (max-width: 768px) {
    font-size: 0.875rem;
  }
`;

export const DashboardContent = styled.div`
  flex: 1;
  display: grid;
  grid-template-columns: 2.2fr 1fr; /* Melhor aproveitamento - mais espaço para deployments */
  gap: 1.25rem;
  width: 100%;
  overflow: hidden;
  min-height: 0;
  padding: 1rem;
  box-sizing: border-box;

  @media (max-width: 1200px) {
    grid-template-columns: 2fr 1fr;
    gap: 1rem;
  }

  @media (max-width: 1024px) {
    grid-template-columns: 1.8fr 1fr;
    gap: 0.875rem;
  }
  
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    gap: 0.75rem;
    padding: 0.75rem;
  }
  
  @media (max-width: 480px) {
    gap: 0.5rem;
    padding: 0.5rem;
  }
`;

export const CardsGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  
  @media (max-width: 768px) {
    grid-template-columns: repeat(2, 1fr);
    gap: 1rem;
  }
  
  @media (max-width: 480px) {
    grid-template-columns: 1fr;
  }
`;

export const Section = styled.section`
  margin-bottom: 2rem;
  
  &:last-child {
    margin-bottom: 0;
  }
`;

export const SectionTitle = styled.h2`
  font-size: 1.5rem;
  font-weight: 600;
  color: #111827;
  margin: 0 0 1.5rem 0;
  
  @media (max-width: 768px) {
    font-size: 1.25rem;
    margin-bottom: 1rem;
  }
`;

export const SystemStatusContainer = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 0.75rem;
  margin-top: 1rem;
  max-width: 100%;
  
  @media (max-width: 768px) {
    grid-template-columns: repeat(2, 1fr);
    gap: 0.5rem;
  }
  
  @media (max-width: 480px) {
    grid-template-columns: 1fr;
    gap: 0.5rem;
  }
`;

export const StatusIndicator = styled.div<{ status: 'online' | 'offline' | 'unknown' }>`
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  text-align: center;
  min-width: 0;
  word-break: break-word;
  
  ${props => {
    switch (props.status) {
      case 'online':
        return `
          background-color: #dcfce7;
          color: #166534;
        `;
      case 'offline':
        return `
          background-color: #fee2e2;
          color: #991b1b;
        `;
      case 'unknown':
        return `
          background-color: #fef3c7;
          color: #92400e;
        `;
      default:
        return `
          background-color: #f3f4f6;
          color: #6b7280;
        `;
    }
  }}
  
  &::before {
    content: '';
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background-color: currentColor;
    flex-shrink: 0;
  }
  
  @media (max-width: 480px) {
    font-size: 0.75rem;
    padding: 0.375rem 0.5rem;
    
    &::before {
      width: 6px;
      height: 6px;
    }
  }
`;

export const ErrorMessage = styled.div`
  background-color: #fee2e2;
  color: #991b1b;
  padding: 0.875rem 1rem;
  border-radius: 0.5rem;
  margin: 0.5rem 1rem;
  border: 1px solid #fecaca;
  flex-shrink: 0;
  
  strong {
    font-weight: 600;
  }
  
  @media (max-width: 768px) {
    margin: 0.5rem 0.75rem;
    padding: 0.75rem;
  }
  
  @media (max-width: 480px) {
    margin: 0.5rem;
    padding: 0.625rem;
  }
`;

export const RefreshButton = styled.button`
  background: linear-gradient(135deg, #6366f1, #8b5cf6);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 0.75rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 2px 4px rgba(99, 102, 241, 0.2);
  
  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(99, 102, 241, 0.3);
  }
  
  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none;
  }
`;

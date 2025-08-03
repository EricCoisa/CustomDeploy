import styled from 'styled-components';

// Styled Components para LoginView
export const LoginContainer = styled.div`
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 1rem;
`;

export const LoginCard = styled.div`
  background: white;
  border-radius: 1rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  padding: 2rem;
  width: 100%;
  max-width: 450px;
`;

export const Logo = styled.div`
  text-align: center;
  margin-bottom: 2rem;
`;

export const LogoIcon = styled.div`
  width: 4rem;
  height: 4rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 1rem;
  margin: 0 auto 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 1.5rem;
  font-weight: bold;
`;

export const LogoText = styled.h1`
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
`;

export const Subtitle = styled.p`
  color: #6b7280;
  text-align: center;
  margin: 0.5rem 0 0 0;
  font-size: 0.875rem;
`;

export const DemoCredentials = styled.div`
  background-color: #f3f4f6;
  border-radius: 0.5rem;
  padding: 1rem;
  margin-top: 1.5rem;
  font-size: 0.875rem;
  color: #4b5563;
`;

export const DemoTitle = styled.div`
  font-weight: 600;
  margin-bottom: 0.5rem;
`;

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { LoginForm } from './components/LoginForm';
import { useAppDispatch, useAppSelector } from '../../store';
import { loginUser } from '../../store/login/actions';
import { type LoginCredentials } from '../../store/login/types';
import {
  LoginContainer,
  LoginCard,
  Logo,
  LogoIcon,
  LogoText,
  Subtitle,
  DemoCredentials,
  DemoTitle,
} from './Styled';

// Componente LoginView
export const LoginView: React.FC = () => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { isLoading, error, isAuthenticated } = useAppSelector(state => state.login);

  // Redirecionar se já estiver autenticado (AuthInitializer já fez a verificação)
  useEffect(() => {
    if (isAuthenticated) {
      console.log('✅ Usuário autenticado! Redirecionando para dashboard...');
      navigate('/dashboard');
    }
  }, [isAuthenticated, navigate]);

  // Handler para submit do login
  const handleLogin = (credentials: LoginCredentials) => {
    dispatch(loginUser(credentials));
  };

  return (
    <LoginContainer>
      <LoginCard>
        <Logo>
          <LogoIcon>CD</LogoIcon>
          <LogoText>CustomDeploy</LogoText>
          <Subtitle>Sistema de Deploy Automatizado</Subtitle>
        </Logo>

        <LoginForm
          onSubmit={handleLogin}
          isLoading={isLoading}
          error={error}
        />

        <DemoCredentials>
          <DemoTitle>💡 Credenciais para teste:</DemoTitle>
          <div><strong>Username:</strong> admin</div>
          <div><strong>Senha:</strong> password</div>
        </DemoCredentials>
      </LoginCard>
    </LoginContainer>
  );
};

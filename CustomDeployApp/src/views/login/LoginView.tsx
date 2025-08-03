import React, { useEffect } from 'react';
import { LoginForm } from './components/LoginForm';
import { useAppDispatch, useAppSelector } from '../../store';
import { loginUser, checkAuthState } from '../../store/login/actions';
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
  const { isLoading, error, isAuthenticated } = useAppSelector(state => state.login);

  // Verificar se usuário já está logado ao carregar a página
  useEffect(() => {
    dispatch(checkAuthState());
  }, [dispatch]);

  // Redirecionar se já estiver autenticado
  useEffect(() => {
    if (isAuthenticated) {
      // TODO: Implementar navegação para dashboard quando as rotas estiverem prontas
      console.log('Usuário autenticado! Redirecionar para dashboard...');
    }
  }, [isAuthenticated]);

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

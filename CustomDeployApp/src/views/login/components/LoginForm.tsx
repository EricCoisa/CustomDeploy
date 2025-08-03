import React, { useState } from 'react';
import { Input } from '../../../components/Input';
import { Button } from '../../../components/Button';
import {
  FormContainer,
  FormTitle,
  ErrorMessage,
  ForgotPasswordLink,
} from './Styled';

// Interfaces
interface LoginFormData {
  username: string; // Mudou de email para username
  password: string;
}

interface LoginFormProps {
  onSubmit: (data: LoginFormData) => void;
  isLoading?: boolean;
  error?: string | null;
}

// Componente LoginForm
export const LoginForm: React.FC<LoginFormProps> = ({
  onSubmit,
  isLoading = false,
  error = null,
}) => {
  const [formData, setFormData] = useState<LoginFormData>({
    username: '',
    password: '',
  });

  const [fieldErrors, setFieldErrors] = useState<Partial<LoginFormData>>({});

  // Validação dos campos
  const validateForm = (): boolean => {
    const errors: Partial<LoginFormData> = {};
    let isValid = true;

    // Validar username
    if (!formData.username) {
      errors.username = 'Username é obrigatório';
      isValid = false;
    } else if (formData.username.length < 3) {
      errors.username = 'Username deve ter pelo menos 3 caracteres';
      isValid = false;
    }

    // Validar password
    if (!formData.password) {
      errors.password = 'Senha é obrigatória';
      isValid = false;
    } else if (formData.password.length < 6) {
      errors.password = 'Senha deve ter pelo menos 6 caracteres';
      isValid = false;
    }

    setFieldErrors(errors);
    return isValid;
  };

  // Handler para mudanças nos campos
  const handleInputChange = (field: keyof LoginFormData) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    setFormData(prev => ({
      ...prev,
      [field]: e.target.value,
    }));

    // Limpar erro do campo quando o usuário começar a digitar
    if (fieldErrors[field]) {
      setFieldErrors(prev => ({
        ...prev,
        [field]: undefined,
      }));
    }
  };

  // Handler para submit do formulário
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (validateForm()) {
      onSubmit(formData);
    }
  };

  return (
    <FormContainer onSubmit={handleSubmit}>
      <FormTitle>Entrar</FormTitle>

      {error && (
        <ErrorMessage>
          {error}
        </ErrorMessage>
      )}

      <Input
        label="Username"
        type="text"
        placeholder="Digite seu username"
        value={formData.username}
        onChange={handleInputChange('username')}
        error={fieldErrors.username}
        autoComplete="username"
        disabled={isLoading}
      />

      <Input
        label="Senha"
        type="password"
        placeholder="Digite sua senha"
        value={formData.password}
        onChange={handleInputChange('password')}
        error={fieldErrors.password}
        autoComplete="current-password"
        disabled={isLoading}
      />

      <Button
        type="submit"
        fullWidth
        isLoading={isLoading}
        disabled={isLoading}
      >
        {isLoading ? 'Entrando...' : 'Entrar'}
      </Button>

      <ForgotPasswordLink href="#" onClick={(e) => e.preventDefault()}>
        Esqueceu sua senha?
      </ForgotPasswordLink>
    </FormContainer>
  );
};

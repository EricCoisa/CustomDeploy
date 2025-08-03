import React, { type InputHTMLAttributes } from 'react';
import {
  StyledInputContainer,
  StyledLabel,
  StyledInput,
  StyledErrorMessage,
} from './Input.Styled';

// Interface para as props
interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
}

// Componente Input
export const Input: React.FC<InputProps> = ({
  label,
  error,
  id,
  ...props
}) => {
  const inputId = id || `input-${Math.random().toString(36).substr(2, 9)}`;

  return (
    <StyledInputContainer>
      {label && (
        <StyledLabel htmlFor={inputId}>
          {label}
        </StyledLabel>
      )}
      <StyledInput
        id={inputId}
        hasError={!!error}
        {...props}
      />
      {error && (
        <StyledErrorMessage>
          {error}
        </StyledErrorMessage>
      )}
    </StyledInputContainer>
  );
};

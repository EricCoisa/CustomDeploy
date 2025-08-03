import styled, { css } from 'styled-components';

// Tipos para variantes do botão
export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'outline';
export type ButtonSize = 'small' | 'medium' | 'large';

// Helpers para estilos (funções internas)
const getVariantStyles = (variant: ButtonVariant) => {
  switch (variant) {
    case 'primary':
      return css`
        background-color: #3b82f6;
        color: white;
        border: 2px solid #3b82f6;

        &:hover:not(:disabled) {
          background-color: #2563eb;
          border-color: #2563eb;
        }

        &:focus {
          box-shadow: 0 0 0 3px #bfdbfe;
        }
      `;
    case 'secondary':
      return css`
        background-color: #6b7280;
        color: white;
        border: 2px solid #6b7280;

        &:hover:not(:disabled) {
          background-color: #4b5563;
          border-color: #4b5563;
        }

        &:focus {
          box-shadow: 0 0 0 3px #d1d5db;
        }
      `;
    case 'danger':
      return css`
        background-color: #ef4444;
        color: white;
        border: 2px solid #ef4444;

        &:hover:not(:disabled) {
          background-color: #dc2626;
          border-color: #dc2626;
        }

        &:focus {
          box-shadow: 0 0 0 3px #fecaca;
        }
      `;
    case 'outline':
      return css`
        background-color: transparent;
        color: #3b82f6;
        border: 2px solid #3b82f6;

        &:hover:not(:disabled) {
          background-color: #3b82f6;
          color: white;
        }

        &:focus {
          box-shadow: 0 0 0 3px #bfdbfe;
        }
      `;
    default:
      return css``;
  }
};

const getSizeStyles = (size: ButtonSize) => {
  switch (size) {
    case 'small':
      return css`
        padding: 0.5rem 1rem;
        font-size: 0.875rem;
      `;
    case 'medium':
      return css`
        padding: 0.75rem 1.5rem;
        font-size: 1rem;
      `;
    case 'large':
      return css`
        padding: 1rem 2rem;
        font-size: 1.125rem;
      `;
    default:
      return css``;
  }
};

// Styled Components para Button
export const StyledButton = styled.button<{
  variant: ButtonVariant;
  size: ButtonSize;
  fullWidth?: boolean;
  isLoading?: boolean;
}>`
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  border-radius: 0.5rem;
  font-weight: 500;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease-in-out;
  text-decoration: none;
  white-space: nowrap;
  
  ${props => getVariantStyles(props.variant)}
  ${props => getSizeStyles(props.size)}
  
  ${props => props.fullWidth && css`
    width: 100%;
  `}

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  ${props => props.isLoading && css`
    cursor: wait;
  `}

  &:focus {
    outline: none;
  }
`;

export const LoadingSpinner = styled.div`
  width: 1rem;
  height: 1rem;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
`;

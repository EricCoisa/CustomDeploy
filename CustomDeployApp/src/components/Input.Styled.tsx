import styled from 'styled-components';

// Styled Components para Input
export const StyledInputContainer = styled.div`
  display: flex;
  flex-direction: column;
  margin-bottom: 1rem;
`;

export const StyledLabel = styled.label`
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
  margin-bottom: 0.5rem;
`;

export const StyledInput = styled.input<{ hasError?: boolean }>`
  width: 100%;
  padding: 0.75rem;
  border: 2px solid ${props => props.hasError ? '#ef4444' : '#d1d5db'};
  border-radius: 0.5rem;
  font-size: 1rem;
  transition: border-color 0.2s ease-in-out;

  &:focus {
    outline: none;
    border-color: ${props => props.hasError ? '#ef4444' : '#3b82f6'};
    box-shadow: 0 0 0 3px ${props => props.hasError ? '#fecaca' : '#bfdbfe'};
  }

  &::placeholder {
    color: #9ca3af;
  }
`;

export const StyledErrorMessage = styled.span`
  font-size: 0.75rem;
  color: #ef4444;
  margin-top: 0.25rem;
`;

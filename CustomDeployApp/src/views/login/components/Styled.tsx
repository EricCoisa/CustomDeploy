import styled from 'styled-components';

// Styled Components para LoginForm
export const FormContainer = styled.form`
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  width: 100%;
  max-width: 400px;
`;

export const FormTitle = styled.h2`
  font-size: 1.875rem;
  font-weight: 700;
  text-align: center;
  color: #1f2937;
  margin-bottom: 1rem;
`;

export const ErrorMessage = styled.div`
  background-color: #fef2f2;
  border: 1px solid #fecaca;
  color: #b91c1c;
  padding: 0.75rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  margin-bottom: 1rem;
`;

export const ForgotPasswordLink = styled.a`
  text-align: center;
  color: #3b82f6;
  font-size: 0.875rem;
  text-decoration: none;
  margin-top: 0.5rem;

  &:hover {
    text-decoration: underline;
  }
`;

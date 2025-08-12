import React from 'react';
import styled from 'styled-components';
import { Link, useLocation } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../store';
import { logoutUser } from '../../store/login/actions';

const HeaderContainer = styled.header`
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  border-radius: 1rem;
  margin-bottom: 1rem;
  width: 100%;
  box-sizing: border-box;
`;

const HeaderContent = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.5rem;
  
  @media (max-width: 768px) {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
    padding: 1rem;
  }
`;

const HeaderLeft = styled.div`
  display: flex;
  flex-direction: column;
`;

const HeaderTitle = styled.h1`
  font-size: 2rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
  background: linear-gradient(135deg, #667eea, #764ba2);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
`;

const HeaderSubtitle = styled.p`
  font-size: 1rem;
  color: #6b7280;
  margin: 0.25rem 0 0 0;
`;

const HeaderRight = styled.div`
  display: flex;
  align-items: center;
  gap: 1rem;
`;

const Navigation = styled.nav`
  display: flex;
  gap: 0.5rem;
  margin-right: 1rem;
  
  @media (max-width: 768px) {
    margin-right: 0;
    margin-bottom: 0.5rem;
  }
`;

const NavLink = styled(Link)<{ $isActive?: boolean }>`
  padding: 0.5rem 1rem;
  border-radius: 0.5rem;
  text-decoration: none;
  font-weight: 500;
  font-size: 0.875rem;
  transition: all 0.2s;
  border: 1px solid transparent;
  
  ${props => props.$isActive ? `
    background: #3b82f6;
    color: white;
    border-color: #3b82f6;
  ` : `
    color: #374151;
    background: transparent;
    
    &:hover {
      background: #f3f4f6;
      border-color: #d1d5db;
    }
  `}
`;

const UserInfo = styled.div`
  display: flex;
  align-items: center;
  gap: 1rem;
  
  span {
    color: #374151;
    font-size: 0.95rem;
  }
  
  @media (max-width: 768px) {
    flex-direction: column;
    gap: 0.5rem;
  }
`;

const LogoutButton = styled.button`
  background: #ef4444;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.5rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  
  &:hover {
    background: #dc2626;
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(239, 68, 68, 0.4);
  }
  
  &:active {
    transform: translateY(0);
  }
`;

interface HeaderProps {
  title?: string;
  subtitle?: string;
  showUserInfo?: boolean;
}

export const Header: React.FC<HeaderProps> = ({
  title = "CustomDeploy",
  subtitle = "Sistema de Deploy Automatizado",
  showUserInfo = true,
}) => {
  const dispatch = useAppDispatch();
  const location = useLocation();
  const { user } = useAppSelector(state => state.login);

  const handleLogout = () => {
    dispatch(logoutUser());
  };

  return (
    <HeaderContainer>
      <HeaderContent>
        <HeaderLeft>
          <HeaderTitle>{title}</HeaderTitle>
          <HeaderSubtitle>{subtitle}</HeaderSubtitle>
        </HeaderLeft>
        
        {showUserInfo && (
          <HeaderRight>
            <Navigation>
              <NavLink 
                to="/dashboard" 
                $isActive={location.pathname === '/dashboard'}
              >
                📊 Dashboard
              </NavLink>
              <NavLink 
                to="/iis" 
                $isActive={location.pathname === '/iis'}
              >
                🖥️ IIS
              </NavLink>
              <NavLink 
                to="/publications" 
                $isActive={location.pathname === '/publications'}
              >
                📦 Publicações
              </NavLink>
              <NavLink 
                to="/system" 
                $isActive={location.pathname === '/system'}
              >
                🚀 Sistema
              </NavLink>
              <NavLink 
                to="/test" 
                $isActive={location.pathname === '/test'}
              >
                🧪 Testes
              </NavLink>
            </Navigation>
            
            <UserInfo>
              <span>Bem-vindo, <strong>{user?.username}</strong>!</span>
              <LogoutButton onClick={handleLogout}>
                Logout
              </LogoutButton>
            </UserInfo>
          </HeaderRight>
        )}
      </HeaderContent>
    </HeaderContainer>
  );
};

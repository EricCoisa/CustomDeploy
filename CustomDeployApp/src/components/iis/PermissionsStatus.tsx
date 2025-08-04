import React from 'react';
import { Button } from '../../components';
import styled from 'styled-components';

const PermissionsHeader = styled.div`
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  
  @media (max-width: 768px) {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
`;

const PermissionsTitle = styled.h3`
  margin: 0;
  color: #1f2937;
  font-size: 1.25rem;
`;

const StatusGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
`;

const StatusItem = styled.div<{ status: boolean }>`
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem;
  border-radius: 4px;
  background-color: ${props => props.status ? '#d4edda' : '#f8d7da'};
  border: 1px solid ${props => props.status ? '#c3e6cb' : '#f5c6cb'};
`;

const StatusIcon = styled.span<{ status: boolean }>`
  font-size: 1.2rem;
  color: ${props => props.status ? '#28a745' : '#dc3545'};
`;

const StatusText = styled.span`
  color: #333;
  font-weight: 500;
`;

const OverallStatus = styled.div<{ allGranted: boolean }>`
  padding: 1rem;
  border-radius: 4px;
  text-align: center;
  font-weight: bold;
  background-color: ${props => props.allGranted ? '#d4edda' : '#f8d7da'};
  border: 1px solid ${props => props.allGranted ? '#c3e6cb' : '#f5c6cb'};
  color: ${props => props.allGranted ? '#155724' : '#721c24'};
  margin-bottom: 1rem;
`;

const AdminStatus = styled.div<{ isAdmin: boolean | null }>`
  padding: 1rem;
  border-radius: 4px;
  text-align: center;
  font-weight: bold;
  background-color: ${props => 
    props.isAdmin === true ? '#d4edda' : 
    props.isAdmin === false ? '#f8d7da' : '#e2e3e5'};
  border: 1px solid ${props => 
    props.isAdmin === true ? '#c3e6cb' : 
    props.isAdmin === false ? '#f5c6cb' : '#d6d8db'};
  color: ${props => 
    props.isAdmin === true ? '#155724' : 
    props.isAdmin === false ? '#721c24' : '#6c757d'};
  margin-bottom: 1rem;
`;

const ActionButtons = styled.div`
  display: flex;
  gap: 0.5rem;
  justify-content: center;
`;

interface PermissionsStatusProps {
  permissions: {
    canCreateFolders: boolean;
    canMoveFiles: boolean;
    canExecuteIISCommands: boolean;
    canManageIIS: boolean;
    allPermissionsGranted: boolean;
  } | null;
  isAdministrator: boolean | null;
  onCheckPermissions: () => void;
  onRequestAdmin: () => void;
  loading?: boolean;
}

export const PermissionsStatus: React.FC<PermissionsStatusProps> = ({
  permissions,
  isAdministrator,
  onCheckPermissions,
  onRequestAdmin,
  loading = false
}) => {
  const getAdminStatusText = () => {
    if (isAdministrator === null) return 'Status de administrador não verificado';
    return isAdministrator ? 
      '✅ Executando como Administrador' : 
      '⚠️ Privilégios de Administrador necessários';
  };

  const getOverallStatusText = () => {
    if (!permissions) return 'Permissões não verificadas';
    return permissions.allPermissionsGranted ? 
      '✅ Todas as permissões necessárias estão disponíveis' : 
      '⚠️ Algumas permissões estão faltando';
  };

  if (loading) {
    return (
      <div>
        <PermissionsTitle>Verificando permissões...</PermissionsTitle>
      </div>
    );
  }

  return (
    <div>
      <PermissionsHeader>
        <PermissionsTitle>Status de Permissões</PermissionsTitle>
        <ActionButtons>
          <Button 
            size="small" 
            variant="secondary"
            onClick={onCheckPermissions}
          >
            🔄 Verificar Permissões
          </Button>
          {isAdministrator === false && (
            <Button 
              size="small" 
              onClick={onRequestAdmin}
            >
              🔐 Solicitar Admin
            </Button>
          )}
        </ActionButtons>
      </PermissionsHeader>

      <AdminStatus isAdmin={isAdministrator}>
        {getAdminStatusText()}
      </AdminStatus>

      {permissions && (
        <>
          <OverallStatus allGranted={permissions.allPermissionsGranted}>
            {getOverallStatusText()}
          </OverallStatus>

          <StatusGrid>
            <StatusItem status={permissions.canCreateFolders}>
              <StatusIcon status={permissions.canCreateFolders}>
                {permissions.canCreateFolders ? '✅' : '❌'}
              </StatusIcon>
              <StatusText>Criar Pastas</StatusText>
            </StatusItem>

            <StatusItem status={permissions.canMoveFiles}>
              <StatusIcon status={permissions.canMoveFiles}>
                {permissions.canMoveFiles ? '✅' : '❌'}
              </StatusIcon>
              <StatusText>Mover Arquivos</StatusText>
            </StatusItem>

            <StatusItem status={permissions.canExecuteIISCommands}>
              <StatusIcon status={permissions.canExecuteIISCommands}>
                {permissions.canExecuteIISCommands ? '✅' : '❌'}
              </StatusIcon>
              <StatusText>Executar Comandos IIS</StatusText>
            </StatusItem>

            <StatusItem status={permissions.canManageIIS}>
              <StatusIcon status={permissions.canManageIIS}>
                {permissions.canManageIIS ? '✅' : '❌'}
              </StatusIcon>
              <StatusText>Gerenciar IIS</StatusText>
            </StatusItem>
          </StatusGrid>
        </>
      )}
    </div>
  );
};

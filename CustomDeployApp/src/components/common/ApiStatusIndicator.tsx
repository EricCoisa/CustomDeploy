import React from 'react';
import { useAppSelector } from '../../store';

export const ApiStatusIndicator: React.FC = () => {
  const { apiStatus } = useAppSelector(state => state.login);

  if (apiStatus === 'online') {
    return null; // Não mostrar nada quando está online
  }

  const getStatusConfig = () => {
    switch (apiStatus) {
      case 'offline':
        return {
          color: '#dc2626',
          backgroundColor: '#fee2e2',
          border: '#fecaca',
          icon: '🚫',
          message: 'Servidor offline'
        };
      case 'checking':
        return {
          color: '#d97706',
          backgroundColor: '#fef3c7',
          border: '#fde68a',
          icon: '🔄',
          message: 'Verificando conexão...'
        };
      default:
        return {
          color: '#6b7280',
          backgroundColor: '#f3f4f6',
          border: '#d1d5db',
          icon: '❓',
          message: 'Status desconhecido'
        };
    }
  };

  const status = getStatusConfig();

  return (
    <div style={{
      position: 'fixed',
      top: '10px',
      right: '10px',
      backgroundColor: status.backgroundColor,
      border: `1px solid ${status.border}`,
      borderRadius: '8px',
      padding: '8px 12px',
      fontSize: '14px',
      color: status.color,
      fontWeight: '500',
      zIndex: 1000,
      display: 'flex',
      alignItems: 'center',
      gap: '6px',
      boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
    }}>
      <span>{status.icon}</span>
      <span>{status.message}</span>
    </div>
  );
};

import React, { useState } from 'react';
import { Modal, Button } from '../../components';
import type { CreateAppPoolRequest } from '../../store/iis/types';

interface AppPoolFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateAppPoolRequest) => void;
}

export const AppPoolFormModal: React.FC<AppPoolFormModalProps> = ({
  isOpen,
  onClose,
  onSubmit
}) => {
  const [formData, setFormData] = useState<CreateAppPoolRequest>({
    poolName: '',
    managedRuntimeVersion: 'v4.0',
    pipelineMode: 'Integrated',
    identityType: 'ApplicationPoolIdentity',
    maxProcesses: 1,
    idleTimeout: '00:20:00',
    regularTimeInterval: '1.05:00:00',
    privateMemory: 0,
    virtualMemory: 0
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    const newErrors: Record<string, string> = {};
    
    if (!formData.poolName.trim()) {
      newErrors.poolName = 'Nome do Application Pool é obrigatório';
    }

    setErrors(newErrors);

    if (Object.keys(newErrors).length === 0) {
      onSubmit(formData);
      onClose();
      setFormData({
        poolName: '',
        managedRuntimeVersion: 'v4.0',
        pipelineMode: 'Integrated',
        identityType: 'ApplicationPoolIdentity',
        maxProcesses: 1,
        idleTimeout: '00:20:00',
        regularTimeInterval: '1.05:00:00',
        privateMemory: 0,
        virtualMemory: 0
      });
    }
  };

  const handleChange = (field: keyof CreateAppPoolRequest, value: string | number) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Criar Novo Application Pool"
    >
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
            Nome do Application Pool *
          </label>
          <input
            type="text"
            value={formData.poolName}
            onChange={(e) => handleChange('poolName', e.target.value)}
            placeholder="Ex: MyAppPool"
            style={{
              width: '100%',
              padding: '0.5rem',
              border: `1px solid ${errors.poolName ? '#ff4444' : '#ddd'}`,
              borderRadius: '4px'
            }}
          />
          {errors.poolName && (
            <small style={{ color: '#ff4444' }}>{errors.poolName}</small>
          )}
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
              Versão do .NET Framework
            </label>
            <select
              value={formData.managedRuntimeVersion}
              onChange={(e) => handleChange('managedRuntimeVersion', e.target.value)}
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            >
              <option value="v4.0">.NET Framework 4.x</option>
              <option value="v2.0">.NET Framework 2.0/3.5</option>
              <option value="">No Managed Code</option>
            </select>
          </div>

          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
              Pipeline Mode
            </label>
            <select
              value={formData.pipelineMode}
              onChange={(e) => handleChange('pipelineMode', e.target.value)}
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            >
              <option value="Integrated">Integrated</option>
              <option value="Classic">Classic</option>
            </select>
          </div>
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
            Identity Type
          </label>
          <select
            value={formData.identityType}
            onChange={(e) => handleChange('identityType', e.target.value)}
            style={{
              width: '100%',
              padding: '0.5rem',
              border: '1px solid #ddd',
              borderRadius: '4px'
            }}
          >
            <option value="ApplicationPoolIdentity">ApplicationPoolIdentity</option>
            <option value="LocalSystem">LocalSystem</option>
            <option value="LocalService">LocalService</option>
            <option value="NetworkService">NetworkService</option>
          </select>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
              Máximo de Processos
            </label>
            <input
              type="number"
              min="1"
              value={formData.maxProcesses}
              onChange={(e) => handleChange('maxProcesses', parseInt(e.target.value) || 1)}
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            />
          </div>

          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
              Idle Timeout
            </label>
            <input
              type="text"
              value={formData.idleTimeout}
              onChange={(e) => handleChange('idleTimeout', e.target.value)}
              placeholder="00:20:00"
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            />
            <small style={{ color: '#666', display: 'block', marginTop: '0.25rem' }}>
              Formato: HH:MM:SS
            </small>
          </div>
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
            Intervalo de Reciclagem
          </label>
          <input
            type="text"
            value={formData.regularTimeInterval}
            onChange={(e) => handleChange('regularTimeInterval', e.target.value)}
            placeholder="1.05:00:00"
            style={{
              width: '100%',
              padding: '0.5rem',
              border: '1px solid #ddd',
              borderRadius: '4px'
            }}
          />
          <small style={{ color: '#666', display: 'block', marginTop: '0.25rem' }}>
            Formato: D.HH:MM:SS (ex: 1.05:00:00 = 1 dia e 5 horas)
          </small>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
              Limite de Memória Privada (KB)
            </label>
            <input
              type="number"
              min="0"
              value={formData.privateMemory}
              onChange={(e) => handleChange('privateMemory', parseInt(e.target.value) || 0)}
              placeholder="0 = sem limite"
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            />
          </div>

          <div>
            <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
              Limite de Memória Virtual (KB)
            </label>
            <input
              type="number"
              min="0"
              value={formData.virtualMemory}
              onChange={(e) => handleChange('virtualMemory', parseInt(e.target.value) || 0)}
              placeholder="0 = sem limite"
              style={{
                width: '100%',
                padding: '0.5rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            />
          </div>
        </div>

        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
          <Button type="button" onClick={onClose} variant="secondary">
            Cancelar
          </Button>
          <Button type="submit">
            Criar Application Pool
          </Button>
        </div>
      </form>
    </Modal>
  );
};

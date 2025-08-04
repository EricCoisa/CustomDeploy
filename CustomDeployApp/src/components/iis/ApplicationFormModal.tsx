import React, { useState } from 'react';
import { Modal, Button } from '../../components';
import { FileBrowser } from '../FileBrowser';
import type { CreateApplicationRequest } from '../../store/iis/types';

interface ApplicationFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateApplicationRequest) => void;
  siteName: string;
  appPools: string[];
}

export const ApplicationFormModal: React.FC<ApplicationFormModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  siteName,
  appPools
}) => {
  const [formData, setFormData] = useState<CreateApplicationRequest>({
    siteName,
    appPath: '',
    physicalPath: '',
    appPoolName: appPools[0] || 'DefaultAppPool'
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isFileBrowserOpen, setIsFileBrowserOpen] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    const newErrors: Record<string, string> = {};
    
    if (!formData.appPath.trim()) {
      newErrors.appPath = 'Caminho da aplicação é obrigatório';
    } else if (!formData.appPath.startsWith('/')) {
      newErrors.appPath = 'Caminho deve começar com /';
    }
    
    if (!formData.physicalPath.trim()) {
      newErrors.physicalPath = 'Caminho físico é obrigatório';
    }
    
    if (!formData.appPoolName.trim()) {
      newErrors.appPoolName = 'Application Pool é obrigatório';
    }

    setErrors(newErrors);

    if (Object.keys(newErrors).length === 0) {
      onSubmit({ ...formData, siteName });
      onClose();
      setFormData({
        siteName,
        appPath: '',
        physicalPath: '',
        appPoolName: appPools[0] || 'DefaultAppPool'
      });
    }
  };

  const handleChange = (field: keyof CreateApplicationRequest, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const handlePathSelect = (selectedPath: string) => {
    setFormData(prev => ({ ...prev, physicalPath: selectedPath }));
    if (errors.physicalPath) {
      setErrors(prev => ({ ...prev, physicalPath: '' }));
    }
    setIsFileBrowserOpen(false);
  };

  // Reset form when siteName changes
  React.useEffect(() => {
    setFormData(prev => ({ ...prev, siteName }));
  }, [siteName]);

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`Criar Nova Aplicação em "${siteName}"`}
    >
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
            Site
          </label>
          <input
            type="text"
            value={siteName}
            disabled
            style={{
              width: '100%',
              padding: '0.5rem',
              border: '1px solid #ddd',
              borderRadius: '4px',
              backgroundColor: '#f5f5f5',
              color: '#666'
            }}
          />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
            Caminho da Aplicação *
          </label>
          <input
            type="text"
            value={formData.appPath}
            onChange={(e) => handleChange('appPath', e.target.value)}
            placeholder="Ex: /api, /admin, /app"
            style={{
              width: '100%',
              padding: '0.5rem',
              border: `1px solid ${errors.appPath ? '#ff4444' : '#ddd'}`,
              borderRadius: '4px'
            }}
          />
          {errors.appPath && (
            <small style={{ color: '#ff4444' }}>{errors.appPath}</small>
          )}
          <small style={{ color: '#666', display: 'block', marginTop: '0.25rem' }}>
            Deve começar com / (ex: /api, /admin)
          </small>
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
            Caminho Físico *
          </label>
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <input
              type="text"
              value={formData.physicalPath}
              readOnly
              placeholder="Selecione uma pasta..."
              style={{
                flex: 1,
                padding: '0.5rem',
                border: `1px solid ${errors.physicalPath ? '#ff4444' : '#ddd'}`,
                borderRadius: '4px',
                backgroundColor: '#f8f9fa',
                cursor: 'pointer'
              }}
              onClick={() => setIsFileBrowserOpen(true)}
            />
            <Button 
              type="button" 
              onClick={() => setIsFileBrowserOpen(true)}
              variant="secondary"
              style={{ whiteSpace: 'nowrap' }}
            >
              📁 Procurar
            </Button>
          </div>
          {errors.physicalPath && (
            <small style={{ color: '#ff4444' }}>{errors.physicalPath}</small>
          )}
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
            Application Pool *
          </label>
          <select
            value={formData.appPoolName}
            onChange={(e) => handleChange('appPoolName', e.target.value)}
            style={{
              width: '100%',
              padding: '0.5rem',
              border: `1px solid ${errors.appPoolName ? '#ff4444' : '#ddd'}`,
              borderRadius: '4px'
            }}
          >
            {appPools.map(pool => (
              <option key={pool} value={pool}>{pool}</option>
            ))}
          </select>
          {errors.appPoolName && (
            <small style={{ color: '#ff4444' }}>{errors.appPoolName}</small>
          )}
        </div>

        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem', marginTop: '1rem' }}>
          <Button type="button" onClick={onClose} variant="secondary">
            Cancelar
          </Button>
          <Button type="submit">
            Criar Aplicação
          </Button>
        </div>
      </form>
      
      <FileBrowser
        isOpen={isFileBrowserOpen}
        onClose={() => setIsFileBrowserOpen(false)}
        onSelect={handlePathSelect}
        selectType="directory"
        initialPath="C:\\inetpub\\wwwroot"
      />
    </Modal>
  );
};

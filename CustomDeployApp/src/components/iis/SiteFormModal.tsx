import React, { useState } from 'react';
import { Modal, Button } from '../../components';
import { FileBrowser } from '../FileBrowser/FileBrowser';
import type { CreateSiteRequest } from '../../store/iis/types';

interface SiteFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: CreateSiteRequest) => void;
  appPools: string[];
}

export const SiteFormModal: React.FC<SiteFormModalProps> = ({
  isOpen,
  onClose,
  onSubmit,
  appPools
}) => {
  const [formData, setFormData] = useState<CreateSiteRequest>({
    siteName: '',
    bindingInformation: '*:80:',
    physicalPath: '',
    appPoolName: appPools[0] || 'DefaultAppPool'
  });

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [showFileBrowser, setShowFileBrowser] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    const newErrors: Record<string, string> = {};
    
    if (!formData.siteName.trim()) {
      newErrors.siteName = 'Nome do site é obrigatório';
    }
    
    if (!formData.bindingInformation.trim()) {
      newErrors.bindingInformation = 'Binding information é obrigatório';
    }
    
    if (!formData.physicalPath.trim()) {
      newErrors.physicalPath = 'Caminho físico é obrigatório';
    }
    
    if (!formData.appPoolName.trim()) {
      newErrors.appPoolName = 'Application Pool é obrigatório';
    }

    setErrors(newErrors);

    if (Object.keys(newErrors).length === 0) {
      onSubmit(formData);
      onClose();
      setFormData({
        siteName: '',
        bindingInformation: '*:80:',
        physicalPath: '',
        appPoolName: appPools[0] || 'DefaultAppPool'
      });
    }
  };

  const handleChange = (field: keyof CreateSiteRequest, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const handlePathSelect = (selectedPath: string) => {
    handleChange('physicalPath', selectedPath);
    setShowFileBrowser(false);
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Criar Novo Site IIS"
    >
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
            Nome do Site *
          </label>
          <input
            type="text"
            value={formData.siteName}
            onChange={(e) => handleChange('siteName', e.target.value)}
            placeholder="Ex: MeuSite"
            style={{
              width: '100%',
              padding: '0.5rem',
              border: `1px solid ${errors.siteName ? '#ff4444' : '#ddd'}`,
              borderRadius: '4px'
            }}
          />
          {errors.siteName && (
            <small style={{ color: '#ff4444' }}>{errors.siteName}</small>
          )}
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: 'bold' }}>
            Binding Information *
          </label>
          <input
            type="text"
            value={formData.bindingInformation}
            onChange={(e) => handleChange('bindingInformation', e.target.value)}
            placeholder="Ex: *:80: ou *:8080: ou localhost:3000"
            style={{
              width: '100%',
              padding: '0.5rem',
              border: `1px solid ${errors.bindingInformation ? '#ff4444' : '#ddd'}`,
              borderRadius: '4px'
            }}
          />
          {errors.bindingInformation && (
            <small style={{ color: '#ff4444' }}>{errors.bindingInformation}</small>
          )}
          <small style={{ color: '#666', display: 'block', marginTop: '0.25rem' }}>
            Formato: IP:Porta:Host (ex: *:80: para qualquer IP na porta 80)
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
              onChange={(e) => handleChange('physicalPath', e.target.value)}
              placeholder="Ex: C:\\inetpub\\wwwroot\\MeuSite"
              style={{
                flex: 1,
                padding: '0.5rem',
                border: `1px solid ${errors.physicalPath ? '#ff4444' : '#ddd'}`,
                borderRadius: '4px'
              }}
            />
            <Button
              type="button"
              onClick={() => setShowFileBrowser(true)}
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
            Criar Site
          </Button>
        </div>
      </form>

      <FileBrowser
        isOpen={showFileBrowser}
        onClose={() => setShowFileBrowser(false)}
        onSelect={handlePathSelect}
        selectType="directory"
        initialPath="C:/"
      />
    </Modal>
  );
};

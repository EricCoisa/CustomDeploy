import React, { useState, useEffect } from 'react';
import { Button } from '../Button';
import { useAppDispatch, useAppSelector } from '../../store';
import { executeDeploy, clearError } from '../../store/deploy/actions';
import { fetchSites } from '../../store/iis';
import type { IISSite } from '../../store/iis/types';
import {
  FormContainer,
  FormTitle,
  FormGrid,
  FormGroup,
  FormLabel,
  RequiredMark,
  FormInput,
  FormSelect,
  HelpText,
  ErrorText,
  FormActions,
  LoadingSpinner,
  SuccessMessage,
  ErrorMessage,
  PreviewSection,
  PreviewTitle,
  PreviewText,
} from './styles';
import type { DeployRequest } from '../../services/deployService';

// Interface estendida para acomodar os campos alternativos da API
interface ExtendedInitialData extends Partial<DeployRequest> {
  siteName?: string;
  subPath?: string;
}

interface DeployFormProps {
  title?: string;
  onSuccess?: (result: Record<string, unknown>) => void;
  onError?: (error: string) => void;
  initialData?: ExtendedInitialData;
  showPreview?: boolean;
}

export const DeployForm: React.FC<DeployFormProps> = ({
  title = 'Executar Deploy',
  onSuccess,
  onError,
  initialData,
  showPreview = true,
}) => {
  const dispatch = useAppDispatch();
  const { sites, loading: iisLoading } = useAppSelector(state => state.iis);
  const { isDeploying, lastDeployResult, error } = useAppSelector(state => state.deploy);

  // Estados do formulário
  console.log("Initial form data:", initialData);
  // Converter os valores iniciais do formato da API para o formato do form
  const initialFormData = {
    repoUrl: initialData?.repoUrl || '',
    branch: initialData?.branch || 'main',
    buildCommand: initialData?.buildCommand || [{ comando: 'npm install && npm run build', terminalId: '1' }],
    buildOutput: initialData?.buildOutput || 'dist',
    // Converter siteName para iisSiteName e subPath para applicationPath
    iisSiteName: initialData?.siteName || initialData?.iisSiteName || '',
    targetPath: initialData?.targetPath || '',
    applicationPath: initialData?.subPath || initialData?.applicationPath || '',
    plataforma: initialData?.plataforma || '',
  };

  console.log("Converted initial form data:", initialFormData);
  
  const [formData, setFormData] = useState<DeployRequest>(initialFormData);

  // Estados de validação
  const [errors, setErrors] = useState<Partial<Record<keyof DeployRequest, string>>>({});
  const [touched, setTouched] = useState<Partial<Record<keyof DeployRequest, boolean>>>({});

  // Carregar sites do IIS ao montar o componente
  useEffect(() => {
    if (sites.length === 0 && !iisLoading.sites) {
      dispatch(fetchSites());
    }
  }, [dispatch, sites.length, iisLoading.sites]);

  // Validação de campos
  const validateField = (name: keyof DeployRequest, value: string | {comando: string, terminalId: string}[]): string => {
    switch (name) {
      case 'repoUrl':
        return !value ? 'URL do repositório é obrigatória' : '';
      case 'branch':
        return !value ? 'Branch é obrigatória' : '';
      case 'buildCommand':
        return Array.isArray(value) && value.length === 0 ? 'Comandos de build são obrigatórios' : '';
      case 'buildOutput':
        return !value ? 'Saída do build é obrigatória' : '';
      case 'iisSiteName':
        return !value ? 'Nome do site IIS é obrigatório' : '';
      default:
        return '';
    }
  };

  // Atualizar campo do formulário
  const handleFieldChange = (name: keyof DeployRequest, value: string) => {
    setFormData(prev => ({ ...prev, [name]: value }));
    
    // Validar campo se já foi tocado
    if (touched[name]) {
      const error = validateField(name, value);
      setErrors(prev => ({ ...prev, [name]: error }));
    }
  };

  // Marcar campo como tocado
  const handleFieldBlur = (name: keyof DeployRequest) => {
    setTouched(prev => ({ ...prev, [name]: true }));
    const error = validateField(name, formData[name] !== undefined ? formData[name] : (name === 'buildCommand' ? [] : '')); // Garantir valor padrão apropriado
    setErrors(prev => ({ ...prev, [name]: error }));
  };

  // Validar todo o formulário
  const validateForm = (): boolean => {
    const newErrors: Partial<Record<keyof DeployRequest, string>> = {};
    let isValid = true;

    (Object.keys(formData) as Array<keyof DeployRequest>).forEach(key => {
      const error = validateField(key, formData[key] || '');
      if (error) {
        newErrors[key] = error;
        isValid = false;
      }
    });

    setErrors(newErrors);
    setTouched({
      repoUrl: true,
      branch: true,
      buildCommand: true,
      buildOutput: true,
      iisSiteName: true,
      applicationPath: true,
      plataforma: true,
    });

    return isValid;
  };

  // Submeter formulário
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    console.log('Submitting form with data:', formData);
    if (!validateForm()) {
      console.log('Form validation failed:', errors);
      return;
    }

    try {
      // Mapeia os dados do formulário para o formato esperado pela API


      const result = await dispatch(executeDeploy(formData));

      if (result.meta.requestStatus === 'fulfilled') {
        onSuccess?.(result.payload as Record<string, unknown>);
      } else {
        onError?.(result.payload as string);
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Erro desconhecido';
      onError?.(errorMsg);
    }
  };

  // Limpar mensagens de erro
  const handleClearError = () => {
    dispatch(clearError());
  };

  // Gerar preview do caminho de destino
  const generateTargetPath = (): string => {
    const selectedSite = sites.find(site => site.name === formData.iisSiteName);
    if (!selectedSite) return 'Selecione um site primeiro';
    
    let path = selectedSite.physicalPath;
    if (formData.applicationPath) {
      path += `\\${formData.applicationPath}`;
    }
    return path;
  };

  const handleAddBuildCommand = () => {
    setFormData(prev => ({
      ...prev,
      buildCommand: [...prev.buildCommand, { comando: '', terminalId: '1' }],
    }));
  };

  const handleRemoveBuildCommand = (index: number) => {
    setFormData(prev => ({
      ...prev,
      buildCommand: prev.buildCommand.filter((_, i) => i !== index),
    }));
  };

  const handleBuildCommandChange = (index: number, field: 'comando' | 'terminalId', value: string) => {
    setFormData(prev => {
      const updatedCommands = [...prev.buildCommand];
      updatedCommands[index] = { ...updatedCommands[index], [field]: value };
      return { ...prev, buildCommand: updatedCommands };
    });
  };

  return (
    <FormContainer onSubmit={handleSubmit}>
      <FormTitle>{title}</FormTitle>

      {/* Mensagem de Sucesso */}
      {lastDeployResult && (
        <SuccessMessage>
          ✅ <strong>Deploy concluído com sucesso!</strong>
          <br />
          {lastDeployResult.message}
        </SuccessMessage>
      )}

      {/* Mensagem de Erro */}
      {error && (
        <ErrorMessage>
          ❌ <strong>Erro no deploy:</strong> {error}
          <Button
            type="button"
            size="small"
            variant="secondary"
            onClick={handleClearError}
            style={{ marginLeft: 'auto' }}
          >
            ✕
          </Button>
        </ErrorMessage>
      )}

      <FormGrid>
        {/* Site IIS */}
        <FormGroup>
          <FormLabel>
            Site IIS <RequiredMark>*</RequiredMark>
          </FormLabel>
          <FormSelect
            value={formData.iisSiteName}
            onChange={(e) => handleFieldChange('iisSiteName', e.target.value)}
            onBlur={() => handleFieldBlur('iisSiteName')}
            disabled={isDeploying || iisLoading.sites}
            className={errors.iisSiteName ? 'error' : ''}
          >
            <option value="">Selecione um site...</option>
            {sites.map((site: IISSite) => (
              <option key={site.name} value={site.name}>
                {site.name} ({site.state})
              </option>
            ))}
          </FormSelect>
          {errors.iisSiteName && <ErrorText>⚠️ {errors.iisSiteName}</ErrorText>}
          {iisLoading.sites && <HelpText>Carregando sites...</HelpText>}
        </FormGroup>

        {/* Caminho da Aplicação */}
        <FormGroup>
          <FormLabel>Caminho da Aplicação</FormLabel>
          {formData.iisSiteName ? (
            <FormSelect
              value={formData.applicationPath}
              onChange={(e) => handleFieldChange('applicationPath', e.target.value)}
              onBlur={() => handleFieldBlur('applicationPath')}
              disabled={isDeploying || !formData.iisSiteName}
              className={errors.applicationPath ? 'error' : ''}
            >
              <option value="">Root do Site</option>
              {sites.find(site => site.name === formData.iisSiteName)?.applications?.map((app) => (
                <option key={app.name} value={app.name.replace('/', '')}>
                  {app.name} ({app.state || 'N/A'})
                </option>
              ))}
            </FormSelect>
          ) : (
            <FormSelect disabled>
              <option value="">Selecione um site primeiro</option>
            </FormSelect>
          )}
          <HelpText>Selecione a aplicação onde o deploy será feito</HelpText>
        </FormGroup>

        {/* URL do Repositório */}
        <FormGroup>
          <FormLabel>
            URL do Repositório <RequiredMark>*</RequiredMark>
          </FormLabel>
          <FormInput
            type="url"
            placeholder="https://github.com/usuario/repositorio.git"
            value={formData.repoUrl}
            onChange={(e) => handleFieldChange('repoUrl', e.target.value)}
            onBlur={() => handleFieldBlur('repoUrl')}
            disabled={isDeploying}
            className={errors.repoUrl ? 'error' : ''}
          />
          {errors.repoUrl && <ErrorText>⚠️ {errors.repoUrl}</ErrorText>}
        </FormGroup>

        {/* Branch */}
        <FormGroup>
          <FormLabel>
            Branch <RequiredMark>*</RequiredMark>
          </FormLabel>
          <FormInput
            type="text"
            placeholder="main, master, develop..."
            value={formData.branch}
            onChange={(e) => handleFieldChange('branch', e.target.value)}
            onBlur={() => handleFieldBlur('branch')}
            disabled={isDeploying}
            className={errors.branch ? 'error' : ''}
          />
          {errors.branch && <ErrorText>⚠️ {errors.branch}</ErrorText>}
        </FormGroup>

        {/* Comando de Build */}
        <FormGroup>
          <FormLabel>
            Comandos de Build <RequiredMark>*</RequiredMark>
          </FormLabel>
          {formData.buildCommand.map((command, index) => (
            <div key={index} style={{ display: 'flex', alignItems: 'center', marginBottom: '8px' }}>
              <FormInput
                type="text"
                placeholder="Comando (ex: npm install && npm run build)"
                value={command.comando}
                onChange={(e) => handleBuildCommandChange(index, 'comando', e.target.value)}
                disabled={isDeploying}
                style={{ flex: 2, marginRight: '8px' }}
              />
              <FormInput
                type="text"
                placeholder="Terminal ID (ex: 1)"
                value={command.terminalId}
                onChange={(e) => handleBuildCommandChange(index, 'terminalId', e.target.value)}
                disabled={isDeploying}
                style={{ flex: 1, marginRight: '8px' }}
              />
              <Button
                type="button"
                size="small"
                variant="secondary"
                onClick={() => handleRemoveBuildCommand(index)}
                disabled={isDeploying}
              >
                ✕
              </Button>
            </div>
          ))}
          <Button
            type="button"
            size="small"
            variant="primary"
            onClick={handleAddBuildCommand}
            disabled={isDeploying}
          >
            + Adicionar Comando
          </Button>
          {errors.buildCommand && <ErrorText>⚠️ {errors.buildCommand}</ErrorText>}
          <HelpText>Comandos para instalar dependências e fazer build do projeto</HelpText>
        </FormGroup>

        {/* Pasta de Saída */}
        <FormGroup>
          <FormLabel>
            Pasta de Saída <RequiredMark>*</RequiredMark>
          </FormLabel>
          <FormInput
            type="text"
            placeholder="dist, build, public..."
            value={formData.buildOutput}
            onChange={(e) => handleFieldChange('buildOutput', e.target.value)}
            onBlur={() => handleFieldBlur('buildOutput')}
            disabled={isDeploying}
            className={errors.buildOutput ? 'error' : ''}
          />
          {errors.buildOutput && <ErrorText>⚠️ {errors.buildOutput}</ErrorText>}
          <HelpText>Pasta onde ficam os arquivos compilados</HelpText>
        </FormGroup>
      </FormGrid>

      {/* Preview do Caminho de Destino */}
      {showPreview && formData.iisSiteName && (
        <PreviewSection>
          <PreviewTitle>📁 Caminho de Destino</PreviewTitle>
          <PreviewText>{generateTargetPath()}</PreviewText>
        </PreviewSection>
      )}

      {/* Ações do Formulário */}
      <FormActions>
        <Button
          type="button"
          variant="secondary"
          disabled={isDeploying}
          onClick={() => {
            setFormData({
              repoUrl: '',
              branch: 'main',
              buildCommand: [{ comando: 'npm install && npm run build', terminalId: '1' }],
              buildOutput: 'dist',
              iisSiteName: '',
              targetPath: '',
              applicationPath: '',
              plataforma: '',
            });
            setErrors({});
            setTouched({});
            dispatch(clearError());
          }}
        >
          Limpar
        </Button>
        <Button
          type="submit"
          disabled={isDeploying || Object.values(errors).some(error => !!error)}
          style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}
        >
          {isDeploying && <LoadingSpinner />}
          {isDeploying ? 'Executando Deploy...' : '🚀 Executar Deploy'}
        </Button>
      </FormActions>
    </FormContainer>
  );
};

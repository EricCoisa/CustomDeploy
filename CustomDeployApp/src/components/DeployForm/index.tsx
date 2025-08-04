import React, { useState, useEffect } from 'react';
import { Button } from '../Button';
import { useAppDispatch, useAppSelector } from '../../store';
import { executeDeploy, clearError } from '../../store/deploy/actions';
import { fetchSites } from '../../store/iis';
import type { DeployFormData } from '../../store/deploy/types';
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
  FormTextarea,
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

interface DeployFormProps {
  title?: string;
  onSuccess?: (result: Record<string, unknown>) => void;
  onError?: (error: string) => void;
  initialData?: Partial<DeployFormData>;
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
  const [formData, setFormData] = useState<DeployFormData>({
    siteName: initialData?.siteName || '',
    applicationPath: initialData?.applicationPath || '',
    repoUrl: initialData?.repoUrl || '',
    branch: initialData?.branch || 'main',
    buildCommand: initialData?.buildCommand || 'npm install && npm run build',
    buildOutput: initialData?.buildOutput || 'dist',
  });

  // Estados de validação
  const [errors, setErrors] = useState<Partial<Record<keyof DeployFormData, string>>>({});
  const [touched, setTouched] = useState<Partial<Record<keyof DeployFormData, boolean>>>({});

  // Carregar sites do IIS ao montar o componente
  useEffect(() => {
    if (sites.length === 0 && !iisLoading.sites) {
      dispatch(fetchSites());
    }
  }, [dispatch, sites.length, iisLoading.sites]);

  // Validação de campos
  const validateField = (name: keyof DeployFormData, value: string): string => {
    switch (name) {
      case 'siteName':
        return !value ? 'Site é obrigatório' : '';
      case 'repoUrl':
        if (!value) return 'URL do repositório é obrigatória';
        if (!value.match(/^https?:\/\/.*\.git$|^git@.*\.git$|^https?:\/\/github\.com\/.*|^https?:\/\/gitlab\.com\/.*|^https?:\/\/bitbucket\.org\/.*/)) {
          return 'URL do repositório inválida';
        }
        return '';
      case 'branch':
        return !value ? 'Branch é obrigatória' : '';
      case 'buildCommand':
        return !value ? 'Comando de build é obrigatório' : '';
      case 'buildOutput':
        return !value ? 'Pasta de saída é obrigatória' : '';
      default:
        return '';
    }
  };

  // Atualizar campo do formulário
  const handleFieldChange = (name: keyof DeployFormData, value: string) => {
    setFormData(prev => ({ ...prev, [name]: value }));
    
    // Validar campo se já foi tocado
    if (touched[name]) {
      const error = validateField(name, value);
      setErrors(prev => ({ ...prev, [name]: error }));
    }
  };

  // Marcar campo como tocado
  const handleFieldBlur = (name: keyof DeployFormData) => {
    setTouched(prev => ({ ...prev, [name]: true }));
    const error = validateField(name, formData[name]);
    setErrors(prev => ({ ...prev, [name]: error }));
  };

  // Validar todo o formulário
  const validateForm = (): boolean => {
    const newErrors: Partial<Record<keyof DeployFormData, string>> = {};
    let isValid = true;

    (Object.keys(formData) as Array<keyof DeployFormData>).forEach(key => {
      const error = validateField(key, formData[key]);
      if (error) {
        newErrors[key] = error;
        isValid = false;
      }
    });

    setErrors(newErrors);
    setTouched({
      siteName: true,
      applicationPath: true,
      repoUrl: true,
      branch: true,
      buildCommand: true,
      buildOutput: true,
    });

    return isValid;
  };

  // Submeter formulário
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
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
    const selectedSite = sites.find(site => site.name === formData.siteName);
    if (!selectedSite) return 'Selecione um site primeiro';
    
    let path = selectedSite.physicalPath;
    if (formData.applicationPath) {
      path += `\\${formData.applicationPath}`;
    }
    return path;
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
            value={formData.siteName}
            onChange={(e) => handleFieldChange('siteName', e.target.value)}
            onBlur={() => handleFieldBlur('siteName')}
            disabled={isDeploying || iisLoading.sites}
            className={errors.siteName ? 'error' : ''}
          >
            <option value="">Selecione um site...</option>
            {sites.map((site: IISSite) => (
              <option key={site.name} value={site.name}>
                {site.name} ({site.state})
              </option>
            ))}
          </FormSelect>
          {errors.siteName && <ErrorText>⚠️ {errors.siteName}</ErrorText>}
          {iisLoading.sites && <HelpText>Carregando sites...</HelpText>}
        </FormGroup>

        {/* Caminho da Aplicação */}
        <FormGroup>
          <FormLabel>Caminho da Aplicação</FormLabel>
          <FormInput
            type="text"
            placeholder="Ex: api, app, api/v1 (opcional)"
            value={formData.applicationPath}
            onChange={(e) => handleFieldChange('applicationPath', e.target.value)}
            onBlur={() => handleFieldBlur('applicationPath')}
            disabled={isDeploying}
            className={errors.applicationPath ? 'error' : ''}
          />
          <HelpText>Deixe vazio para fazer deploy no root do site</HelpText>
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
            Comando de Build <RequiredMark>*</RequiredMark>
          </FormLabel>
          <FormTextarea
            placeholder="npm install && npm run build"
            value={formData.buildCommand}
            onChange={(e) => handleFieldChange('buildCommand', e.target.value)}
            onBlur={() => handleFieldBlur('buildCommand')}
            disabled={isDeploying}
            className={errors.buildCommand ? 'error' : ''}
          />
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
      {showPreview && formData.siteName && (
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
              siteName: '',
              applicationPath: '',
              repoUrl: '',
              branch: 'main',
              buildCommand: 'npm install && npm run build',
              buildOutput: 'dist',
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

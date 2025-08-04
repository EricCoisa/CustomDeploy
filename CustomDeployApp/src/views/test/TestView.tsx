import React, { useState } from 'react';
import { ProtectedLayout, Modal, Button, FileBrowser } from '../../components';
import styled from 'styled-components';

// Styled components para a página de teste
const TestContainer = styled.div`
  display: flex;
  flex-direction: column;
  gap: 2rem;
`;

const TestSection = styled.div`
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 2rem;
  border-radius: 1rem;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
`;

const SectionTitle = styled.h2`
  color: #1f2937;
  margin: 0 0 1rem 0;
  font-size: 1.5rem;
  border-bottom: 2px solid #e5e7eb;
  padding-bottom: 0.5rem;
`;

const TestGrid = styled.div`
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-top: 1rem;
`;

const TestButton = styled(Button)`
  width: 100%;
  padding: 1rem;
  font-size: 0.875rem;
`;

const ResultDisplay = styled.div`
  margin-top: 1rem;
  padding: 1rem;
  background: #f3f4f6;
  border-radius: 0.5rem;
  border-left: 4px solid #3b82f6;
  font-family: 'Courier New', monospace;
  font-size: 0.875rem;
  word-break: break-all;
`;

const CodeBlock = styled.pre`
  background: #1f2937;
  color: #e5e7eb;
  padding: 1rem;
  border-radius: 0.5rem;
  overflow-x: auto;
  font-size: 0.875rem;
  margin: 1rem 0;
`;

const InfoBox = styled.div`
  background: #dbeafe;
  border: 1px solid #93c5fd;
  border-radius: 0.5rem;
  padding: 1rem;
  margin: 1rem 0;
  
  h4 {
    margin: 0 0 0.5rem 0;
    color: #1e40af;
  }
  
  p {
    margin: 0;
    color: #1e3a8a;
    font-size: 0.875rem;
  }
`;

export const TestView: React.FC = () => {
  // Estados para diferentes testes
  const [showInfoModal, setShowInfoModal] = useState(false);
  const [showConfirmModal, setShowConfirmModal] = useState(false);
  const [showFileBrowser, setShowFileBrowser] = useState(false);
  const [fileBrowserType, setFileBrowserType] = useState<'file' | 'directory' | 'both'>('both');
  const [lastResult, setLastResult] = useState<string>('');

  // Handlers para testes
  const handleModalTest = (type: 'info' | 'confirm') => {
    if (type === 'info') {
      setShowInfoModal(true);
    } else {
      setShowConfirmModal(true);
    }
    setLastResult(`Modal ${type} aberto`);
  };

  const handleFileBrowserTest = (selectType: 'file' | 'directory' | 'both') => {
    setFileBrowserType(selectType);
    setShowFileBrowser(true);
    setLastResult(`FileBrowser aberto (tipo: ${selectType})`);
  };

  const handleFileSelect = (path: string) => {
    setLastResult(`Arquivo/pasta selecionado: ${path}`);
    console.log('Arquivo selecionado:', path);
  };

  const handleConfirmAction = () => {
    setLastResult('Ação confirmada no modal!');
    setShowConfirmModal(false);
  };

  const clearResults = () => {
    setLastResult('');
  };

  return (
    <ProtectedLayout title="Página de Testes" subtitle="Ambiente para testar componentes">
      <TestContainer>
        {/* Seção de Informações */}
        <TestSection>
          <SectionTitle>ℹ️ Sobre esta página</SectionTitle>
          <InfoBox>
            <h4>Ambiente de Teste Temporário</h4>
            <p>
              Esta página foi criada exclusivamente para testes de componentes durante o desenvolvimento. 
              Aqui você pode testar manualmente todos os componentes implementados e verificar seu funcionamento.
            </p>
          </InfoBox>
          
          <p><strong>Componentes disponíveis para teste:</strong></p>
          <ul>
            <li>🪟 <strong>Modal</strong> - Modais informativos e de confirmação</li>
            <li>📁 <strong>FileBrowser</strong> - Navegador de arquivos e pastas</li>
            <li>🔘 <strong>Button</strong> - Botões personalizados</li>
            <li>📝 <strong>Input</strong> - Campos de entrada</li>
            <li>🎨 <strong>Layout</strong> - Layouts protegidos</li>
          </ul>
        </TestSection>

        {/* Seção de Testes de Modal */}
        <TestSection>
          <SectionTitle>🪟 Testes de Modal</SectionTitle>
          <p>Teste diferentes tipos de modal com várias configurações:</p>
          
          <TestGrid>
            <TestButton onClick={() => handleModalTest('info')}>
              Modal Informativo
            </TestButton>
            <TestButton onClick={() => handleModalTest('confirm')}>
              Modal de Confirmação
            </TestButton>
          </TestGrid>

          <CodeBlock>
{`// Exemplo de uso:
<Modal isOpen={showModal} onClose={() => setShowModal(false)} title="Título">
  <p>Conteúdo do modal...</p>
</Modal>`}
          </CodeBlock>
        </TestSection>

        {/* Seção de Testes de FileBrowser */}
        <TestSection>
          <SectionTitle>📁 Testes de FileBrowser</SectionTitle>
          <p>Teste o navegador de arquivos com diferentes configurações de seleção:</p>
          
          <TestGrid>
            <TestButton onClick={() => handleFileBrowserTest('both')}>
              Arquivos e Pastas
            </TestButton>
            <TestButton onClick={() => handleFileBrowserTest('file')}>
              Apenas Arquivos
            </TestButton>
            <TestButton onClick={() => handleFileBrowserTest('directory')}>
              Apenas Pastas
            </TestButton>
          </TestGrid>

          <CodeBlock>
{`// Exemplo de uso:
<FileBrowser
  isOpen={showBrowser}
  onClose={() => setShowBrowser(false)}
  onSelect={(path) => console.log(path)}
  selectType="both" // 'file' | 'directory' | 'both'
  initialPath="C:/"
/>`}
          </CodeBlock>
        </TestSection>

        {/* Seção de Resultados */}
        <TestSection>
          <SectionTitle>📊 Resultados dos Testes</SectionTitle>
          <p>Aqui você pode ver o resultado das suas interações:</p>
          
          <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
            <Button onClick={clearResults} style={{ background: '#6b7280' }}>
              Limpar Resultados
            </Button>
            <span style={{ fontSize: '0.875rem', color: '#6b7280' }}>
              {lastResult ? 'Último resultado:' : 'Nenhum teste executado ainda'}
            </span>
          </div>

          {lastResult && (
            <ResultDisplay>
              {lastResult}
            </ResultDisplay>
          )}
        </TestSection>

        {/* Seção de Notas de Desenvolvimento */}
        <TestSection>
          <SectionTitle>🛠️ Notas de Desenvolvimento</SectionTitle>
          <InfoBox>
            <h4>Como adicionar novos testes:</h4>
            <p>
              1. Importe o componente que deseja testar<br/>
              2. Adicione um estado para controlar o componente<br/>
              3. Crie um botão na seção apropriada<br/>
              4. Implemente o handler para testar o componente
            </p>
          </InfoBox>
          
          <p><strong>Exemplo de adição de novo teste:</strong></p>
          <CodeBlock>
{`// 1. Import
import { NovoComponente } from '../../components';

// 2. Estado
const [showNovoComponente, setShowNovoComponente] = useState(false);

// 3. Botão
<TestButton onClick={() => setShowNovoComponente(true)}>
  Testar Novo Componente
</TestButton>

// 4. Componente
<NovoComponente 
  isOpen={showNovoComponente}
  onClose={() => setShowNovoComponente(false)}
/>`}
          </CodeBlock>
        </TestSection>
      </TestContainer>

      {/* Modais de Teste */}
      <Modal 
        isOpen={showInfoModal} 
        onClose={() => setShowInfoModal(false)} 
        title="Modal de Teste - Informativo"
      >
        <p>✅ Este é um modal informativo funcionando corretamente!</p>
        <p>Características testadas:</p>
        <ul>
          <li>Abertura e fechamento</li>
          <li>Título personalizado</li>
          <li>Conteúdo HTML</li>
          <li>Botão de fechar (X)</li>
          <li>Fechamento com ESC</li>
          <li>Fechamento clicando fora</li>
        </ul>
        
        <div style={{ marginTop: '1rem', textAlign: 'right' }}>
          <Button onClick={() => setShowInfoModal(false)}>
            Entendi
          </Button>
        </div>
      </Modal>

      <Modal 
        isOpen={showConfirmModal} 
        onClose={() => setShowConfirmModal(false)} 
        title="Modal de Teste - Confirmação"
      >
        <p>⚠️ Esta é uma simulação de confirmação.</p>
        <p><strong>Deseja prosseguir com a ação de teste?</strong></p>
        
        <div style={{ 
          marginTop: '1.5rem', 
          display: 'flex', 
          gap: '1rem', 
          justifyContent: 'flex-end' 
        }}>
          <Button 
            onClick={() => setShowConfirmModal(false)}
            style={{ background: '#6b7280' }}
          >
            Cancelar
          </Button>
          <Button 
            onClick={handleConfirmAction}
            style={{ background: '#ef4444' }}
          >
            Confirmar
          </Button>
        </div>
      </Modal>

      {/* FileBrowser de Teste */}
      <FileBrowser
        isOpen={showFileBrowser}
        onClose={() => setShowFileBrowser(false)}
        onSelect={handleFileSelect}
        selectType={fileBrowserType}
        initialPath="C:/"
      />
    </ProtectedLayout>
  );
};

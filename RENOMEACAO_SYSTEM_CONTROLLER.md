# Renomeação DeployController para SystemController

## Resumo das Alterações

O `DeployController` foi renomeado para `SystemController` pois suas funcionalidades não são especificamente sobre deploy, mas sim sobre funcionalidades do sistema (credenciais Git, validação de repositórios, etc.).

## Alterações Realizadas

### Backend (C#)

1. **Arquivo renomeado**: `DeployController.cs` → `SystemController.cs`
2. **Classe renomeada**: `DeployController` → `SystemController` 
3. **Rota alterada**: `[Route("deploy")]` → `[Route("system")]`
4. **Logger atualizado**: `ILogger<DeployController>` → `ILogger<SystemController>`

### Frontend (TypeScript/React)

1. **Serviço atualizado**: `deployService.ts`
   - Comentário alterado: "DeployController" → "SystemController"
   - Rota atualizada: `/deploy` → `/system`

2. **Constantes atualizadas**: `constants.ts`
   - Seção `DEPLOY` renomeada para `SYSTEM`
   - Novas rotas adicionadas:
     - `CREDENTIALS_STATUS: '/system/credentials/status'`
     - `CREDENTIALS_TEST: '/system/credentials/test'`
     - `REPOSITORY_VALIDATE: '/system/repository/validate'`
   - Nova rota em `ROUTES`: `SYSTEM: '/system'`

3. **Header atualizado**: `Header.tsx`
   - Link alterado: `/deploy` → `/system`
   - Texto alterado: "🚀 Deploy" → "🚀 Sistema"

4. **Roteamento atualizado**: `routes.tsx`
   - Rota alterada: `path: 'deploy'` → `path: 'system'`
   - Mantém o mesmo componente `<DeployView />`

## Funcionalidades do SystemController

O SystemController agora gerencia:

- ✅ **Deploy básico**: Execução de deploy (POST `/system`)
- ✅ **Credenciais Git**: 
  - Status das credenciais (GET `/system/credentials/status`)
  - Teste de credenciais (POST `/system/credentials/test`)
- ✅ **Validação de repositório**: 
  - Validação de repositório e branch (POST `/system/repository/validate`)

## Endpoints Disponíveis

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/system` | Executar deploy |
| GET | `/system/credentials/status` | Verificar status das credenciais |
| POST | `/system/credentials/test` | Testar credenciais |
| POST | `/system/repository/validate` | Validar repositório e branch |

## Compatibilidade

- ✅ **Backend**: Compilando sem erros
- ✅ **Frontend**: TypeScript sem erros
- ✅ **Roteamento**: Atualizado para `/system`
- ✅ **Navegação**: Header atualizado

## Observações

1. O componente `DeployView` foi mantido para preservar a funcionalidade existente
2. As rotas antigas não funcionarão mais - necessário usar `/system`
3. O frontend agora acessa o sistema através da rota `/system`
4. Todas as funcionalidades de deploy foram movidas para o novo `DeploysController` com Entity Framework

## Próximos Passos

1. Testar os endpoints do SystemController
2. Atualizar documentação da API
3. Considerar renomear `DeployView` para `SystemView` se necessário
4. Implementar testes para os novos endpoints

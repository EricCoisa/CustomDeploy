# 🗑️ Exclusão Completa de Publicações (Metadados + Pasta Física)

## 📋 Resumo da Funcionalidade Atualizada

O endpoint `DELETE /deploy/publications/{name}` foi atualizado para realizar **exclusão completa** da publicação, removendo tanto os metadados do arquivo `deploys.json` quanto a pasta física do projeto.

## 🏗️ Implementação da Exclusão Completa

### ✅ **Novo Método no `DeployService`**
```csharp
public (bool Success, string Message) DeletePublicationCompletely(string name)
```

**Funcionalidades:**
- ✅ Localiza o deploy pelos metadados
- ✅ Remove a pasta física do disco (`Directory.Delete(targetPath, true)`)
- ✅ Remove a entrada dos metadados
- ✅ Operação thread-safe com lock
- ✅ Tratamento robusto de erros
- ✅ Logs detalhados de todas as operações

### ✅ **Fluxo de Operação Completa**

1. **Busca metadados** do deploy pelo nome
2. **Localiza pasta física** usando o `targetPath`
3. **Deleta pasta física** se existir (recursivamente)
4. **Remove entrada** dos metadados
5. **Salva arquivo** atualizado
6. **Retorna resultado** com detalhes da operação

## 🌐 Endpoints Atualizados

### 1. **DELETE /deploy/publications/{name}** - Exclusão Completa
**Descrição:** Remove publicação completamente (metadados + pasta física)

**Comportamento:**
- ✅ Deleta pasta física se existir
- ✅ Remove entrada dos metadados
- ✅ Sucesso mesmo se pasta já foi removida manualmente
- ✅ Logs detalhados de cada operação

**Exemplo de Uso:**
```bash
DELETE https://localhost:7071/deploy/publications/MyApp
Authorization: Bearer {token}
```

**Resposta de Sucesso:**
```json
{
  "message": "Deploy 'MyApp' removido completamente (metadados e pasta física)",
  "name": "MyApp",
  "timestamp": "2025-08-02T16:30:00Z"
}
```

**Resposta Parcial (pasta já removida):**
```json
{
  "message": "Deploy 'MyApp' removido dos metadados. Pasta física não existe (já removida ou não encontrada)",
  "name": "MyApp", 
  "timestamp": "2025-08-02T16:30:00Z"
}
```

### 2. **DELETE /deploy/publications/{name}/metadata-only** - Apenas Metadados
**Descrição:** Remove apenas os metadados (mantém pasta física)

**Uso:** Para casos especiais onde você quer manter a pasta física mas limpar os metadados

**Exemplo de Uso:**
```bash
DELETE https://localhost:7071/deploy/publications/MyApp/metadata-only
Authorization: Bearer {token}
```

**Resposta:**
```json
{
  "message": "Metadados removidos (pasta física mantida): Deploy 'MyApp' removido com sucesso dos metadados",
  "name": "MyApp",
  "timestamp": "2025-08-02T16:30:00Z"
}
```

## 🔧 Tratamento de Erros e Cenários

### **Cenário 1: Exclusão Completa Bem-Sucedida**
- ✅ Pasta física deletada
- ✅ Metadados removidos
- ✅ Log: "Deploy removido completamente"

### **Cenário 2: Pasta Já Removida Manualmente**
- ⚠️ Pasta física não encontrada
- ✅ Metadados removidos
- ✅ Log: "Pasta física não encontrada, removendo apenas metadados"

### **Cenário 3: Erro de Permissão na Pasta**
- ❌ Falha ao deletar pasta física
- ✅ Metadados removidos
- ⚠️ Log: "Erro ao deletar pasta física: Access denied"

### **Cenário 4: Deploy Não Encontrado**
- ❌ Deploy não existe nos metadados
- ❌ HTTP 404 NotFound
- 📋 Resposta: "Deploy com nome 'X' não encontrado nos metadados"

## 📊 Logs Gerados

### **Exclusão Bem-Sucedida:**
```
[INFO] Solicitação para remover publicação completamente: MyApp
[INFO] Deletando pasta física: C:\inetpub\wwwroot\MyApp
[INFO] Pasta física deletada: C:\inetpub\wwwroot\MyApp
[INFO] Deploy removido dos metadados: MyApp
```

### **Pasta Já Removida:**
```
[INFO] Solicitação para remover publicação completamente: MyApp
[INFO] Pasta física não encontrada: C:\inetpub\wwwroot\MyApp
[INFO] Deploy removido dos metadados: MyApp
```

### **Erro na Exclusão Física:**
```
[INFO] Solicitação para remover publicação completamente: MyApp
[INFO] Deletando pasta física: C:\inetpub\wwwroot\MyApp
[ERROR] Erro ao deletar pasta física: C:\inetpub\wwwroot\MyApp - Access to the path is denied
[INFO] Deploy removido dos metadados: MyApp
```

## 🛡️ Recursos de Segurança

### ✅ **Validações Implementadas:**
- **Verificação de metadados:** Confirma que deploy existe antes de tentar deletar
- **Paths seguros:** Usa caminhos dos metadados (validados previamente)
- **Thread-safety:** Operações protegidas com lock
- **Tratamento de exceções:** Captura erros de I/O e permissões

### ✅ **Comportamento Robusto:**
- **Tolerância a falhas:** Remove metadados mesmo se pasta física falhar
- **Logs detalhados:** Rastreamento completo de operações
- **Resposta informativa:** Usuário sabe exatamente o que aconteceu

## 🔄 Comparação: Antes vs Depois

### **Antes (Apenas Metadados):**
```bash
DELETE /deploy/publications/MyApp
# Resultado: Apenas metadados removidos, pasta física permanece
```

### **Depois (Exclusão Completa):**
```bash
DELETE /deploy/publications/MyApp
# Resultado: Metadados E pasta física removidos

# Novo endpoint para apenas metadados:
DELETE /deploy/publications/MyApp/metadata-only
# Resultado: Apenas metadados removidos (como era antes)
```

## 🎯 Casos de Uso

### **1. Limpeza Completa de Projeto**
Remover completamente um projeto que não é mais necessário:
```bash
DELETE /deploy/publications/projeto-obsoleto
```

### **2. Preparação para Re-deploy Limpo**
Limpar projeto atual antes de novo deploy:
```bash
DELETE /deploy/publications/MyApp
POST /deploy (novo deploy)
```

### **3. Manutenção de Espaço em Disco**
Remover aplicações antigas para liberar espaço:
```bash
DELETE /deploy/publications/app-v1
DELETE /deploy/publications/app-v2  
DELETE /deploy/publications/teste-antigo
```

### **4. Limpeza Administrativa**
Remover apenas registros órfãos mantendo pastas físicas:
```bash
DELETE /deploy/publications/app-manual/metadata-only
```

## 🧪 Teste da Funcionalidade

### **Cenário de Teste Completo:**

1. **Criar deploy de teste:**
```bash
POST /deploy
{
  "repoUrl": "https://github.com/user/test.git",
  "targetPath": "teste-exclusao"
}
```

2. **Verificar criação:**
```bash
GET /deploy/publications
# Confirmar que "teste-exclusao" aparece na lista
```

3. **Verificar pasta física:**
```bash
# Confirmar que pasta existe em C:\inetpub\wwwroot\teste-exclusao
ls C:\inetpub\wwwroot\teste-exclusao
```

4. **Deletar completamente:**
```bash
DELETE /deploy/publications/teste-exclusao
```

5. **Verificar remoção:**
```bash
GET /deploy/publications
# Confirmar que "teste-exclusao" NÃO aparece na lista

# Confirmar que pasta NÃO existe mais
ls C:\inetpub\wwwroot\teste-exclusao  # Deve retornar erro
```

## 📈 Benefícios da Implementação

### ✅ **Para Administradores:**
- **Limpeza simples:** Um comando remove tudo
- **Gestão de espaço:** Fácil liberação de disco
- **Manutenção reduzida:** Não há pastas órfãs

### ✅ **Para Desenvolvedores:**
- **API completa:** Controle total sobre exclusões
- **Flexibilidade:** Opção de remover apenas metadados
- **Previsibilidade:** Comportamento claro e documentado

### ✅ **Para o Sistema:**
- **Consistência:** Disco e metadados sempre sincronizados
- **Robustez:** Tolerância a falhas de permissão
- **Auditoria:** Logs completos de operações

## ⚠️ Considerações Importantes

### **🚨 Operação Irreversível:**
- A exclusão da pasta física **não pode ser desfeita**
- Certifique-se de que o projeto não é mais necessário
- Considere fazer backup antes de exclusões importantes

### **📋 Boas Práticas:**
1. **Verificar antes:** Use `GET /deploy/publications/{name}` para confirmar dados
2. **Backup crítico:** Faça backup de projetos importantes antes da exclusão
3. **Logs de auditoria:** Monitore logs para rastrear exclusões
4. **Teste em ambiente:** Teste o comportamento em ambiente de desenvolvimento

---

**✅ Status:** Implementado e testado  
**🔧 Compilação:** Sucesso sem erros  
**📅 Data:** Agosto 2025  
**🎯 Funcionalidade:** Exclusão completa (metadados + pasta física)

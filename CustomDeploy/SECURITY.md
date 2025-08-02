# Validação de Segurança - TargetPath

## 🔐 Medidas de Segurança Implementadas

### **Validação de TargetPath**

O CustomDeploy implementa validações rigorosas para garantir que os usuários só possam publicar dentro do diretório autorizado.

## ⚠️ **Problemas de Segurança Prevenidos**

### **1. Path Traversal (Directory Traversal)**
- ❌ `../../../etc/passwd`
- ❌ `..\\..\\Windows\\System32`
- ❌ `app/../../../malware`

### **2. Caminhos Absolutos**
- ❌ `C:\\Windows\\System32\\malware.exe`
- ❌ `/etc/passwd`
- ❌ `/var/www/html/backdoor.php`

### **3. Caracteres Perigosos**
- ❌ Dois pontos `:` (indicam drives no Windows)
- ❌ Barras iniciais `\` ou `/`
- ❌ Sequências `..` (navegação de diretório)

## ✅ **Validações Implementadas**

### **Método `ValidateAndResolveTargetPath`**

```csharp
private string? ValidateAndResolveTargetPath(string targetPath)
{
    // 1. Limpar barras iniciais/finais
    targetPath = targetPath.Trim('\\', '/');

    // 2. Verificar padrões perigosos
    if (targetPath.Contains("..") || 
        targetPath.Contains(":") || 
        targetPath.StartsWith("\\") || 
        targetPath.StartsWith("/") ||
        Path.IsPathFullyQualified(targetPath))
    {
        return null; // Rejeitar
    }

    // 3. Combinar com path base seguro
    var resolvedPath = Path.Combine(_publicationsPath, targetPath);
    var normalizedPath = Path.GetFullPath(resolvedPath);

    // 4. Verificar se ainda está dentro do diretório permitido
    var normalizedPublicationsPath = Path.GetFullPath(_publicationsPath);
    if (!normalizedPath.StartsWith(normalizedPublicationsPath))
    {
        return null; // Rejeitar tentativa de escape
    }

    return normalizedPath; // Aprovado
}
```

## 🎯 **Exemplos de Uso**

### **✅ Caminhos Válidos**

| Input | Resultado |
|-------|-----------|
| `"minha-app"` | `{PublicationsPath}/minha-app` |
| `"projetos/frontend"` | `{PublicationsPath}/projetos/frontend` |
| `"v2/sistema"` | `{PublicationsPath}/v2/sistema` |
| `"app1"` | `{PublicationsPath}/app1` |

### **❌ Caminhos Inválidos**

| Input | Motivo da Rejeição |
|-------|-------------------|
| `"C:\\Windows"` | Caminho absoluto |
| `"../../../etc"` | Path traversal |
| `"/var/www/html"` | Caminho absoluto Unix |
| `"app:backup"` | Contém caractere `:` |
| `"\\\\server\\share"` | UNC path |

## 📝 **Logs de Segurança**

O sistema registra todas as tentativas de acesso:

### **Tentativa Válida:**
```
info: TargetPath resolvido: minha-app -> C:\temp\wwwroot\minha-app
```

### **Tentativa Suspeita:**
```
warn: TargetPath inválido detectado: ../../../etc/passwd
warn: TargetPath tentando escapar do diretório permitido: malware -> C:\Windows\malware
```

## 🛡️ **Configuração Segura**

### **appsettings.json**
```json
{
  "DeploySettings": {
    "PublicationsPath": "C:\\inetpub\\wwwroot" // Diretório controlado
  }
}
```

### **Recomendações:**

1. **📁 Diretório Isolado**: Configure `PublicationsPath` para um diretório dedicado
2. **🔒 Permissões**: Configure permissões restritivas no diretório
3. **📊 Monitoramento**: Monitore logs de tentativas inválidas
4. **🔄 Backups**: Mantenha backups do diretório de publicações

## 🚨 **Códigos de Erro**

### **400 Bad Request**
```json
{
  "message": "TargetPath inválido. Deve ser um caminho relativo dentro de C:\\inetpub\\wwwroot"
}
```

### **Quando ocorre:**
- Path traversal detectado
- Caminho absoluto fornecido
- Tentativa de escape do diretório

## 🎉 **Benefícios**

✅ **Prevenção de ataques** de path traversal  
✅ **Isolamento** de aplicações no diretório correto  
✅ **Logs detalhados** para auditoria de segurança  
✅ **Validação robusta** com múltiplas camadas  
✅ **Compatibilidade** com Windows e Linux paths  

O sistema agora garante que **todas as publicações** ficam dentro do diretório autorizado! 🔐✨

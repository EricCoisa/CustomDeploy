### Testes para o PrepareCommand method - Cenários de "start cmd" com /c automático

### Teste 1: Comando exec simples (sem aspas)
# Entrada: start cmd echo hello
# Esperado: start cmd /c echo hello

### Teste 2: Comando exec com aspas (caso do usuário)
# Entrada: start cmd "nvm use 20.11.1 && npm install && npm run build:carteira"
# Esperado: start cmd /c "nvm use 20.11.1 && npm install && npm run build:carteira"

### Teste 3: Comando que já possui /c (não deve duplicar)
# Entrada: start cmd /c "npm install"
# Esperado: start cmd /c "npm install" (sem alteração)

### Teste 4: Comando que já possui /c simples (não deve duplicar)
# Entrada: start cmd /c echo test
# Esperado: start cmd /c echo test (sem alteração)

### Teste 5: Comando normal sem start cmd (não deve alterar)
# Entrada: npm install
# Esperado: npm install (sem alteração)

### Teste 6: Comando com start mas não cmd (não deve alterar)  
# Entrada: start notepad.exe
# Esperado: start notepad.exe (sem alteração)

### CENÁRIOS DE TESTE IMPLEMENTADOS:

## 1. Detecta "start cmd " seguido de texto sem aspas
- ✅ Adiciona /c entre "start cmd " e o comando

## 2. Detecta "start cmd \"" (com aspas)
- ✅ Substitui "start cmd \"" por "start cmd /c \""

## 3. Verifica se já contém "/c" para evitar duplicação
- ✅ Usa !command.Contains("start cmd /c") para verificar

## 4. Mantém outros tipos de comando inalterados
- ✅ Apenas aplica a regra para comandos que começam com "start cmd "

### Logs esperados:
- "Detectado comando 'start cmd' sem /c, adicionando automaticamente"
- "Comando modificado: {comando_final}"

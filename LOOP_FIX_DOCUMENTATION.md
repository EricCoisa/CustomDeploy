# Correção do Loop Infinito no Healthcheck

## 🐛 Problema Identificado

O front-end estava em loop infinito acessando `http://localhost:5092/healthcheck` devido a uma condição problemática no `AuthInitializer`.

### Causa Raiz

1. **Estado inicial problemático**: O `apiStatus` começava como `'checking'`
2. **Condição de loading**: `if (isValidatingToken || apiStatus === 'checking')`
3. **Ciclo infinito**: 
   - Estado inicial: `apiStatus: 'checking'` → Mostra loading
   - `useEffect` executa → `checkAuthState()` 
   - `checkAuthState()` → `tokenValidationStart()` → `isValidatingToken: true, apiStatus: 'checking'`
   - Continua em loading → `useEffect` executa novamente...

## ✅ Correções Implementadas

### 1. Estado Inicial Corrigido
```typescript
// ANTES (problemático)
const initialState: LoginState = {
  // ...
  apiStatus: 'checking', // ❌ Causava loop
};

// DEPOIS (corrigido)  
const initialState: LoginState = {
  // ...
  apiStatus: 'online', // ✅ Assume online e verifica depois
};
```

### 2. Lógica do AuthInitializer Melhorada
```tsx
// ANTES (problemático)
const AuthInitializer = ({ children }) => {
  useEffect(() => {
    dispatch(checkAuthState()); // ❌ Executava sempre
  }, [dispatch]);

  if (isValidatingToken || apiStatus === 'checking') { // ❌ Loop
    return <LoadingScreen />;
  }
  // ...
};

// DEPOIS (corrigido)
const AuthInitializer = ({ children }) => {
  const [hasInitialized, setHasInitialized] = useState(false);

  useEffect(() => {
    if (!hasInitialized) { // ✅ Só executa uma vez
      dispatch(checkAuthState());
      setHasInitialized(true);
    }
  }, [dispatch, hasInitialized]);

  if (isValidatingToken) { // ✅ Só loading durante validação ativa
    return <LoadingScreen />;
  }
  // ...
};
```

### 3. Condição de Loading Simplificada
- **Antes**: `isValidatingToken || apiStatus === 'checking'` (causava loop)
- **Depois**: `isValidatingToken` apenas (mais preciso)

## 🔄 Fluxo Corrigido

1. **Inicialização**: `apiStatus: 'online'`, `isValidatingToken: false`
2. **AuthInitializer monta**: Renderiza aplicação normalmente
3. **useEffect executa** (apenas uma vez): `hasInitialized: false` → executa `checkAuthState()`
4. **checkAuthState()**: 
   - `tokenValidationStart()` → `isValidatingToken: true`
   - Mostra loading apenas agora
   - Executa verificações (healthcheck → token validation)
   - Atualiza estado final baseado no resultado
5. **Finalização**: `isValidatingToken: false` → Para de mostrar loading

## 🎯 Benefícios da Correção

- ✅ **Sem loops infinitos**: Execução única na inicialização
- ✅ **Performance melhorada**: Não há requisições desnecessárias
- ✅ **UX mais fluida**: Loading só quando necessário
- ✅ **Logs limpos**: Sem spam de requisições
- ✅ **Lógica mais clara**: Estados bem definidos

## 🧪 Como Testar

1. **Abrir DevTools** → Network tab
2. **Recarregar página** (F5)
3. **Verificar requests**: 
   - Deve aparecer apenas 1 request para `/healthcheck`
   - Deve aparecer apenas 1 request para `/auth/validate-token` (se token existir)
   - Não deve haver requests repetidos

## 📝 Arquivos Modificados

- `src/store/login/reducers/index.ts` - Estado inicial corrigido
- `src/components/AuthInitializer.tsx` - Lógica de inicialização melhorada

## 🎉 Resultado

O loop infinito foi eliminado e a aplicação agora funciona corretamente, executando a validação de autenticação apenas uma vez na inicialização, conforme esperado.

# 🧪 Teste da API CustomDeploy

## Endpoint de Login
POST http://localhost:5092/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password"
}

### Exemplo de resposta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-08-03T18:54:24.443Z"
}
```

## Como testar no PowerShell:

```powershell
# Teste de login
$response = Invoke-RestMethod -Uri "http://localhost:5092/auth/login" -Method Post -ContentType "application/json" -Body '{"username": "admin", "password": "password"}'
$response

# Testar com credenciais inválidas
Invoke-RestMethod -Uri "http://localhost:5092/auth/login" -Method Post -ContentType "application/json" -Body '{"username": "wrong", "password": "wrong"}'
```

## Credenciais válidas:
- **Username:** admin
- **Password:** password

## Credenciais inválidas retornam:
```json
{
  "message": "Invalid credentials"
}
```

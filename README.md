# CryptoPlatform - Plataforma de Gestão de Criptomoedas

## 📋 Arquitetura do Projeto

Este projeto utiliza uma **arquitetura em camadas** com separação entre API REST e Serviço SOAP/XML:

```
┌─────────────┐
│   Cliente   │
└──────┬──────┘
       │ HTTP/REST (JSON)
┌──────▼────────────┐
│   API (REST)      │ ← Porta 7141 (HTTPS)
│   - Auth (JWT)    │
│   - Controllers   │
└──────┬────────────┘
       │ HTTP/XML (SOAP-like)
       │ + API Key
┌──────▼────────────┐
│  DataService      │ ← Porta 7165 (HTTPS)
│  (XML/SOAP)       │
└──────┬────────────┘
       │ SQL
┌──────▼────────────┐
│  PostgreSQL       │
└───────────────────┘
```

## 🏗️ Estrutura de Projetos

- **API**: API REST principal com autenticação JWT
- **DataService**: Serviço intermediário SOAP/XML que acessa o banco
- **Application**: DTOs e interfaces
- **Domain**: Entidades de domínio
- **Infrastructure**: Repositórios e serviços externos (PostgreSQL, CoinGecko)

## 🔒 Segurança

### 1. API Key no DataService

O DataService está protegido por **API Key** para garantir que apenas a API principal pode acessá-lo.

**Headers necessários:**
```
X-Api-Key: crypto-platform-secure-key-2024
```

**⚠️ IMPORTANTE:** Em produção, mude a API Key para um valor secreto!

### 2. Configuração da API Key

**DataService** (`DataService/appsettings.json`):
```json
{
  "ApiKey": "crypto-platform-secure-key-2024"
}
```

**API Principal** (`API/appsettings.json`):
```json
{
  "DataService": {
    "Url": "https://localhost:7165",
    "ApiKey": "crypto-platform-secure-key-2024"
  }
}
```

### 3. Secrets em Produção

**❌ NÃO FAÇA ISSO:**
- Commitar senhas de banco no Git
- Usar API Keys padrão em produção
- Expor JWT Secret

**✅ FAÇA ISSO:**

#### Opção 1: Variáveis de Ambiente
```bash
# Linux/macOS
export ConnectionStrings__Postgres="Host=..."
export Jwt__Secret="sua-chave-secreta"
export DataService__ApiKey="sua-api-key"

# Windows PowerShell
$env:ConnectionStrings__Postgres="Host=..."
$env:Jwt__Secret="sua-chave-secreta"
$env:DataService__ApiKey="sua-api-key"
```

#### Opção 2: User Secrets (desenvolvimento)
```bash
# No diretório da API
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "sua-chave-secreta"
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=..."

# No diretório do DataService
dotnet user-secrets init
dotnet user-secrets set "ApiKey" "sua-api-key"
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=..."
```

#### Opção 3: Azure Key Vault / AWS Secrets Manager
Para produção real, use um gerenciador de secrets.

## 🚀 Como Executar

### 1. Configurar o Banco de Dados

O projeto já está configurado para PostgreSQL no Neon.tech. A connection string está em:
- `API/appsettings.json`
- `DataService/appsettings.json`

### 2. Executar o DataService

```bash
cd DataService
dotnet run
```

O DataService estará disponível em:
- **HTTPS**: https://localhost:7165
- **Swagger**: https://localhost:7165/

### 3. Executar a API Principal

```bash
cd API
dotnet run
```

A API estará disponível em:
- **HTTPS**: https://localhost:7141
- **Swagger**: https://localhost:7141/swagger

### 4. Testar a Comunicação

#### A) Registrar um usuário
```bash
POST https://localhost:7141/api/auth/register
Content-Type: application/json

{
  "email": "teste@example.com",
  "password": "senha123"
}
```

#### B) Fazer login e obter token
```bash
POST https://localhost:7141/api/auth/login
Content-Type: application/json

{
  "email": "teste@example.com",
  "password": "senha123"
}
```

#### C) Adicionar transação (com token)
```bash
POST https://localhost:7141/api/portfolio/transaction
Authorization: Bearer {seu-token}
Content-Type: application/json

{
  "cryptoId": "bitcoin",
  "type": "BUY",
  "quantity": 0.5,
  "priceEur": 30000
}
```

## 📊 Endpoints Principais

### API (REST - JSON)

- `POST /api/auth/register` - Registrar usuário
- `POST /api/auth/login` - Login
- `GET /api/portfolio` - Ver portfolio (requer auth)
- `POST /api/portfolio/transaction` - Adicionar transação (requer auth)
- `GET /api/watchlist` - Ver watchlist (requer auth)
- `POST /api/watchlist/{cryptoId}` - Adicionar à watchlist
- `GET /api/market/{cryptoId}` - Ver dados do mercado

### DataService (SOAP/XML)

- `GET /data/portfolio/{userId}` - Listar portfolios (XML)
- `POST /data/portfolio` - Criar portfolio (XML)
- `PUT /data/portfolio/{portfolioId}` - Atualizar portfolio
- `DELETE /data/portfolio/{portfolioId}` - Deletar portfolio
- `GET /data/watchlist/{userId}` - Listar watchlist (XML)
- `POST /data/watchlist` - Adicionar à watchlist (XML)
- `DELETE /data/watchlist` - Remover da watchlist
- `GET /data/transaction/{portfolioId}` - Listar transações (XML)
- `POST /data/transaction` - Criar transação (XML)

**⚠️ Todos os endpoints do DataService requerem o header `X-Api-Key`**

## 🔧 Melhorias Implementadas

### ✅ Correções Aplicadas:

1. **Serialização XML**
   - Adicionados atributos `[XmlRoot]` e `[XmlElement]` às entidades
   - Portfolio, Transaction e WatchlistItem agora serializam corretamente

2. **Tratamento de Erros**
   - Try-catch em todos os métodos do DataServiceClient
   - Validação de parsing XML
   - Mensagens de erro descritivas

3. **Validações no DataService**
   - Validação de Guid.Empty
   - Validação de strings vazias
   - Validação de valores negativos
   - Validação de tipos de transação (BUY/SELL)

4. **Segurança**
   - API Key Middleware no DataService
   - Proteção contra acesso não autorizado
   - Swagger funciona sem API Key (para testes)

5. **Documentação**
   - Swagger em ambos os serviços
   - README com instruções completas
   - Comentários no código

## 🧪 Testar com Swagger

### DataService (https://localhost:7165/)

1. Abra o Swagger (não precisa de API Key)
2. Teste os endpoints diretamente
3. Note que os dados são retornados em XML

### API Principal (https://localhost:7141/swagger)

1. Registre um usuário
2. Faça login e copie o token
3. Clique em "Authorize" no Swagger
4. Cole o token (sem "Bearer", só o token)
5. Teste os endpoints autenticados

## 📝 Notas Importantes

- **AMBOS os serviços precisam estar rodando** ao mesmo tempo
- A API não acessa o banco diretamente, sempre via DataService
- O DataService valida todas as entradas antes de processar
- Use HTTPS em produção e configure certificados SSL válidos

## 🐛 Troubleshooting

### DataService não responde
```bash
# Verifique se está rodando
curl -k https://localhost:7165/

# Verifique a API Key
curl -k -H "X-Api-Key: crypto-platform-secure-key-2024" https://localhost:7165/data/portfolio/{userId}
```

### API não consegue comunicar com DataService
```bash
# Verifique se a API Key está correta em ambos appsettings.json
# Verifique se a URL está correta (porta 7165)
# Verifique os logs do DataService
```

### Erro de conexão ao PostgreSQL
```bash
# Verifique se a connection string está correta
# Verifique se tem acesso à internet (Neon.tech é cloud)
# Verifique os logs para mais detalhes
```

---

## 👥 Contribuir

Este projeto foi desenvolvido como trabalho acadêmico para demonstrar:
- Arquitetura em camadas
- Comunicação SOAP/XML entre serviços
- Autenticação JWT
- Integração com APIs externas (CoinGecko)
- Uso de PostgreSQL

---

**Desenvolvido com .NET 8.0**

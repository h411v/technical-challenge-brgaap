## Backend - .NET 8 API

### Requisitos
- .NET 8 SDK
- PostgreSQL

### Passos

1. Configurar string de conexão em appsettings.json
2. Criar banco no PostgreSQL
3. Na pasta backend/TodoApp.Api:

```bash
dotnet restore
dotnet run
```

Para testes com xUnit, na pasta backend/TodoApp.Tests:

```bash
dotnet restore
dotnet test
```

---

## 📌 Frontend

### Requisitos
- Node.js
- UI5 CLI

### Instalar UI5 CLI

```bash
npm install --global @ui5/cli

```

### Rodando UI5

Na pasta frontend:

```bash
npm install
npm start
```

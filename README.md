# 🏢 API de Gerenciamento de Empresas e Funcionários

API REST desenvolvida para a disciplina de **Arquitetura e Desenvolvimento de APIs** da UNINTER.

## 📋 Descrição
API completa para CRUD (Create, Read, Update, Delete) de empresas e seus funcionários, seguindo padrões RESTful e boas práticas de desenvolvimento com .NET 8.

## 🎯 Funcionalidades
- ✅ CRUD completo de Empresas
- ✅ CRUD completo de Funcionários
- ✅ Validações de CNPJ e dados
- ✅ Impedir exclusão de empresa com funcionários
- ✅ Listar funcionários por empresa
- ✅ Tratamento robusto de erros com try/catch
- ✅ Logging detalhado de todas as operações
- ✅ Documentação interativa com Swagger

## 🚀 Tecnologias Utilizadas
- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - Criação da API
- **Entity Framework Core 8** - ORM para banco de dados
- **MySQL 8** - Sistema gerenciador de banco de dados
- **Pomelo.EntityFrameworkCore.MySql** - Provedor MySQL para EF Core
- **Swagger (Swashbuckle.AspNetCore)** - Documentação interativa
- **Docker** - Containerização opcional do banco

## 🏗️ Arquitetura
- **Padrão MVC** (Model-View-Controller)
- **Repository Pattern** para abstração de dados
- **Entity Framework Core** com abordagem Code-First
- **Injeção de Dependência** nativa do .NET
- **Tratamento Global de Exceções**

## 📁 Estrutura do Projeto

ApiEmpresas/
├── Controllers/ # Controladores da API
│ ├── EmpresasController.cs
│ └── FuncionariosController.cs
├── Models/ # Modelos de dados
│ ├── Empresa.cs
│ └── Funcionario.cs
├── Data/ # Contexto do banco
│ └── AppDbContext.cs
├── Repositories/ # Padrão Repository
│ ├── IRepository.cs
│ └── Repository.cs
├── Migrations/ # Migrações do EF Core
├── Program.cs # Ponto de entrada
├── appsettings.json # Configurações
└── README.md # Este arquivo

## ⚙️ Pré-requisitos
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL 8+](https://dev.mysql.com/downloads/) 
- [Visual Studio Code](https://code.visualstudio.com/) 
- [Swagger UI](https://swagger.io/)

## 🛠️ Configuração e Execução

### 1. Clonar o repositório

- git clone https://github.com/seuusuario/api-empresas.git
- cd api-empresas

### 2. Configurar banco de dados

- CREATE DATABASE ApiEmpresasDB;

### 3. Configurar connection string

- Edite appsettings.json:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ApiEmpresasDB;User=root;Password=root;"
  }
}

### 4. Instalar dependências

- dotnet restore

### 5. Aplicar migrações

- dotnet ef migrations add InitialCreate
- dotnet ef database update

### 6. Executar a aplicação

- dotnet run

### Documentação Automática com Swagger

Esta API inclui **documentação gerada automaticamente** pelo Swashbuckle (Swagger). Toda a documentação é criada em tempo real a partir do código-fonte.

### Acesso à Documentação

# 1. Acesse a documentação interativa
http://localhost:5004/swagger

# 3. Acesse a especificação OpenAPI 
http://localhost:5004/swagger/v1/swagger.json

### 📡 Endpoints da API
# Empresas

Métodos	
GET /api/Empresas - Lista todas empresas
GET	/api/Empresas/{id} - Obtém empresa por ID
POST /api/Empresas - Cria nova empresa
PUT	/api/Empresas/{id} - Atualiza empresa
DELETE	/api/Empresas/{id} - Exclui empresa
GET	/api/Empresas/{id}/funcionarios	Lista funcionários da empresa

# Funcionários
Métodos
GET	/api/Funcionarios - Lista todos funcionários
GET	/api/Funcionarios/{id} - Obtém funcionário por ID
POST /api/Funcionarios - Cria novo funcionário
PUT	/api/Funcionarios/{id} - Atualiza funcionário
DELETE /api/Funcionarios/{id} - Exclui funcionário
GET	/api/Funcionarios/empresa/{empresaId} - Lista funcionários por empresa

### 👨‍🎓 Informações do Aluno
Nome: Nathalia Escossio Cavalcante
RU: 4888825
Disciplina: Arquitetura e Desenvolvimento de APIs
Professores: Rodrigo da S. do Nascimento, Osmar T. P. D. Junior
Instituição: UNINTER
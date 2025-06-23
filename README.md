# 🧠 Cadastro de Usuários - Backend C#/.NET

## ❓ What (O quê?)
Esta aplicação é uma **API RESTful em C#/.NET** desenvolvida com o objetivo de realizar o **CRUD completo 
de usuários (funcionários)** de uma empresa, grupo de RH ou pequeno comércio. 
A API oferece os seguintes recursos:

- Criar usuários (POST)
- Listar usuários (GET)
- Atualizar usuários (PUT)
- Remover usuários (DELETE)

> O projeto segue uma arquitetura em camadas, com separação clara entre API, Aplicação, Domínio e Infraestrutura.

---

## 🎯 Why (Por quê?)
Este sistema foi desenvolvido com o propósito de servir como **exemplo educacional e prático** para:

- Estudantes de .NET/C#
- Desenvolvedores interessados em boas práticas (DDD, testes automatizados, camadas)
- Pequenos negócios que queiram uma base de código para gerenciar funcionários


> Além disso, o projeto já está **preparado para escalabilidade via microserviços**, como descrito abaixo.

---

## 🧱 Arquitetura Futura (Microserviços e Mensageria)
No roadmap de evolução deste sistema, estão previstas:

- 🔐 Uma **API independente de autenticação JWT** para login/autorização
- 📦 Uma estrutura de **microserviços** desacoplados
- 🔁 Implementação de **serviço de mensageria com RabbitMQ**, para comunicação entre APIs:
  - Exemplo: Quando um novo usuário for criado, o evento será enviado para outros serviços (como Projetos, RH, etc)
- ☁️ Cada microserviço terá seu próprio banco de dados, podendo variar (SQL Server, MongoDB, etc)

---

## 🌍 Where (Onde?)
A aplicação pode ser executada **localmente** em sua máquina, utilizando:

- Docker (para o banco de dados SQL Server)
- .NET SDK (para compilar e rodar a aplicação)
- Swagger UI (para testes dos endpoints)

---

## 📆 When (Quando?)
- **Primeira versão**: 2025
- **Última atualização**: Junho de 2025 
- **Próximos passos**:
  - ✅ Integração com RabbitMQ (mensageria)
  - ✅ Desacoplamento de autenticação para outro microserviço
  - ⏳ Integração com outros bancos de dados em outras aplicaçôes

---

## 👤 Who (Quem?)
Desenvolvido por **Eduardo Gomes** como projeto open-source.  
Todos são bem-vindos para contribuir, testar ou sugerir melhorias.

---

## ⚙️ How (Como rodar o projeto?)

### ✔️ Pré-requisitos

- [.NET 8 SDK ou superior](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/products/docker-desktop)
- Editor de código (como [Visual Studio 2022](https://visualstudio.microsoft.com/pt-br/) ou [Visual Studio Code](https://code.visualstudio.com/))
---

## ⚠️ Para rodar esta aplicação localmente:
---

### 📛 **Atenção - Segurança em ambientes reais**
---

- As credenciais (`Usuário: sa`, `Senha: MinhaSenha@2025`, `Database: master`) estão sendo demonstradas, neste projeto apenas para fins **educacionais e de testes locais**.
---

### ❌ **Nunca exponha senhas diretamente em arquivos como:
---

- `README.md` ou `docker-compose.yml`** em ambientes reais (produção ou projetos privados).

- Em projetos sérios, utilize **variáveis de ambiente** e arquivos `.env` para armazenar informações sensíveis.
---
### Exemplo de uso seguro: 

- No `docker-compose.yml`, faça:

    ```yaml
  environment:
  MSSQL_SA_PASSWORD=${SA_PASSWORD}
  ```
   


- Na raiz do projeto (Solução),
- Crie um arquivo `.env`.
- Adicione as seguintes informações:

     ````env
     SA_PASSWORD=SuaSenhaAqui123!
     ````

☑️ Assim, `SA_PASSWORD` conterá o valor definido por você, no arquivo `.env`.  


🔔 O arquivo `.env` **não deve ser versionado**!!! Ou seja, ele tem de ser acrescentado no .gitignore.

---
## 🐳 Subindo os containers (SQL Server, RabbitMQ e Mailhog)

No diretório onde se encontra o arquivo `docker-compose.yml`, abra um terminal de sua preferência (como **CMD**, **PowerShell**, **Git Bash** ou outro compatível com Docker) e execute o seguinte comando:

```bash
docker-compose up -d
```
---

## 🛠️ Rodar migrations para criar banco do zero

No VisualStudio, na guia "Ferramentas", escolher a opção "Gerenciador de Pacotes Nugets". 
Em seguida, escolher a opção: "**Console do Gerenciador de Pacotes**"

Ao abrir o console, na parte superior, certifique-se de que a dropdow **Projeto padrão**: Seja o `UsuariosApp.InfraStructure` 
Além disso, certifique-se que o **projeto de inicialização** seja o `UsuariosApp.API`.

🔦 **Dica:** O projeto de inicialização se destaca por apresentar um tom de cor mais forte que os outros projetos.

Após isso, execute os seguintes comandos:

```bash
Add-Migration Initial -Project UsuariosApp.InfraStructure -StartupProject UsuariosApp.API
```

```bash
Update-Database -Project UsuariosApp.InfraStructure -StartupProject UsuariosApp.API
```

⚠️ O primeiro comando (Add-Migration) só é necessário se você precisar criar ou atualizar migrations.
Para aplicar migrations existentes, apenas o segundo comando (dotnet ef database update) é suficiente.

--- **Observação**: Se você já tiver um banco de dados existente, pode pular esta etapa.

---
## ▶️ Executando o projeto

Navegue até a pasta `src/API/` e rode o seguinte comando:

```bash
dotnet run
```

---

### 🔌 Acesse o Swagger em:

```
https://localhost:5009/swagger
```

### ✅ Endpoints disponíveis

| Método | Rota           | Descrição               |
|--------|----------------|--------------------------|
| POST   | /api/usuarios/cadastrar  | Cadastrar novo usuário |
| GET    | /api/usuarios/obterpor/{id}  | Consulta usuários por Id |
| GET    | /api/usuarios/consultartodos  | Listar todos os usuários |
| PUT    | /api/usuarios/atualizarconta/{id}  | Atualizar um usuário com base no Id |
| DELETE | /api/usuarios/deletar/{id}  | Remover um usuário |
---
## 🧪 Testes Automatizados

A aplicação foi testada com o framework **xUnit**, cobrindo:

- Controllers (`UsuariosController`)
- Middleware (`TratamentoExcecoesMiddleware`)
- Validações (`UsuarioValidator`)
- Criptografia (`ServicoDeCriptografia`)
- Serviços (`UsuarioServico`)
- Domínio (`Usuario`)
- Repositórios e Unidade de Trabalho com **EF InMemory**

Os testes estão organizados na pasta `/tests`.

Para executar os testes:

```bash
dotnet test
```

---


## 📦 Serviços de Infraestrutura no Docker

Este projeto inclui no docker-compose.yml os seguintes serviços:

- 🐘 SQL Server (banco de dados relacional)

- 🐇 RabbitMQ com painel em http://localhost:15672 (usuário: guest, senha: guest)

- ✉️ Mailhog (teste de envio de e-mails) em http://localhost:8025

---

## 📝 Licença

Este projeto está licenciado sob a licença **MIT**.  
Isso significa que você pode utilizar, modificar, distribuir e até comercializar este código, **desde que mantenha os créditos ao autor original**.

---

## 📬 Contribuições

Sinta-se à vontade para abrir **issues** ou enviar **pull requests**.  
Toda colaboração é bem-vinda!

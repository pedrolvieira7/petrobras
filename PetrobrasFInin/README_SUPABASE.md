# SIAF — implantação Railway + Supabase (PostgreSQL)

Esta atualização mantém o visual e as funcionalidades MVC do repositório original,
substituindo SQL Server por PostgreSQL e adicionando sessões autenticadas.

## Antes de publicar

- Faça backup do banco SQL Server original, se você precisa dos registros antigos.
  **A troca de provedor não importa os registros existentes automaticamente.**
- O projeto passa a usar .NET 10 LTS. Atualize o SDK do computador se for executá-lo localmente.
- O projeto Supabase deve estar criado. Use apenas **dados fictícios** na demonstração
  até revisar controle de acesso e regras de privacidade.

## 1. Supabase: copiar os dados de conexão

No painel Supabase, clique em **Conectar** > **Pooler de sessões (Session pooler)**.
Copie o host, porta, nome de usuário e nome do banco de dados. Para conexão em IPv4,
use **Pooler de sessões**, porta **5432**, não o pooler de transações (6543).

A string ADO.NET para .NET deve ter este formato (substitua os valores):

```
Host=HOST_DO_POOLER;Port=5432;Database=postgres;Username=postgres.ID_DO_PROJETO;Password=SUA_SENHA;SSL Mode=Require;Timeout=20;Command Timeout=30;Maximum Pool Size=10
```

**NÃO cole sua senha ou conexão no GitHub**, `.csproj` ou `appsettings.json`.

## 2. Railway: variáveis privadas

No serviço **Petrobras**, acesse **Variáveis** e configure:

- `ConnectionStrings__PetrobrasContext` = a string ADO.NET acima.
- `SIAF_INITIALIZE_DATABASE` = `true` na PRIMEIRA inicialização apenas.
- `SIAF_BOOTSTRAP_KEY` = uma chave longa e aleatória (valor secreto, definido por você).
- `PORT` = `8080` (se o Railway solicitar).

Aplique as alterações pendentes para que a implantação as utilize.
O `Dockerfile` da raiz do repositório já está preparado para .NET 10.

## 3. Inicialização das tabelas

Na primeira implantação com `SIAF_INITIALIZE_DATABASE=true`, o programa:

1. Verifica se já há tabelas no esquema `siaf`.
2. Caso esteja vazio, cria as **10 tabelas do SIAF** usando o modelo EF.
3. Insere os perfis `Administrador`, `Analista`, `Gestor` e `Operador`.

O esquema `siaf` não é exposto por padrão na API REST do Supabase,
sem necessidade de dar acesso público às tabelas. **Não adicione `siaf` à lista de esquemas
expostos do Supabase.** O site acessa os dados pelo servidor ASP.NET.

Assim que as tabelas forem criadas, mude `SIAF_INITIALIZE_DATABASE` para `false`
no Railway e aplique as alterações. Isso evita verificações e escritas na inicialização futura.

> Importante: se o esquema `siaf` já contém tabelas ou ficou parcialmente criado,
> não apague os dados. Faça backup e revise antes de repetir a inicialização.
> O mecanismo inicial não substitui migrações do Entity Framework para mudanças futuras.

## 4. Criar o primeiro administrador

Acesse o endereço HTTPS do Railway e clique em **Criar administrador** na tela de login.
Digite dados de **demonstração** e informe a chave `SIAF_BOOTSTRAP_KEY`.
O primeiro usuário sempre recebe o perfil `Administrador`.
Após o primeiro cadastro, novos usuários só podem ser criados por administradores.

Recomenda-se remover `SIAF_BOOTSTRAP_KEY` após criar o primeiro administrador.
O login utiliza senhas com hash e cookie de autenticação.

## 5. Conferências

- O site abre e apresenta a tela de login.
- Usuário anônimo é redirecionado ao login quando acessa `/Equipamento`.
- O administrador faz login e pode criar setor, usuário e equipamento.
- Os cadastros continuam salvos após atualizar a página.
- Usuário sem perfil `Administrador` não consegue acessar `/Usuario` nem `/Perfil`.

## Limitações

- Não há importação automática do SQL Server: requer migração separada dos registros.
- Este código usa criação inicial do esquema, **não migrações incrementais**.
- Sem acesso à sua senha de banco, não foi possível testar a conexão Supabase.
- Cookies são inválidos quando o contêiner é recriado, pois a chave de proteção do ASP.NET
  não está persistida. É necessário fazer login novamente após alguns reinícios.
- Para produção com usuários reais, revisar privacidade, autorização por recurso,
  auditoria, backups, armazenamento de anexos e persistência de chaves.

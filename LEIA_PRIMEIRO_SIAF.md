# SIAF — pacote integrado de atualização (12/11/2026)

## O QUE ESTE PACOTE ALTERA

Sem trocar a identidade visual nem apagar tabelas existentes:
- Fluxo de registro de falhas industriais, pesquisa e filtros por equipamento, período, situação e texto;
- Análise técnica com método e causa raiz, atribuição a gestor e ações corretivas com prazo e responsável;
- Aprovação/devolução pelo gestor, parecer, revisão da causa e acompanhamento da execução;
- Controle de acesso em cada rota; operadores registram, analistas investigam, gestores aprovam e administradores gerenciam cadastros;
- Cadastro de funcionários com senha protegida, impedindo gestores de criarem administradores;
- Evidências PDF/PNG/JPG até 5MB persistidas em tabela PostgreSQL privada, sem disco efêmero;
- Notificações persistidas no banco, contador atualizado a cada 15 segundos (sem WebSocket);
- Auditoria automática das operações centrais, com antes/depois para alguns campos;
- Painel com indicadores reais do PostgreSQL e relatório PDF básico exportável;
- Bloqueio das rotas de escrita dos cadastros antigos de Falha, Analise, CausaRaiz, AcaoCorretiva e Anexo.

## NÃO CONFUNDIR COM HOMOLOGAÇÃO

Este pacote é uma implementação substancial, não garantia de que os 13 RF e os 10 RNF foram homologados.
Necessita de compilação pela Action, implantação e testes reais com Supabase.
Limitações conhecidas:
- Notificações atualizadas a cada 15s, não via SignalR/WebSocket;
- Modo de falha tem referência descritiva à ISO 14224, não catálogo completo validado pela norma;
- MTBF exige tempo de operação entre falhas, que ainda não é coletado;
- PDF contém resumo e até 250 ocorrências; impressão web oferece mais opções;
- Histórico cobre operações novas mais importantes; ações legadas fora do fluxo não são reescritas retroativamente;
- Não foi adicionada API JWT nem backup automático; priorizamos o fluxo MVC com cookies;
- Anexos antigos, se eram apenas caminhos de arquivos, não recebem conteúdo automaticamente;
- Arquivos de 5MB armazenados em bytea podem ampliar o custo/armazenamento do Supabase;
- Sem contas e dados reais, não foi possível executar E2E, comprovar conformidade legal ou validar cada permissão em produção.

## INSTALAÇÃO RÁPIDA NO GITHUB CODESPACES

### Passo A — Atualizar o repositório (ANTES de publicar)
1. Abra o Codespaces na raiz do repositório `pedrolvieira7/petrobras`.
2. Envie `SIAF_ATUALIZACAO_COMPLETA.zip` para essa pasta (não precisa abrir cada C#).
3. No terminal, execute:
   ```bash
   unzip -o SIAF_ATUALIZACAO_COMPLETA.zip -d .siaf-atualizacao
   bash .siaf-atualizacao/aplicar.sh
   ```
4. O script verifica a pasta, exige alterações existentes salvas, cria a ramificação
   `atualizacao-siaf-completo-20261112`, copia os arquivos, tenta `dotnet publish`,
   cria commit e envia a ramificação ao GitHub. **Não mescla na main.**
5. Abra o link que o script mostrar e crie uma Pull Request contra `main`.
6. Aguarde a Action `Validar SIAF (.NET 10)` ficar verde. Se falhar, não mescle.

### Passo B — Preparar o Supabase (antes de mesclar PR)
1. Faça um BACKUP de seu banco Supabase.
2. Abra Supabase > SQL Editor.
3. Copie o conteúdo de `sql/01_atualizar_siaf_sem_apagar_dados.sql` do repositório e execute.
4. A consulta final deve indicar 12 tabelas no schema `siaf` (10 iniciais + 2 novas).
   Caso conte diferente, confira o motivo antes de publicar.
5. **NÃO execute o script inicial que recria o banco, não apague tabelas, não exponha o schema `siaf` na API REST.**

### Passo C — Railway
1. Confira variáveis, mantendo segredos SOMENTE no Railway:
   - `ConnectionStrings__PetrobrasContext` — conexão privada do Supabase;
   - `SIAF_INITIALIZE_DATABASE=false` — o esquema foi migrado pelo SQL acima;
   - `SIAF_BOOTSTRAP_KEY` — necessária somente antes do primeiro administrador.
2. Mescle a Pull Request no GitHub apenas depois de passar na Action e executar o SQL.
3. Acompanhe a implantação no Railway.
4. Teste o login, usuários de perfis diferentes e o fluxo completo em dados de demonstração.
5. Após criar o primeiro administrador, remova `SIAF_BOOTSTRAP_KEY` das variáveis.

## SE APARECER ERRO
- `Chave inicial inválida`: configure a `SIAF_BOOTSTRAP_KEY` no Railway e informe EXATAMENTE a mesma no primeiro cadastro.
- `relation siaf.notificacao does not exist`: o SQL de migração não foi executado no banco correto.
- `Falha na compilação`: envie o log da Action do GitHub, sem senhas.
- `403 Forbidden`: a conta não possui o papel necessário para a operação.
- `Não consigo cadastrar equipamento`: faça login como Administrador e cadastre primeiro Setor e Funcionário.

## TESTE DE ACEITAÇÃO
1. Cadastrar primeiro administrador via chave (uma vez) com dados fictícios.
2. Cadastrar perfis Gestor, Analista, Operador pela seção Funcionários.
3. Como Administrador, cadastrar Setor e Equipamento.
4. Como Operador, registrar uma falha e conferir filtros.
5. Como Analista, abrir o detalhe, registrar causa raiz e ação com prazo.
6. Como Gestor designado, abrir o detalhe, devolver com parecer, conferir notificação.
7. Como Analista, revisar e reenviar; como Gestor, aprovar.
8. Como responsável, atualizar ação para Concluída e verificar situação final.
9. Enviar PDF pequeno de demonstração e baixar o mesmo documento.
10. Conferir os indicadores reais, relatórios, auditoria e bloqueio de acesso por perfil.

**Não use dados pessoais reais ou documentos industriais confidenciais sem uma revisão adicional de segurança e privacidade.**

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;
using PetrobrasFInin.Services;
using PetrobrasFInin.ViewModels;

namespace PetrobrasFInin.Controllers;

[Authorize]
public class OcorrenciasController : Controller
{
    private readonly PetrobrasContext _db;
    public OcorrenciasController(PetrobrasContext db) => _db = db;

    private bool Tecnico => User.IsInRole("Analista") || User.IsInRole("Administrador");
    private bool Gestao => User.IsInRole("Gestor") || User.IsInRole("Administrador");
    private static DateTime Agora() => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

    public async Task<IActionResult> Index(string? busca, int? equipamento, string? situacao,
        DateTime? inicio, DateTime? fim)
    {
        var consulta = _db.Falhas.Include(x => x.IdEquipamentoNavigation)
            .AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(f => f.CodigoFalha.Contains(termo) ||
                f.TituloFalha.Contains(termo) || (f.Descricao != null && f.Descricao.Contains(termo)));
        }
        if (equipamento.HasValue) consulta = consulta.Where(f => f.IdEquipamento == equipamento.Value);
        if (!string.IsNullOrWhiteSpace(situacao)) consulta = consulta.Where(f => f.Situacao == situacao);
        if (inicio.HasValue) consulta = consulta.Where(f => f.DataOcorrencia >= inicio.Value.Date);
        if (fim.HasValue) consulta = consulta.Where(f => f.DataOcorrencia < fim.Value.Date.AddDays(1));
        ViewBag.Equipamentos = new SelectList(await _db.Equipamentos.AsNoTracking()
            .OrderBy(x => x.NomeEquipamento).ToListAsync(), "IdEquipamento", "NomeEquipamento", equipamento);
        ViewBag.Busca = busca;
        ViewBag.Situacao = situacao;
        ViewBag.Inicio = inicio?.ToString("yyyy-MM-dd");
        ViewBag.Fim = fim?.ToString("yyyy-MM-dd");
        return View(await consulta.OrderByDescending(x => x.DataOcorrencia).Take(500).ToListAsync());
    }

    public async Task<IActionResult> Detalhes(int id)
    {
        var falha = await _db.Falhas.AsNoTracking().AsSplitQuery()
            .Include(x => x.IdEquipamentoNavigation)
            .Include(x => x.IdUsuarioNavigation)
            .Include(x => x.Analises).ThenInclude(a => a.CausaRaizs)
            .Include(x => x.Analises).ThenInclude(a => a.AcaoCorretivas)
            .Include(x => x.Analises).ThenInclude(a => a.IdAnalistaNavigation)
            .Include(x => x.Analises).ThenInclude(a => a.IdGestorNavigation)
            .FirstOrDefaultAsync(x => x.IdFalha == id);
        if (falha == null) return NotFound();
        ViewBag.Anexos = await _db.Anexos.AsNoTracking()
            .Where(x => x.IdFalha == id || (x.IdAnalise != null &&
                x.IdAnaliseNavigation!.IdFalha == id))
            .OrderByDescending(x => x.IdAnexo).ToListAsync();
        return View(falha);
    }

    [HttpGet]
    public async Task<IActionResult> NovaFalha()
    {
        await Equipamentos();
        return View(new RegistroFalhaVm { DataOcorrencia = DateTime.Now });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NovaFalha(RegistroFalhaVm model)
    {
        if (!new[] { "Baixa", "Média", "Alta", "Crítica" }.Contains(model.Criticidade))
            ModelState.AddModelError(nameof(model.Criticidade), "Criticidade inválida.");
        if (!await _db.Equipamentos.AnyAsync(x => x.IdEquipamento == model.IdEquipamento))
            ModelState.AddModelError(nameof(model.IdEquipamento), "Equipamento inexistente.");
        if (await _db.Falhas.AnyAsync(x => x.CodigoFalha == model.CodigoFalha))
            ModelState.AddModelError(nameof(model.CodigoFalha), "Código da falha já existe.");
        int? autor = RegistroSiaf.UsuarioId(User);
        if (autor == null) return Challenge();
        if (!ModelState.IsValid)
        {
            await Equipamentos();
            return View(model);
        }
        var falha = new Falha
        {
            CodigoFalha = model.CodigoFalha.Trim(), TituloFalha = model.TituloFalha.Trim(),
            Descricao = model.Descricao.Trim(), ModoFalha = model.ModoFalha.Trim(),
            DataOcorrencia = DateTime.SpecifyKind(model.DataOcorrencia, DateTimeKind.Unspecified),
            Criticidade = model.Criticidade, HorasParada = model.HorasParada,
            Situacao = "Aberta", IdEquipamento = model.IdEquipamento, IdUsuario = autor.Value
        };
        await using var tx = await _db.Database.BeginTransactionAsync();
        _db.Falhas.Add(falha);
        await _db.SaveChangesAsync();
        RegistroSiaf.Auditar(_db, User, "Criar falha", $"falha:{falha.IdFalha}",
            depois: new { falha.CodigoFalha, falha.TituloFalha, falha.Criticidade, falha.Situacao });
        var destinatarios = await _db.Usuarios.Where(x =>
            x.IdPerfilNavigation.Nome == "Analista" || x.IdPerfilNavigation.Nome == "Gestor")
            .Select(x => x.IdUsuario).ToListAsync();
        foreach (int id in destinatarios.Where(id => id != autor.Value))
            RegistroSiaf.Notificar(_db, id, "Nova falha registrada", falha.CodigoFalha,
                $"/Ocorrencias/Detalhes/{falha.IdFalha}");
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        TempData["Sucesso"] = "Falha registrada e encaminhada para acompanhamento.";
        return RedirectToAction(nameof(Detalhes), new { id = falha.IdFalha });
    }

    [Authorize(Roles = "Analista,Administrador")]
    [HttpGet]
    public async Task<IActionResult> NovaAnalise(int idFalha)
    {
        if (!await _db.Falhas.AnyAsync(x => x.IdFalha == idFalha)) return NotFound();
        await Gestores();
        ViewBag.Falha = await _db.Falhas.AsNoTracking().FirstAsync(x => x.IdFalha == idFalha);
        return View(new RegistroAnaliseVm { IdFalha = idFalha });
    }

    [Authorize(Roles = "Analista,Administrador")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NovaAnalise(RegistroAnaliseVm model)
    {
        int? autor = RegistroSiaf.UsuarioId(User);
        if (autor == null) return Challenge();
        var falha = await _db.Falhas.FindAsync(model.IdFalha);
        if (falha == null) return NotFound();
        if (falha.Situacao == "Concluída") return BadRequest("Falha concluída.");
        bool gestorValido = await _db.Usuarios.AnyAsync(x => x.IdUsuario == model.IdGestor &&
            (x.IdPerfilNavigation.Nome == "Gestor" || x.IdPerfilNavigation.Nome == "Administrador"));
        if (!gestorValido) ModelState.AddModelError(nameof(model.IdGestor), "Selecione um gestor válido.");
        if (await _db.Analises.AnyAsync(x => x.CodigoAnalise == model.CodigoAnalise))
            ModelState.AddModelError(nameof(model.CodigoAnalise), "Código de análise já existe.");
        if (!ModelState.IsValid)
        {
            await Gestores();
            ViewBag.Falha = falha;
            return View(model);
        }
        await using var tx = await _db.Database.BeginTransactionAsync();
        var analise = new Analise
        {
            CodigoAnalise = model.CodigoAnalise.Trim(), MetodoAnalise = model.MetodoAnalise.Trim(),
            Disciplina = model.Disciplina.Trim(), DecisaoGestor = "Pendente",
            IdFalha = falha.IdFalha, IdAnalista = autor.Value, IdGestor = model.IdGestor
        };
        _db.Analises.Add(analise);
        await _db.SaveChangesAsync();
        _db.CausaRaizs.Add(new CausaRaiz
        {
            IdAnalise = analise.IdAnalise, CausaPrincipal = model.CausaPrincipal.Trim(),
            Categoria = model.Categoria.Trim(), TipoCausa = model.TipoCausa.Trim()
        });
        falha.Situacao = "Em análise";
        RegistroSiaf.Auditar(_db, User, "Criar análise", $"analise:{analise.IdAnalise}",
            depois: new { analise.CodigoAnalise, model.CausaPrincipal });
        RegistroSiaf.Notificar(_db, model.IdGestor, "Análise iniciada",
            $"Análise {analise.CodigoAnalise} em andamento.", $"/Ocorrencias/Detalhes/{falha.IdFalha}");
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        TempData["Sucesso"] = "Análise e causa registradas. Crie um plano corretivo para aprovação.";
        return RedirectToAction(nameof(Detalhes), new { id = falha.IdFalha });
    }

    [Authorize(Roles = "Analista,Administrador")]
    [HttpGet]
    public async Task<IActionResult> NovaAcao(int idAnalise)
    {
        var analise = await _db.Analises.AsNoTracking().FirstOrDefaultAsync(x => x.IdAnalise == idAnalise);
        if (analise == null) return NotFound();
        if (User.IsInRole("Analista") && !User.IsInRole("Administrador") &&
            RegistroSiaf.UsuarioId(User) != analise.IdAnalista) return Forbid();
        if (analise.DecisaoGestor == "Aprovada") return BadRequest("Análise já aprovada.");
        await Responsaveis();
        return View(new RegistroAcaoVm { IdAnalise = idAnalise, Prazo = DateTime.Now.AddDays(7) });
    }

    [Authorize(Roles = "Analista,Administrador")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NovaAcao(RegistroAcaoVm model)
    {
        var analise = await _db.Analises.Include(x => x.IdFalhaNavigation)
            .FirstOrDefaultAsync(x => x.IdAnalise == model.IdAnalise);
        if (analise == null) return NotFound();
        if (User.IsInRole("Analista") && !User.IsInRole("Administrador") &&
            RegistroSiaf.UsuarioId(User) != analise.IdAnalista) return Forbid();
        if (analise.DecisaoGestor == "Aprovada") return BadRequest("Análise já aprovada.");
        if (!await _db.Usuarios.AnyAsync(x => x.IdUsuario == model.IdResponsavel))
            ModelState.AddModelError(nameof(model.IdResponsavel), "Responsável inválido.");
        if (model.Prazo.Date < DateTime.Today)
            ModelState.AddModelError(nameof(model.Prazo), "O prazo não pode estar no passado.");
        if (await _db.AcaoCorretivas.AnyAsync(x => x.CodigoAcao == model.CodigoAcao))
            ModelState.AddModelError(nameof(model.CodigoAcao), "Código da ação já existe.");
        if (!ModelState.IsValid)
        {
            await Responsaveis();
            return View(model);
        }
        await using var tx = await _db.Database.BeginTransactionAsync();
        var acao = new AcaoCorretiva
        {
            IdAnalise = analise.IdAnalise, CodigoAcao = model.CodigoAcao.Trim(),
            DescricaoAcao = model.DescricaoAcao.Trim(),
            Prazo = DateTime.SpecifyKind(model.Prazo, DateTimeKind.Unspecified),
            Situacao = "Pendente", IdResponsavel = model.IdResponsavel
        };
        _db.AcaoCorretivas.Add(acao);
        analise.DecisaoGestor = "Pendente";
        analise.IdFalhaNavigation.Situacao = "Aguardando aprovação";
        await _db.SaveChangesAsync();
        RegistroSiaf.Auditar(_db, User, "Criar ação", $"acao:{acao.IdAcao}",
            depois: new { acao.CodigoAcao, acao.Prazo, acao.IdResponsavel });
        RegistroSiaf.Notificar(_db, analise.IdGestor, "Plano aguardando avaliação",
            $"Plano {acao.CodigoAcao} está pronto para avaliação.",
            $"/Ocorrencias/Detalhes/{analise.IdFalha}");
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        return RedirectToAction(nameof(Detalhes), new { id = analise.IdFalha });
    }

    [Authorize(Roles = "Gestor,Administrador")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Avaliar(int idAnalise, string decisao, string? parecer)
    {
        var analise = await _db.Analises.Include(x => x.IdFalhaNavigation)
            .Include(x => x.CausaRaizs).Include(x => x.AcaoCorretivas)
            .FirstOrDefaultAsync(x => x.IdAnalise == idAnalise);
        if (analise == null) return NotFound();
        if (!User.IsInRole("Administrador") && RegistroSiaf.UsuarioId(User) != analise.IdGestor)
            return Forbid();
        if (decisao != "Aprovada" && decisao != "Devolvida") return BadRequest("Decisão inválida.");
        if (decisao == "Devolvida" && string.IsNullOrWhiteSpace(parecer))
        {
            TempData["Erro"] = "Informe um parecer para devolver a análise.";
            return RedirectToAction(nameof(Detalhes), new { id = analise.IdFalha });
        }
        if (decisao == "Aprovada" && (!analise.CausaRaizs.Any() || !analise.AcaoCorretivas.Any()))
        {
            TempData["Erro"] = "A análise precisa ter causa raiz e pelo menos um plano corretivo.";
            return RedirectToAction(nameof(Detalhes), new { id = analise.IdFalha });
        }
        if (analise.DecisaoGestor == "Aprovada") return BadRequest("Esta análise já foi aprovada.");
        var antes = new { analise.DecisaoGestor, analise.ParecerGestor };
        analise.DecisaoGestor = decisao;
        analise.ParecerGestor = parecer?.Trim();
        analise.IdFalhaNavigation.Situacao = decisao == "Aprovada" ? "Em correção" : "Em revisão";
        RegistroSiaf.Auditar(_db, User, "Decisão do gestor", $"analise:{analise.IdAnalise}",
            antes, new { analise.DecisaoGestor, analise.ParecerGestor });
        RegistroSiaf.Notificar(_db, analise.IdAnalista, decisao == "Aprovada" ? "Plano aprovado" : "Plano devolvido",
            parecer?.Trim() is { Length: > 0 } p ? p : $"Análise {analise.CodigoAnalise}: {decisao}.",
            $"/Ocorrencias/Detalhes/{analise.IdFalha}");
        if (decisao == "Aprovada")
        {
            foreach (var id in analise.AcaoCorretivas.Select(x => x.IdResponsavel).Distinct())
                RegistroSiaf.Notificar(_db, id, "Plano corretivo aprovado",
                    $"A análise {analise.CodigoAnalise} foi aprovada. Execute a ação atribuída.",
                    $"/Ocorrencias/Detalhes/{analise.IdFalha}");
        }
        await _db.SaveChangesAsync();
        TempData["Sucesso"] = decisao == "Aprovada" ? "Plano aprovado." : "Análise devolvida ao analista.";
        return RedirectToAction(nameof(Detalhes), new { id = analise.IdFalha });
    }

    [Authorize(Roles = "Analista,Administrador")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Revisar(int idAnalise, string causaPrincipal, string tipoCausa)
    {
        var analise = await _db.Analises.Include(x => x.CausaRaizs)
            .Include(x => x.IdFalhaNavigation).Include(x => x.AcaoCorretivas)
            .FirstOrDefaultAsync(x => x.IdAnalise == idAnalise);
        if (analise == null) return NotFound();
        if (!User.IsInRole("Administrador") && RegistroSiaf.UsuarioId(User) != analise.IdAnalista)
            return Forbid();
        if (analise.DecisaoGestor != "Devolvida") return BadRequest("Somente análises devolvidas podem ser revisadas.");
        if (string.IsNullOrWhiteSpace(causaPrincipal) || causaPrincipal.Length > 100 ||
            string.IsNullOrWhiteSpace(tipoCausa) || tipoCausa.Length > 80)
            return BadRequest("Informe causa e tipo válidos.");
        var causa = analise.CausaRaizs.FirstOrDefault();
        if (causa == null) return BadRequest("A análise não possui causa cadastrada.");
        var anterior = new { causa.CausaPrincipal, causa.TipoCausa, analise.DecisaoGestor };
        causa.CausaPrincipal = causaPrincipal.Trim();
        causa.TipoCausa = tipoCausa.Trim();
        analise.DecisaoGestor = "Pendente";
        analise.IdFalhaNavigation.Situacao = analise.AcaoCorretivas.Any()
            ? "Aguardando aprovação" : "Em análise";
        RegistroSiaf.Auditar(_db, User, "Revisar análise", $"analise:{idAnalise}", anterior,
            new { causa.CausaPrincipal, causa.TipoCausa, analise.DecisaoGestor });
        RegistroSiaf.Notificar(_db, analise.IdGestor, "Análise revisada",
            $"Análise {analise.CodigoAnalise} foi revisada e aguarda nova avaliação.",
            $"/Ocorrencias/Detalhes/{analise.IdFalha}");
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Detalhes), new { id = analise.IdFalha });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarAcao(int id, string situacao)
    {
        var acao = await _db.AcaoCorretivas
            .Include(x => x.IdAnaliseNavigation).ThenInclude(x => x.IdFalhaNavigation)
            .FirstOrDefaultAsync(x => x.IdAcao == id);
        if (acao == null) return NotFound();
        if (RegistroSiaf.UsuarioId(User) != acao.IdResponsavel && !Gestao) return Forbid();
        if (acao.IdAnaliseNavigation.DecisaoGestor != "Aprovada")
            return BadRequest("A ação só pode ser executada após aprovação.");
        if (!new[] { "Pendente", "Em andamento", "Concluída" }.Contains(situacao))
            return BadRequest("Situação inválida.");
        var antes = acao.Situacao;
        acao.Situacao = situacao;
        if (situacao == "Concluída")
        {
            bool temPendencias = await _db.AcaoCorretivas.AnyAsync(x =>
                x.IdAnaliseNavigation.IdFalha == acao.IdAnaliseNavigation.IdFalha &&
                x.IdAcao != id && x.Situacao != "Concluída");
            if (!temPendencias)
                acao.IdAnaliseNavigation.IdFalhaNavigation.Situacao = "Concluída";
        }
        else if (acao.IdAnaliseNavigation.IdFalhaNavigation.Situacao == "Concluída")
            acao.IdAnaliseNavigation.IdFalhaNavigation.Situacao = "Em correção";
        RegistroSiaf.Auditar(_db, User, "Atualizar ação", $"acao:{id}",
            new { Situacao = antes }, new { acao.Situacao });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Detalhes), new { id = acao.IdAnaliseNavigation.IdFalha });
    }

    private async Task Equipamentos()
    {
        ViewBag.Equipamentos = new SelectList(await _db.Equipamentos.AsNoTracking()
            .OrderBy(x => x.NomeEquipamento).ToListAsync(), "IdEquipamento", "NomeEquipamento");
    }
    private async Task Gestores()
    {
        ViewBag.Gestores = new SelectList(await _db.Usuarios.AsNoTracking()
            .Where(x => x.IdPerfilNavigation.Nome == "Gestor" || x.IdPerfilNavigation.Nome == "Administrador")
            .OrderBy(x => x.NomeCompleto).ToListAsync(), "IdUsuario", "NomeCompleto");
    }
    private async Task Responsaveis()
    {
        ViewBag.Responsaveis = new SelectList(await _db.Usuarios.AsNoTracking()
            .OrderBy(x => x.NomeCompleto).ToListAsync(), "IdUsuario", "NomeCompleto");
    }
}

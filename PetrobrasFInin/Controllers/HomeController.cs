using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;
using PetrobrasFInin.Services;
using PetrobrasFInin.ViewModels;
using System.Diagnostics;

namespace PetrobrasFInin.Controllers;

public class HomeController : Controller
{
    private readonly PetrobrasContext _db;
    public HomeController(PetrobrasContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        return View(await Painel());
    }

    public async Task<IActionResult> Relatorios()
    {
        return View(await Painel());
    }

    [Authorize(Roles="Administrador,Gestor")]
    public async Task<IActionResult> ExportarPdf()
    {
        var painel = await Painel();
        var falhas = await _db.Falhas.AsNoTracking().Include(x => x.IdEquipamentoNavigation)
            .OrderByDescending(x => x.DataOcorrencia).Take(250).ToListAsync();
        var linhas = new List<string> {
            "SIAF - RELATORIO CONSOLIDADO DE OCORRENCIAS",
            $"Emitido: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC",
            $"Ocorrencias: {painel.TotalFalhas} | Concluidas: {painel.Concluidas} | Em analise: {painel.EmAnalise}",
            $"Alta / critica: {painel.Altas} | Acoes: {painel.Acoes} | Equipamentos: {painel.Equipamentos}",
            new string('-',90),
            "CODIGO | DATA | EQUIPAMENTO | CRITICIDADE | SITUACAO"
        };
        linhas.AddRange(falhas.Select(f => $"{f.CodigoFalha} | {f.DataOcorrencia:dd/MM/yyyy} | {f.IdEquipamentoNavigation.NomeEquipamento} | {f.Criticidade} | {f.Situacao}"));
        if (falhas.Count == 250) linhas.Add("Listagem limitada as 250 ocorrencias mais recentes.");
        return File(PdfSimples.Gerar(linhas), "application/pdf", $"SIAF_Relatorio_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    public IActionResult Privacy() => View();
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    private async Task<SiafPainelVm> Painel()
    {
        return new SiafPainelVm
        {
            TotalFalhas = await _db.Falhas.CountAsync(),
            EmAnalise = await _db.Falhas.CountAsync(x => x.Situacao == "Em análise" || x.Situacao == "Em revisão" || x.Situacao == "Aguardando aprovação"),
            Concluidas = await _db.Falhas.CountAsync(x => x.Situacao == "Concluída"),
            Altas = await _db.Falhas.CountAsync(x => x.Criticidade == "Alta" || x.Criticidade == "Crítica"),
            Acoes = await _db.AcaoCorretivas.CountAsync(),
            Equipamentos = await _db.Equipamentos.CountAsync(),
            Pendentes = await _db.Analises.CountAsync(x => x.DecisaoGestor == "Pendente"),
            Recentes = await _db.Falhas.AsNoTracking().Include(x => x.IdEquipamentoNavigation)
                .OrderByDescending(x => x.DataOcorrencia).Take(10).ToListAsync()
        };
    }
}

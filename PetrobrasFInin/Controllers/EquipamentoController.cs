using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Controllers;

public class EquipamentoController : Controller
{
    private readonly PetrobrasContext _context;
    private readonly ILogger<EquipamentoController> _logger;

    public EquipamentoController(PetrobrasContext context, ILogger<EquipamentoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private async Task CarregarListas()
    {
        ViewData["IdSetor"] = new SelectList(
            await _context.Set<Setor>().AsNoTracking().OrderBy(x => x.Unidade).ToListAsync(),
            "IdSetor", "Unidade");

        ViewData["IdResponsavel"] = new SelectList(
            await _context.Set<Usuario>().AsNoTracking().OrderBy(x => x.NomeCompleto).ToListAsync(),
            "IdUsuario", "NomeCompleto");
    }

    public async Task<IActionResult> Index()
    {
        var equipamentos = await _context.Equipamentos
            .AsNoTracking()
            .OrderBy(x => x.NomeEquipamento)
            .ToListAsync();

        return View(equipamentos);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.Equipamentos
            .AsNoTracking()
            .Include(x => x.IdSetorNavigation)
            .Include(x => x.IdResponsavelNavigation)
            .FirstOrDefaultAsync(x => x.IdEquipamento == id);

        if (item == null) return NotFound();
        return View(item);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarListas();

        if (!await _context.Set<Setor>().AnyAsync())
            ModelState.AddModelError(string.Empty, "Cadastre pelo menos um setor antes de cadastrar um equipamento.");

        if (!await _context.Set<Usuario>().AnyAsync())
            ModelState.AddModelError(string.Empty, "Cadastre pelo menos um usuário responsável antes de cadastrar um equipamento.");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Tag,NomeEquipamento,Tipo,Criticidade,Situacao,IdSetor,IdResponsavel")] Equipamento item)
    {
        ModelState.Remove(nameof(Equipamento.IdSetorNavigation));
        ModelState.Remove(nameof(Equipamento.IdResponsavelNavigation));
        ModelState.Remove(nameof(Equipamento.Falhas));

        if (ModelState.IsValid)
        {
            var setorExiste = await _context.Set<Setor>().AnyAsync(x => x.IdSetor == item.IdSetor);
            var responsavelExiste = await _context.Set<Usuario>().AnyAsync(x => x.IdUsuario == item.IdResponsavel);

            if (!setorExiste)
                ModelState.AddModelError(nameof(item.IdSetor), "Selecione um setor válido.");
            if (!responsavelExiste)
                ModelState.AddModelError(nameof(item.IdResponsavel), "Selecione um responsável válido.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Equipamentos.Add(item);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Equipamento cadastrado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao salvar equipamento.");
                ModelState.AddModelError(string.Empty, "O banco de dados recusou o cadastro. Confira se a Tag já existe e se os campos informados são compatíveis com as colunas do banco.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao salvar equipamento.");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro inesperado ao salvar o equipamento. Consulte a janela Saída do Visual Studio.");
            }
        }

        await CarregarListas();
        return View(item);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.Equipamentos.FindAsync(id);
        if (item == null) return NotFound();

        await CarregarListas();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdEquipamento,Tag,NomeEquipamento,Tipo,Criticidade,Situacao,IdSetor,IdResponsavel")] Equipamento item)
    {
        if (id != item.IdEquipamento) return NotFound();

        ModelState.Remove(nameof(Equipamento.IdSetorNavigation));
        ModelState.Remove(nameof(Equipamento.IdResponsavelNavigation));
        ModelState.Remove(nameof(Equipamento.Falhas));

        if (ModelState.IsValid)
        {
            var setorExiste = await _context.Set<Setor>().AnyAsync(x => x.IdSetor == item.IdSetor);
            var responsavelExiste = await _context.Set<Usuario>().AnyAsync(x => x.IdUsuario == item.IdResponsavel);

            if (!setorExiste)
                ModelState.AddModelError(nameof(item.IdSetor), "Selecione um setor válido.");
            if (!responsavelExiste)
                ModelState.AddModelError(nameof(item.IdResponsavel), "Selecione um responsável válido.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Equipamentos.Update(item);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Equipamento atualizado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao atualizar equipamento.");
                ModelState.AddModelError(string.Empty, "O banco de dados recusou a alteração. Confira os dados e os relacionamentos selecionados.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao atualizar equipamento.");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro inesperado ao atualizar o equipamento. Consulte a janela Saída do Visual Studio.");
            }
        }

        await CarregarListas();
        return View(item);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var item = await _context.Equipamentos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdEquipamento == id);

        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Equipamentos.FindAsync(id);
        if (item != null)
        {
            try
            {
                _context.Equipamentos.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Equipamento excluído com sucesso.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao excluir equipamento.");
                TempData["Erro"] = "Não foi possível excluir este equipamento porque ele pode estar vinculado a falhas ou análises.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}

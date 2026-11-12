using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Controllers;

[Authorize(Roles = "Administrador,Gestor")]
public class PerfilController : Controller
{
    private readonly PetrobrasContext _context;
    private static readonly string[] PerfisPrincipais =
        { "Administrador", "Gestor", "Analista", "Operador" };

    public PerfilController(PetrobrasContext context) => _context = context;

    private static bool EhPrincipal(string nome) =>
        PerfisPrincipais.Contains(nome, StringComparer.OrdinalIgnoreCase);

    public async Task<IActionResult> Index() =>
        View(await _context.Perfils.AsNoTracking().OrderBy(p => p.Nome).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Perfils.AsNoTracking().FirstOrDefaultAsync(x => x.IdPerfil == id);
        return item == null ? NotFound() : View(item);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public IActionResult Create() => View();

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Descricao")] Perfil item)
    {
        item.Nome = item.Nome?.Trim() ?? "";
        item.Descricao = item.Descricao?.Trim() ?? "";
        if (await _context.Perfils.AnyAsync(p => p.Nome == item.Nome))
            ModelState.AddModelError(nameof(item.Nome), "Já existe um perfil com este nome.");
        if (!ModelState.IsValid) return View(item);
        _context.Perfils.Add(item);
        try
        {
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Não foi possível criar o perfil.");
            return View(item);
        }
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Perfils.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdPerfil,Nome,Descricao")] Perfil item)
    {
        if (id != item.IdPerfil) return NotFound();
        var existente = await _context.Perfils.FindAsync(id);
        if (existente == null) return NotFound();
        item.Nome = item.Nome?.Trim() ?? "";
        item.Descricao = item.Descricao?.Trim() ?? "";
        if (EhPrincipal(existente.Nome) && item.Nome != existente.Nome)
            ModelState.AddModelError(nameof(item.Nome), "Não altere o nome de um perfil principal.");
        if (await _context.Perfils.AnyAsync(p => p.IdPerfil != id && p.Nome == item.Nome))
            ModelState.AddModelError(nameof(item.Nome), "Já existe outro perfil com esse nome.");
        if (!ModelState.IsValid) return View(item);
        existente.Nome = item.Nome;
        existente.Descricao = item.Descricao;
        try
        {
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Não foi possível atualizar o perfil.");
            return View(item);
        }
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Perfils.FindAsync(id);
        if (item == null) return NotFound();
        if (EhPrincipal(item.Nome)) return BadRequest("Não é possível excluir um perfil principal.");
        return View(item);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Perfils.FindAsync(id);
        if (item == null) return RedirectToAction(nameof(Index));
        if (EhPrincipal(item.Nome) || await _context.Usuarios.AnyAsync(u => u.IdPerfil == id))
        {
            TempData["Erro"] = "O perfil é principal ou está associado a funcionários.";
            return RedirectToAction(nameof(Index));
        }
        try
        {
            _context.Perfils.Remove(item);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Erro"] = "Não foi possível excluir este perfil.";
        }
        return RedirectToAction(nameof(Index));
    }
}

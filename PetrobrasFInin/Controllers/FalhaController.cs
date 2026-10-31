using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Controllers;

public class FalhaController : Controller
{
    private readonly PetrobrasContext _context;

    public FalhaController(PetrobrasContext context)
    {
        _context = context;
    }

    private async Task CarregarListas()
    {
        ViewData["IdEquipamento"] = new SelectList(await _context.Set<Equipamento>().ToListAsync(), "IdEquipamento", "NomeEquipamento");
        ViewData["IdUsuario"] = new SelectList(await _context.Set<Usuario>().ToListAsync(), "IdUsuario", "NomeCompleto");
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Set<Falha>().ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Set<Falha>().FirstOrDefaultAsync(x => x.IdFalha == id);
        if (item == null) return NotFound();
        return View(item);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarListas();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CodigoFalha,TituloFalha,DataOcorrencia,Criticidade,Situacao,IdEquipamento,IdUsuario")] Falha item)
    {
        if (!ModelState.IsValid)
        {
            await CarregarListas();
            return View(item);
        }

        _context.Add(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Set<Falha>().FindAsync(id);
        if (item == null) return NotFound();
        await CarregarListas();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdFalha,CodigoFalha,TituloFalha,DataOcorrencia,Criticidade,Situacao,IdEquipamento,IdUsuario")] Falha item)
    {
        if (id != item.IdFalha) return NotFound();
        if (!ModelState.IsValid)
        {
            await CarregarListas();
            return View(item);
        }

        _context.Update(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Set<Falha>().FirstOrDefaultAsync(x => x.IdFalha == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Set<Falha>().FindAsync(id);
        if (item != null)
        {
            _context.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

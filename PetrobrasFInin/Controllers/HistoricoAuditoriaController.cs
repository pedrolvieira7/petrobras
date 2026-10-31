using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Controllers;

public class HistoricoAuditoriaController : Controller
{
    private readonly PetrobrasContext _context;

    public HistoricoAuditoriaController(PetrobrasContext context)
    {
        _context = context;
    }

    private async Task CarregarListas()
    {
        ViewData["IdUsuario"] = new SelectList(await _context.Set<Usuario>().ToListAsync(), "IdUsuario", "NomeCompleto");
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Set<HistoricoAuditorium>().ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Set<HistoricoAuditorium>().FirstOrDefaultAsync(x => x.IdHistorico == id);
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
    public async Task<IActionResult> Create([Bind("TipoEvento,RegistroAfetado,DataHora,IdUsuario")] HistoricoAuditorium item)
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
        var item = await _context.Set<HistoricoAuditorium>().FindAsync(id);
        if (item == null) return NotFound();
        await CarregarListas();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdHistorico,TipoEvento,RegistroAfetado,DataHora,IdUsuario")] HistoricoAuditorium item)
    {
        if (id != item.IdHistorico) return NotFound();
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
        var item = await _context.Set<HistoricoAuditorium>().FirstOrDefaultAsync(x => x.IdHistorico == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Set<HistoricoAuditorium>().FindAsync(id);
        if (item != null)
        {
            _context.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

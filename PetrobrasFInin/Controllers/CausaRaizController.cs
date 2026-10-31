using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Controllers;

public class CausaRaizController : Controller
{
    private readonly PetrobrasContext _context;

    public CausaRaizController(PetrobrasContext context)
    {
        _context = context;
    }

    private async Task CarregarListas()
    {
        ViewData["IdAnalise"] = new SelectList(await _context.Set<Analise>().ToListAsync(), "IdAnalise", "CodigoAnalise");
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Set<CausaRaiz>().ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Set<CausaRaiz>().FirstOrDefaultAsync(x => x.IdCausa == id);
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
    public async Task<IActionResult> Create([Bind("CausaPrincipal,Categoria,TipoCausa,IdAnalise")] CausaRaiz item)
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
        var item = await _context.Set<CausaRaiz>().FindAsync(id);
        if (item == null) return NotFound();
        await CarregarListas();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdCausa,CausaPrincipal,Categoria,TipoCausa,IdAnalise")] CausaRaiz item)
    {
        if (id != item.IdCausa) return NotFound();
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
        var item = await _context.Set<CausaRaiz>().FirstOrDefaultAsync(x => x.IdCausa == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Set<CausaRaiz>().FindAsync(id);
        if (item != null)
        {
            _context.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

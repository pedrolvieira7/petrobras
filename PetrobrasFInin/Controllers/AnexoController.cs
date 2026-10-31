using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Controllers;

public class AnexoController : Controller
{
    private readonly PetrobrasContext _context;

    public AnexoController(PetrobrasContext context)
    {
        _context = context;
    }

    private async Task CarregarListas()
    {
        ViewData["IdFalha"] = new SelectList(await _context.Set<Falha>().ToListAsync(), "IdFalha", "TituloFalha");
        ViewData["IdAnalise"] = new SelectList(await _context.Set<Analise>().ToListAsync(), "IdAnalise", "CodigoAnalise");
        ViewData["IdAcao"] = new SelectList(await _context.Set<AcaoCorretiva>().ToListAsync(), "IdAcao", "CodigoAcao");
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Set<Anexo>().ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Set<Anexo>().FirstOrDefaultAsync(x => x.IdAnexo == id);
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
    public async Task<IActionResult> Create([Bind("NomeArquivo,CaminhoArquivo,IdFalha,IdAnalise,IdAcao")] Anexo item)
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
        var item = await _context.Set<Anexo>().FindAsync(id);
        if (item == null) return NotFound();
        await CarregarListas();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdAnexo,NomeArquivo,CaminhoArquivo,IdFalha,IdAnalise,IdAcao")] Anexo item)
    {
        if (id != item.IdAnexo) return NotFound();
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
        var item = await _context.Set<Anexo>().FirstOrDefaultAsync(x => x.IdAnexo == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Set<Anexo>().FindAsync(id);
        if (item != null)
        {
            _context.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

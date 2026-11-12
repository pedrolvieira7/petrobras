using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Services;

namespace PetrobrasFInin.Controllers;

[Authorize]
public class NotificacoesController : Controller
{
    private readonly PetrobrasContext _db;
    public NotificacoesController(PetrobrasContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var id = RegistroSiaf.UsuarioId(User);
        if (id == null) return Challenge();
        return View(await _db.Notificacoes.AsNoTracking().Where(x => x.IdUsuario == id.Value)
            .OrderByDescending(x => x.CriadoEm).Take(100).ToListAsync());
    }
    [HttpGet]
    public async Task<IActionResult> Contador()
    {
        var id = RegistroSiaf.UsuarioId(User);
        if (id == null) return Unauthorized();
        int total = await _db.Notificacoes.CountAsync(x => x.IdUsuario == id.Value && x.LidoEm == null);
        return Json(new { pendentes = total });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Ler(int id)
    {
        var usuario = RegistroSiaf.UsuarioId(User);
        if (usuario == null) return Challenge();
        var item = await _db.Notificacoes.FirstOrDefaultAsync(x => x.IdNotificacao == id && x.IdUsuario == usuario.Value);
        if (item == null) return NotFound();
        item.LidoEm ??= DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        await _db.SaveChangesAsync();
        if (!string.IsNullOrWhiteSpace(item.Link) && item.Link.StartsWith("/Ocorrencias/Detalhes/", StringComparison.Ordinal))
            return LocalRedirect(item.Link);
        return RedirectToAction(nameof(Index));
    }
}

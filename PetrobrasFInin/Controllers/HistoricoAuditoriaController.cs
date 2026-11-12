using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;

namespace PetrobrasFInin.Controllers;

[Authorize(Roles="Administrador,Gestor")]
public class HistoricoAuditoriaController : Controller
{
    private readonly PetrobrasContext _db;
    public HistoricoAuditoriaController(PetrobrasContext db) => _db = db;
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _db.HistoricoAuditoria.AsNoTracking()
            .Include(x => x.IdUsuarioNavigation).OrderByDescending(x => x.DataHora)
            .Take(500).ToListAsync());
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PetrobrasFInin.Controllers;

// Telas legadas substituídas pelo fluxo completo de ocorrências.
// A antiga manutenção direta permitia alterar status e aprovação sem validação.
[Authorize]
public class AnexoController : Controller
{
    [HttpGet]
    public IActionResult Index() => RedirectToAction("Index", "Ocorrencias");

    [HttpGet]
    public IActionResult Details(int? id) => RedirectToAction("Index", "Ocorrencias");

    [HttpGet]
    public IActionResult Create() => RedirectToAction("Index", "Ocorrencias");

    [HttpGet]
    public IActionResult Edit(int? id) => RedirectToAction("Index", "Ocorrencias");

    [HttpGet]
    public IActionResult Delete(int? id) => RedirectToAction("Index", "Ocorrencias");

    // Endpoints antigos de escrita ficam bloqueados: use OcorrenciasController.
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(object? item) => Forbid();
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, object? item) => Forbid();
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id) => Forbid();
}

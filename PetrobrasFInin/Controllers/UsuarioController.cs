using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuarioController : Controller
{
    private readonly PetrobrasContext _context;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public UsuarioController(PetrobrasContext context)
    {
        _context = context;
    }

    private async Task CarregarListas()
    {
        ViewData["IdPerfil"] = new SelectList(await _context.Set<Perfil>().ToListAsync(), "IdPerfil", "Nome");
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Set<Usuario>().ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Set<Usuario>().FirstOrDefaultAsync(x => x.IdUsuario == id);
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
    public async Task<IActionResult> Create([Bind("NomeCompleto,Cpf,Email,Senha,IdPerfil")] Usuario item)
    {
        if (string.IsNullOrWhiteSpace(item.Senha))
            ModelState.AddModelError(nameof(item.Senha), "Informe uma senha.");

        if (!string.IsNullOrWhiteSpace(item.Senha) && item.Senha.Length < 8)
            ModelState.AddModelError(nameof(item.Senha), "A senha deve ter pelo menos 8 caracteres.");

        item.Email = item.Email?.Trim() ?? "";
        item.Cpf = item.Cpf?.Trim() ?? "";
        item.NomeCompleto = item.NomeCompleto?.Trim() ?? "";

        if (await _context.Usuarios.AnyAsync(u => u.Email == item.Email))
            ModelState.AddModelError(nameof(item.Email), "Já existe um usuário com esse e-mail.");

        if (await _context.Usuarios.AnyAsync(u => u.Cpf == item.Cpf))
            ModelState.AddModelError(nameof(item.Cpf), "Já existe um usuário com esse CPF.");

        if (!await _context.Perfils.AnyAsync(p => p.IdPerfil == item.IdPerfil))
            ModelState.AddModelError(nameof(item.IdPerfil), "Selecione um perfil válido.");

        if (!ModelState.IsValid)
        {
            await CarregarListas();
            return View(item);
        }

        item.Senha = _passwordHasher.HashPassword(item, item.Senha);

        try
        {
            _context.Usuarios.Add(item);
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Usuário cadastrado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            // Registra o erro completo no console/Output do Visual Studio para facilitar o diagnóstico.
            Console.Error.WriteLine($"Erro ao salvar usuário: {ex}");
            ModelState.AddModelError(string.Empty,
                "Não foi possível salvar o usuário. Confira os campos e tente novamente.");
            await CarregarListas();
            return View(item);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Set<Usuario>().FindAsync(id);
        if (item == null) return NotFound();
        await CarregarListas();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string nomeCompleto, string cpf, string email,
        int idPerfil, string? novaSenha)
    {
        var existente = await _context.Usuarios.FindAsync(id);
        if (existente == null) return NotFound();

        if (string.IsNullOrWhiteSpace(nomeCompleto) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(cpf) || !await _context.Perfils.AnyAsync(p => p.IdPerfil == idPerfil))
            ModelState.AddModelError(string.Empty, "Preencha os campos e escolha um perfil válido.");
        if (!string.IsNullOrWhiteSpace(novaSenha) && novaSenha.Length < 8)
            ModelState.AddModelError(string.Empty, "A nova senha deve ter pelo menos 8 caracteres.");
        if (await _context.Usuarios.AnyAsync(u => u.IdUsuario != id && (u.Email == (email ?? string.Empty).Trim() || u.Cpf == (cpf ?? string.Empty).Trim())))
            ModelState.AddModelError(string.Empty, "Já existe um usuário com esse e-mail ou CPF.");

        // Não permitir a remoção do último administrador.
        var admin = await _context.Perfils.FirstOrDefaultAsync(p => p.Nome == "Administrador");
        if (admin != null && existente.IdPerfil == admin.IdPerfil && idPerfil != admin.IdPerfil &&
            !await _context.Usuarios.AnyAsync(u => u.IdUsuario != id && u.IdPerfil == admin.IdPerfil))
            ModelState.AddModelError(string.Empty, "Não é possível remover o último administrador.");

        if (!ModelState.IsValid)
        {
            await CarregarListas();
            return View(existente);
        }

        existente.NomeCompleto = nomeCompleto.Trim();
        existente.Cpf = cpf.Trim();
        existente.Email = email.Trim();
        existente.IdPerfil = idPerfil;
        if (!string.IsNullOrWhiteSpace(novaSenha))
            existente.Senha = _passwordHasher.HashPassword(existente, novaSenha);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Set<Usuario>().FirstOrDefaultAsync(x => x.IdUsuario == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Set<Usuario>().FindAsync(id);
        if (item != null)
        {
            var admin = await _context.Perfils.FirstOrDefaultAsync(p => p.Nome == "Administrador");
            if (admin != null && item.IdPerfil == admin.IdPerfil &&
                !await _context.Usuarios.AnyAsync(u => u.IdUsuario != id && u.IdPerfil == admin.IdPerfil))
            {
                TempData["Erro"] = "Não é possível excluir o último administrador.";
                return RedirectToAction(nameof(Index));
            }
            _context.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

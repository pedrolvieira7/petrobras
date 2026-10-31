using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Controllers;

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
            var detalhe = ex.InnerException?.Message;
            var mensagem = "Não foi possível salvar o usuário no banco de dados. ";

            if (!string.IsNullOrWhiteSpace(detalhe))
            {
                // Mensagem útil para ambiente local de desenvolvimento.
                mensagem += $"Detalhe do SQL Server: {detalhe}";
            }
            else
            {
                mensagem += "Confira a conexão, se o perfil existe e se a coluna dbo.usuario.senha aceita VARCHAR(255).";
            }

            ModelState.AddModelError(string.Empty, mensagem);
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
    public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,NomeCompleto,Cpf,Email,Senha,IdPerfil")] Usuario item)
    {
        if (id != item.IdUsuario) return NotFound();
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
            _context.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}

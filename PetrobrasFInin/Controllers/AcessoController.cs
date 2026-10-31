using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Controllers;

public class AcessoController : Controller
{
    private readonly PetrobrasContext _context;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public AcessoController(PetrobrasContext context) => _context = context;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpGet]
    public async Task<IActionResult> CriarConta()
    {
        await CarregarPerfis();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarConta(string nome, string email, string cpf, int? perfil, string senha)
    {
        await CarregarPerfis();
        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(cpf) || string.IsNullOrWhiteSpace(senha) || !perfil.HasValue)
        { ViewBag.Erro = "Preencha todos os campos e selecione um perfil."; return View(); }
        if (senha.Length < 8) { ViewBag.Erro = "A senha deve ter pelo menos 8 caracteres."; return View(); }
        if (!await _context.Perfils.AnyAsync(p => p.IdPerfil == perfil.Value))
        { ViewBag.Erro = "O perfil selecionado não existe. Confira os perfis cadastrados."; return View(); }
        email = email.Trim(); cpf = cpf.Trim();
        if (await _context.Usuarios.AnyAsync(u => u.Email == email || u.Cpf == cpf))
        { ViewBag.Erro = "Já existe um usuário com esse e-mail ou CPF."; return View(); }

        var usuario = new Usuario { NomeCompleto = nome.Trim(), Email = email, Cpf = cpf, IdPerfil = perfil.Value, Senha = "" };
        usuario.Senha = _passwordHasher.HashPassword(usuario, senha);
        try
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Cadastro realizado. Você já pode entrar no sistema.";
            return RedirectToAction(nameof(Login));
        }
        catch (DbUpdateException)
        {
            _context.Entry(usuario).State = EntityState.Detached;
            ViewBag.Erro = "Não foi possível salvar. Execute o script SQL AjustarSenhaUsuario.sql incluído no projeto e tente novamente.";
            return View();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrar(string email, string senha)
    {
        var usuario = await _context.Usuarios.Include(u => u.IdPerfilNavigation).FirstOrDefaultAsync(u => u.Email == email);
        if (usuario == null || string.IsNullOrWhiteSpace(senha) || _passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, senha) == PasswordVerificationResult.Failed)
        { ViewBag.Erro = "E-mail ou senha inválidos."; return View("Login"); }
        TempData["Sucesso"] = "Senha verificada. A autenticação por sessão ainda precisa ser configurada para proteger as áreas internas.";
        return RedirectToAction("Index", "Home");
    }

    private async Task CarregarPerfis()
    {
        try { ViewBag.Perfis = await _context.Perfils.AsNoTracking().OrderBy(p => p.Nome).ToListAsync(); }
        catch { ViewBag.Perfis = new List<Perfil>(); ViewBag.Erro = "Não foi possível carregar os perfis. Confira a conexão com o banco."; }
    }
}

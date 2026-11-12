using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Controllers;

public class AcessoController : Controller
{
    private readonly PetrobrasContext _context;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public AcessoController(PetrobrasContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Login()
    {
        ViewBag.PrimeiroCadastro = !await _context.Usuarios.AnyAsync();
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> CriarConta()
    {
        var primeiro = !await _context.Usuarios.AnyAsync();
        if (!primeiro && !User.IsInRole("Administrador"))
            return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();

        await CarregarPerfis(primeiro);
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarConta(
        string nome, string email, string cpf, int? perfil, string senha, string? chaveInicial)
    {
        var primeiro = !await _context.Usuarios.AnyAsync();
        if (!primeiro && !User.IsInRole("Administrador"))
            return User.Identity?.IsAuthenticated == true ? Forbid() : Challenge();

        await CarregarPerfis(primeiro);

        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(cpf) || string.IsNullOrWhiteSpace(senha) || !perfil.HasValue)
        {
            ViewBag.Erro = "Preencha todos os campos.";
            return View();
        }
        if (senha.Length < 8)
        {
            ViewBag.Erro = "A senha deve ter pelo menos 8 caracteres.";
            return View();
        }

        if (primeiro)
        {
            var chaveConfigurada = _configuration["SIAF_BOOTSTRAP_KEY"];
            if (string.IsNullOrWhiteSpace(chaveConfigurada) ||
                string.IsNullOrWhiteSpace(chaveInicial) ||
                !CryptographicOperations.FixedTimeEquals(
                    SHA256.HashData(Encoding.UTF8.GetBytes(chaveConfigurada)),
                    SHA256.HashData(Encoding.UTF8.GetBytes(chaveInicial))))
            {
                ViewBag.Erro = "Chave inicial inválida. Configure SIAF_BOOTSTRAP_KEY no Railway.";
                return View();
            }

            // O primeiro usuário será sempre administrador, independentemente do campo enviado.
            var admin = await _context.Perfils.SingleAsync(p => p.Nome == "Administrador");
            perfil = admin.IdPerfil;
        }
        else if (!await _context.Perfils.AnyAsync(p => p.IdPerfil == perfil.Value))
        {
            ViewBag.Erro = "O perfil selecionado não existe.";
            return View();
        }

        email = email.Trim();
        cpf = cpf.Trim();
        if (await _context.Usuarios.AnyAsync(u => u.Email == email || u.Cpf == cpf))
        {
            ViewBag.Erro = "Já existe um usuário com esse e-mail ou CPF.";
            return View();
        }

        var usuario = new Usuario
        {
            NomeCompleto = nome.Trim(), Email = email, Cpf = cpf,
            IdPerfil = perfil.Value, Senha = ""
        };
        usuario.Senha = _passwordHasher.HashPassword(usuario, senha);
        try
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Cadastro realizado. Entre com seu e-mail e senha.";
            return RedirectToAction(nameof(Login));
        }
        catch (DbUpdateException)
        {
            _context.Entry(usuario).State = EntityState.Detached;
            ViewBag.Erro = "Não foi possível salvar o usuário. Confira os campos e tente novamente.";
            return View();
        }
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrar(string email, string senha)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.IdPerfilNavigation)
            .FirstOrDefaultAsync(u => u.Email == (email ?? string.Empty).Trim());
        if (usuario == null || string.IsNullOrWhiteSpace(senha) ||
            _passwordHasher.VerifyHashedPassword(usuario, usuario.Senha, senha) == PasswordVerificationResult.Failed)
        {
            ViewBag.Erro = "E-mail ou senha inválidos.";
            return View("Login");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.NomeCompleto),
            new Claim(ClaimTypes.Role, usuario.IdPerfilNavigation.Nome)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private async Task CarregarPerfis(bool primeiro)
    {
        ViewBag.PrimeiroCadastro = primeiro;
        var perfis = await _context.Perfils.AsNoTracking().OrderBy(p => p.Nome).ToListAsync();
        ViewBag.Perfis = primeiro
            ? perfis.Where(p => p.Nome == "Administrador").ToList()
            : perfis;
    }
}

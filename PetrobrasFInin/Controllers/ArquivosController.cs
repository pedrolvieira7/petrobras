using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;
using PetrobrasFInin.Services;

namespace PetrobrasFInin.Controllers;

[Authorize]
public class ArquivosController : Controller
{
    private readonly PetrobrasContext _db;
    public ArquivosController(PetrobrasContext db) => _db = db;

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Enviar(int idFalha, IFormFile? arquivo)
    {
        if (!await _db.Falhas.AnyAsync(f => f.IdFalha == idFalha)) return NotFound();
        if (arquivo == null || arquivo.Length == 0 || arquivo.Length > 5 * 1024 * 1024)
        {
            TempData["Erro"] = "Selecione um arquivo de até 5 MB.";
            return RedirectToAction("Detalhes", "Ocorrencias", new { id = idFalha });
        }
        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        var tipos = new Dictionary<string, string> {
            [".pdf"] = "application/pdf", [".png"] = "image/png",
            [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg"
        };
        if (!tipos.TryGetValue(extensao, out var tipo))
        {
            TempData["Erro"] = "Formato inválido. Envie PDF, PNG ou JPG.";
            return RedirectToAction("Detalhes", "Ocorrencias", new { id = idFalha });
        }
        await using var buffer = new MemoryStream();
        await arquivo.CopyToAsync(buffer);
        byte[] bytes = buffer.ToArray();
        bool assinaturaValida = extensao switch
        {
            ".pdf" => bytes.Length > 5 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D,
            ".png" => bytes.Length > 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47,
            _ => bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF
        };
        if (!assinaturaValida)
        {
            TempData["Erro"] = "O conteúdo do arquivo não corresponde ao formato informado.";
            return RedirectToAction("Detalhes", "Ocorrencias", new { id = idFalha });
        }
        string nome = Path.GetFileName(arquivo.FileName).Replace("/", "_").Replace("\\", "_");
        if (nome.Length > 150) nome = nome[^150..];
        await using var transacao = await _db.Database.BeginTransactionAsync();
        var anexo = new Anexo { IdFalha = idFalha, NomeArquivo = nome, CaminhoArquivo = "armazenamento-privado-banco" };
        _db.Anexos.Add(anexo);
        await _db.SaveChangesAsync();
        _db.AnexosConteudo.Add(new AnexoConteudo { IdAnexo = anexo.IdAnexo, TipoConteudo = tipo, Conteudo = bytes });
        RegistroSiaf.Auditar(_db, User, "Enviar anexo", $"anexo:{anexo.IdAnexo}", depois: new { idFalha, nome });
        await _db.SaveChangesAsync();
        await transacao.CommitAsync();
        TempData["Sucesso"] = "Arquivo anexado com sucesso.";
        return RedirectToAction("Detalhes", "Ocorrencias", new { id = idFalha });
    }

    [HttpGet]
    public async Task<IActionResult> Baixar(int id)
    {
        var anexo = await _db.Anexos.AsNoTracking().FirstOrDefaultAsync(x => x.IdAnexo == id);
        if (anexo == null) return NotFound();
        var conteudo = await _db.AnexosConteudo.AsNoTracking().FirstOrDefaultAsync(x => x.IdAnexo == id);
        if (conteudo == null) return NotFound("Este anexo antigo não possui arquivo disponível no banco.");
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers["Content-Security-Policy"] = "sandbox";
        return File(conteudo.Conteudo, conteudo.TipoConteudo, anexo.NomeArquivo);
    }
}

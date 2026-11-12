using System.Security.Claims;
using System.Text.Json;
using PetrobrasFInin.Data;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Services;

public static class RegistroSiaf
{
    public static int? UsuarioId(ClaimsPrincipal principal)
    {
        return int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    }

    public static void Auditar(PetrobrasContext db, ClaimsPrincipal principal,
        string evento, string registro, object? antes = null, object? depois = null)
    {
        var id = UsuarioId(principal);
        if (id == null) return;
        db.HistoricoAuditoria.Add(new HistoricoAuditorium
        {
            IdUsuario = id.Value,
            TipoEvento = evento.Length > 50 ? evento[..50] : evento,
            RegistroAfetado = registro.Length > 60 ? registro[..60] : registro,
            DataHora = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            DadosAntes = antes == null ? null : JsonSerializer.Serialize(antes),
            DadosDepois = depois == null ? null : JsonSerializer.Serialize(depois)
        });
    }

    public static void Notificar(PetrobrasContext db, int usuarioId,
        string titulo, string mensagem, string? link = null)
    {
        if (usuarioId <= 0) return;
        db.Notificacoes.Add(new Notificacao
        {
            IdUsuario = usuarioId,
            Titulo = titulo.Length > 120 ? titulo[..120] : titulo,
            Mensagem = mensagem.Length > 500 ? mensagem[..500] : mensagem,
            Link = link,
            CriadoEm = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        });
    }
}

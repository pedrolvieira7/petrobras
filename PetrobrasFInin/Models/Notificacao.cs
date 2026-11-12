using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetrobrasFInin.Models;

[Table("notificacao", Schema = "siaf")]
public class Notificacao
{
    [Key, Column("id_notificacao")]
    public int IdNotificacao { get; set; }
    [Column("id_usuario")]
    public int IdUsuario { get; set; }
    [Column("titulo"), StringLength(120)]
    public string Titulo { get; set; } = "";
    [Column("mensagem"), StringLength(500)]
    public string Mensagem { get; set; } = "";
    [Column("link"), StringLength(250)]
    public string? Link { get; set; }
    [Column("criado_em", TypeName="timestamp without time zone")]
    public DateTime CriadoEm { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    [Column("lido_em", TypeName="timestamp without time zone")]
    public DateTime? LidoEm { get; set; }
}

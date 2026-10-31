using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetrobrasFInin.Models;

[Table("historico_auditoria")]
public partial class HistoricoAuditorium
{
    [Key]
    [Column("id_historico")]
    public int IdHistorico { get; set; }

    [Column("tipo_evento")]
    [StringLength(50)]
    [Unicode(false)]
    public string TipoEvento { get; set; } = null!;

    [Column("registro_afetado")]
    [StringLength(60)]
    [Unicode(false)]
    public string RegistroAfetado { get; set; } = null!;

    [Column("data_hora", TypeName = "datetime")]
    public DateTime DataHora { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [ForeignKey("IdUsuario")]
    [InverseProperty("HistoricoAuditoria")]
    [ValidateNever]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

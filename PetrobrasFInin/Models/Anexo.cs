using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetrobrasFInin.Models;

[Table("anexo")]
public partial class Anexo
{
    [Key]
    [Column("id_anexo")]
    public int IdAnexo { get; set; }

    [Column("nome_arquivo")]
    [StringLength(150)]
    [Unicode(false)]
    public string NomeArquivo { get; set; } = null!;

    [Column("caminho_arquivo")]
    [StringLength(225)]
    [Unicode(false)]
    public string CaminhoArquivo { get; set; } = null!;

    [Column("id_falha")]
    public int? IdFalha { get; set; }

    [Column("id_analise")]
    public int? IdAnalise { get; set; }

    [Column("id_acao")]
    public int? IdAcao { get; set; }

    [ForeignKey("IdAcao")]
    [InverseProperty("Anexos")]
    [ValidateNever]
    public virtual AcaoCorretiva? IdAcaoNavigation { get; set; }

    [ForeignKey("IdAnalise")]
    [InverseProperty("Anexos")]
    [ValidateNever]
    public virtual Analise? IdAnaliseNavigation { get; set; }

    [ForeignKey("IdFalha")]
    [InverseProperty("Anexos")]
    [ValidateNever]
    public virtual Falha? IdFalhaNavigation { get; set; }
}

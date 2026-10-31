using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetrobrasFInin.Models;

[Table("acao_corretiva")]
public partial class AcaoCorretiva
{
    [Key]
    [Column("id_acao")]
    public int IdAcao { get; set; }

    [Column("codigo_acao")]
    [StringLength(50)]
    [Unicode(false)]
    public string CodigoAcao { get; set; } = null!;

    [Column("descricao_acao")]
    [StringLength(200)]
    [Unicode(false)]
    public string DescricaoAcao { get; set; } = null!;

    [Column("prazo")]
    public DateTime Prazo { get; set; }

    [Column("situacao")]
    [StringLength(200)]
    [Unicode(false)]
    public string Situacao { get; set; } = null!;

    [Column("id_analise")]
    public int IdAnalise { get; set; }

    [Column("id_responsavel")]
    public int IdResponsavel { get; set; }

    [InverseProperty("IdAcaoNavigation")]
    [ValidateNever]
    public virtual ICollection<Anexo> Anexos { get; set; } = new List<Anexo>();

    [ForeignKey("IdAnalise")]
    [InverseProperty("AcaoCorretivas")]
    [ValidateNever]
    public virtual Analise IdAnaliseNavigation { get; set; } = null!;

    [ForeignKey("IdResponsavel")]
    [InverseProperty("AcaoCorretivas")]
    [ValidateNever]
    public virtual Usuario IdResponsavelNavigation { get; set; } = null!;
}

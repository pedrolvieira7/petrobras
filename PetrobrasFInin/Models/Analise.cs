using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetrobrasFInin.Models;

[Table("analise")]
[Index("CodigoAnalise", Name = "UQ__analise__B7D85627CFE5A1A6", IsUnique = true)]
public partial class Analise
{
    [Key]
    [Column("id_analise")]
    public int IdAnalise { get; set; }

    [Column("codigo_analise")]
    [StringLength(50)]
    [Unicode(false)]
    public string CodigoAnalise { get; set; } = null!;

    [Column("metodo_analise")]
    [StringLength(50)]
    [Unicode(false)]
    public string MetodoAnalise { get; set; } = null!;

    [Column("disciplina")]
    [StringLength(50)]
    [Unicode(false)]
    public string Disciplina { get; set; } = null!;

    [Column("decisao_gestor")]
    [StringLength(50)]
    [Unicode(false)]
    public string DecisaoGestor { get; set; } = null!;

    [Column("id_falha")]
    public int IdFalha { get; set; }

    [Column("id_analista")]
    public int IdAnalista { get; set; }

    [Column("id_gestor")]
    public int IdGestor { get; set; }

    [InverseProperty("IdAnaliseNavigation")]
    [ValidateNever]
    public virtual ICollection<AcaoCorretiva> AcaoCorretivas { get; set; } = new List<AcaoCorretiva>();

    [InverseProperty("IdAnaliseNavigation")]
    [ValidateNever]
    public virtual ICollection<Anexo> Anexos { get; set; } = new List<Anexo>();

    [InverseProperty("IdAnaliseNavigation")]
    [ValidateNever]
    public virtual ICollection<CausaRaiz> CausaRaizs { get; set; } = new List<CausaRaiz>();

    [ForeignKey("IdAnalista")]
    [InverseProperty("AnaliseIdAnalistaNavigations")]
    [ValidateNever]
    public virtual Usuario IdAnalistaNavigation { get; set; } = null!;

    [ForeignKey("IdFalha")]
    [InverseProperty("Analises")]
    [ValidateNever]
    public virtual Falha IdFalhaNavigation { get; set; } = null!;

    [ForeignKey("IdGestor")]
    [InverseProperty("AnaliseIdGestorNavigations")]
    [ValidateNever]
    public virtual Usuario IdGestorNavigation { get; set; } = null!;
}

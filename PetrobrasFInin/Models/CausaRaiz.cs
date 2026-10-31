using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetrobrasFInin.Models;

[Table("causa_raiz")]
public partial class CausaRaiz
{
    [Key]
    [Column("id_causa")]
    public int IdCausa { get; set; }

    [Column("causa_principal")]
    [StringLength(100)]
    [Unicode(false)]
    public string CausaPrincipal { get; set; } = null!;

    [Column("categoria")]
    [StringLength(50)]
    [Unicode(false)]
    public string Categoria { get; set; } = null!;

    [Column("tipo_causa")]
    [StringLength(80)]
    [Unicode(false)]
    public string TipoCausa { get; set; } = null!;

    [Column("id_analise")]
    public int IdAnalise { get; set; }

    [ForeignKey("IdAnalise")]
    [InverseProperty("CausaRaizs")]
    [ValidateNever]
    public virtual Analise IdAnaliseNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetrobrasFInin.Models;

[Table("falha")]
[Index("CodigoFalha", Name = "UQ__falha__6F01453461D70943", IsUnique = true)]
public partial class Falha
{
    [Key]
    [Column("id_falha")]
    public int IdFalha { get; set; }

    [Column("codigo_falha")]
    [StringLength(80)]
    [Unicode(false)]
    public string CodigoFalha { get; set; } = null!;

    [Column("titulo_falha")]
    [StringLength(50)]
    [Unicode(false)]
    public string TituloFalha { get; set; } = null!;

    [Column("data_ocorrencia")]
    public DateTime DataOcorrencia { get; set; }

    [Column("criticidade")]
    [StringLength(30)]
    [Unicode(false)]
    public string Criticidade { get; set; } = null!;

    [Column("situacao")]
    [StringLength(200)]
    [Unicode(false)]
    public string Situacao { get; set; } = null!;

    [Column("id_equipamento")]
    public int IdEquipamento { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [InverseProperty("IdFalhaNavigation")]
    [ValidateNever]
    public virtual ICollection<Analise> Analises { get; set; } = new List<Analise>();

    [InverseProperty("IdFalhaNavigation")]
    [ValidateNever]
    public virtual ICollection<Anexo> Anexos { get; set; } = new List<Anexo>();

    [ForeignKey("IdEquipamento")]
    [InverseProperty("Falhas")]
    [ValidateNever]
    public virtual Equipamento IdEquipamentoNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Falhas")]
    [ValidateNever]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

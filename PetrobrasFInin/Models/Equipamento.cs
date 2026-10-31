using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetrobrasFInin.Models;

[Table("equipamento")]
public partial class Equipamento
{
    [Key]
    [Column("id_equipamento")]
    public int IdEquipamento { get; set; }

    [Column("tag")]
    [StringLength(50)]
    [Unicode(false)]
    public string Tag { get; set; } = null!;

    [Column("nome_equipamento")]
    [StringLength(100)]
    [Unicode(false)]
    public string NomeEquipamento { get; set; } = null!;

    [Column("tipo")]
    [StringLength(30)]
    [Unicode(false)]
    public string Tipo { get; set; } = null!;

    [Column("criticidade")]
    [StringLength(30)]
    [Unicode(false)]
    public string Criticidade { get; set; } = null!;

    [Column("situacao")]
    [StringLength(100)]
    [Unicode(false)]
    public string Situacao { get; set; } = null!;

    [Column("id_setor")]
    public int IdSetor { get; set; }

    [Column("id_responsavel")]
    public int IdResponsavel { get; set; }

    [InverseProperty("IdEquipamentoNavigation")]
    [ValidateNever]
    public virtual ICollection<Falha> Falhas { get; set; } = new List<Falha>();

    [ForeignKey("IdResponsavel")]
    [InverseProperty("Equipamentos")]
    [ValidateNever]
    public virtual Usuario IdResponsavelNavigation { get; set; } = null!;

    [ForeignKey("IdSetor")]
    [InverseProperty("Equipamentos")]
    [ValidateNever]
    public virtual Setor IdSetorNavigation { get; set; } = null!;
}

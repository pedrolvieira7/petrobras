using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetrobrasFInin.Models;

[Table("setor")]
public partial class Setor
{
    [Key]
    [Column("id_setor")]
    public int IdSetor { get; set; }

    [Column("unidade")]
    [StringLength(100)]
    [Unicode(false)]
    public string Unidade { get; set; } = null!;

    [Column("area")]
    [StringLength(60)]
    [Unicode(false)]
    public string Area { get; set; } = null!;

    [InverseProperty("IdSetorNavigation")]
    [ValidateNever]
    public virtual ICollection<Equipamento> Equipamentos { get; set; } = new List<Equipamento>();
}

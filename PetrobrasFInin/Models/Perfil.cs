using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetrobrasFInin.Models;

[Table("perfil")]
public partial class Perfil
{
    [Key]
    [Column("id_perfil")]
    public int IdPerfil { get; set; }

    [Column("nome")]
    [StringLength(100)]
    [Unicode(false)]
    public string Nome { get; set; } = null!;

    [Column("descricao")]
    [StringLength(100)]
    [Unicode(false)]
    public string Descricao { get; set; } = null!;

    [InverseProperty("IdPerfilNavigation")]
    [ValidateNever]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PetrobrasFInin.Models;

[Table("usuario")]
public partial class Usuario
{
    [Key]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("nome_completo")]
    [StringLength(100)]
    [Unicode(false)]
    public string NomeCompleto { get; set; } = null!;

    [Column("cpf")]
    [StringLength(14)]
    [Unicode(false)]
    public string Cpf { get; set; } = null!;

    [Column("email")]
    [StringLength(80)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column("senha")]
    [StringLength(255)]
    [Unicode(false)]
    public string Senha { get; set; } = null!;

    [Column("id_perfil")]
    public int IdPerfil { get; set; }

    [InverseProperty("IdResponsavelNavigation")]
    [ValidateNever]
    public virtual ICollection<AcaoCorretiva> AcaoCorretivas { get; set; } = new List<AcaoCorretiva>();

    [InverseProperty("IdAnalistaNavigation")]
    [ValidateNever]
    public virtual ICollection<Analise> AnaliseIdAnalistaNavigations { get; set; } = new List<Analise>();

    [InverseProperty("IdGestorNavigation")]
    [ValidateNever]
    public virtual ICollection<Analise> AnaliseIdGestorNavigations { get; set; } = new List<Analise>();

    [InverseProperty("IdResponsavelNavigation")]
    [ValidateNever]
    public virtual ICollection<Equipamento> Equipamentos { get; set; } = new List<Equipamento>();

    [InverseProperty("IdUsuarioNavigation")]
    [ValidateNever]
    public virtual ICollection<Falha> Falhas { get; set; } = new List<Falha>();

    [InverseProperty("IdUsuarioNavigation")]
    [ValidateNever]
    public virtual ICollection<HistoricoAuditorium> HistoricoAuditoria { get; set; } = new List<HistoricoAuditorium>();

    [ForeignKey("IdPerfil")]
    [InverseProperty("Usuarios")]
    [ValidateNever]
    public virtual Perfil IdPerfilNavigation { get; set; } = null!;
}

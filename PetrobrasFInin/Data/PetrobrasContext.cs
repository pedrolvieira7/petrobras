using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.Data;

public partial class PetrobrasContext : DbContext
{
    public PetrobrasContext()
    {
    }

    public PetrobrasContext(DbContextOptions<PetrobrasContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcaoCorretiva> AcaoCorretivas { get; set; }

    public virtual DbSet<Analise> Analises { get; set; }

    public virtual DbSet<Anexo> Anexos { get; set; }

    public virtual DbSet<CausaRaiz> CausaRaizs { get; set; }

    public virtual DbSet<Equipamento> Equipamentos { get; set; }

    public virtual DbSet<Falha> Falhas { get; set; }

    public virtual DbSet<HistoricoAuditorium> HistoricoAuditoria { get; set; }

    public virtual DbSet<Perfil> Perfils { get; set; }

    public virtual DbSet<Setor> Setors { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcaoCorretiva>(entity =>
        {
            entity.HasKey(e => e.IdAcao).HasName("PK__acao_cor__B474D3979C70F383");

            entity.HasOne(d => d.IdAnaliseNavigation).WithMany(p => p.AcaoCorretivas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_acao_analise");

            entity.HasOne(d => d.IdResponsavelNavigation).WithMany(p => p.AcaoCorretivas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_acao_responsavel");
        });

        modelBuilder.Entity<Analise>(entity =>
        {
            entity.HasKey(e => e.IdAnalise).HasName("PK__analise__11262DCC72CAD34C");

            entity.HasOne(d => d.IdAnalistaNavigation).WithMany(p => p.AnaliseIdAnalistaNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_analise_analista");

            entity.HasOne(d => d.IdFalhaNavigation).WithMany(p => p.Analises)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_analise_falha");

            entity.HasOne(d => d.IdGestorNavigation).WithMany(p => p.AnaliseIdGestorNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_analise_gestor");
        });

        modelBuilder.Entity<Anexo>(entity =>
        {
            entity.HasKey(e => e.IdAnexo).HasName("PK__anexo__F5D8C19ECB7D000A");

            entity.HasOne(d => d.IdAcaoNavigation).WithMany(p => p.Anexos).HasConstraintName("fk_anexo_acao");

            entity.HasOne(d => d.IdAnaliseNavigation).WithMany(p => p.Anexos).HasConstraintName("fk_anexo_analise");

            entity.HasOne(d => d.IdFalhaNavigation).WithMany(p => p.Anexos).HasConstraintName("fk_anexo_falha");
        });

        modelBuilder.Entity<CausaRaiz>(entity =>
        {
            entity.HasKey(e => e.IdCausa).HasName("PK__causa_ra__D200EA9BD822D693");

            entity.HasOne(d => d.IdAnaliseNavigation).WithMany(p => p.CausaRaizs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_causa_analise");
        });

        modelBuilder.Entity<Equipamento>(entity =>
        {
            entity.HasKey(e => e.IdEquipamento).HasName("PK__equipame__B5F07F5C7E15A3F2");

            entity.HasOne(d => d.IdResponsavelNavigation).WithMany(p => p.Equipamentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_equipamento_responsavel");

            entity.HasOne(d => d.IdSetorNavigation).WithMany(p => p.Equipamentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_equipamento_setor");
        });

        modelBuilder.Entity<Falha>(entity =>
        {
            entity.HasKey(e => e.IdFalha).HasName("PK__falha__64D4FB3D4209B1CE");

            entity.HasOne(d => d.IdEquipamentoNavigation).WithMany(p => p.Falhas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_falha_equipamento");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Falhas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_falha_usuario");
        });

        modelBuilder.Entity<HistoricoAuditorium>(entity =>
        {
            entity.HasKey(e => e.IdHistorico).HasName("PK__historic__76E62AC3D88C25AE");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistoricoAuditoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_historico_usuario");
        });

        modelBuilder.Entity<Perfil>(entity =>
        {
            entity.HasKey(e => e.IdPerfil).HasName("PK__perfil__1D1C8768395410E8");
        });

        modelBuilder.Entity<Setor>(entity =>
        {
            entity.HasKey(e => e.IdSetor).HasName("PK__setor__4861BDD4970570F2");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__usuario__4E3E04AD240A1A96");

            entity.HasOne(d => d.IdPerfilNavigation).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_usuario_perfil");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

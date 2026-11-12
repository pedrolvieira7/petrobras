using System.ComponentModel.DataAnnotations;
using PetrobrasFInin.Models;

namespace PetrobrasFInin.ViewModels;

public class RegistroFalhaVm
{
    [Required, StringLength(80)] public string CodigoFalha { get; set; } = "";
    [Required, StringLength(50)] public string TituloFalha { get; set; } = "";
    [Required, StringLength(2000)] public string Descricao { get; set; } = "";
    [Required, StringLength(100)] public string ModoFalha { get; set; } = "";
    [Required] public DateTime DataOcorrencia { get; set; } = DateTime.Now;
    [Required] public string Criticidade { get; set; } = "Média";
    [Range(1,int.MaxValue)] public int IdEquipamento { get; set; }
    [Range(0,100000)] public decimal HorasParada { get; set; }
}
public class RegistroAnaliseVm
{
    [Range(1,int.MaxValue)] public int IdFalha { get; set; }
    [Required, StringLength(50)] public string CodigoAnalise { get; set; } = "";
    [Required, StringLength(50)] public string MetodoAnalise { get; set; } = "5 Porquês";
    [Required, StringLength(50)] public string Disciplina { get; set; } = "Mecânica";
    [Range(1,int.MaxValue)] public int IdGestor { get; set; }
    [Required, StringLength(100)] public string CausaPrincipal { get; set; } = "";
    [Required, StringLength(50)] public string Categoria { get; set; } = "Equipamento";
    [Required, StringLength(80)] public string TipoCausa { get; set; } = "";
}
public class RegistroAcaoVm
{
    [Range(1,int.MaxValue)] public int IdAnalise { get; set; }
    [Required, StringLength(50)] public string CodigoAcao { get; set; } = "";
    [Required, StringLength(200)] public string DescricaoAcao { get; set; } = "";
    [Required] public DateTime Prazo { get; set; } = DateTime.Now.AddDays(7);
    [Range(1,int.MaxValue)] public int IdResponsavel { get; set; }
}
public class SiafPainelVm
{
    public int TotalFalhas { get; set; }
    public int EmAnalise { get; set; }
    public int Acoes { get; set; }
    public int Equipamentos { get; set; }
    public int Concluidas { get; set; }
    public int Altas { get; set; }
    public int Pendentes { get; set; }
    public List<Falha> Recentes { get; set; } = new();
}

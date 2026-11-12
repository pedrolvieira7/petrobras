using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetrobrasFInin.Models;

[Table("anexo_conteudo", Schema = "siaf")]
public class AnexoConteudo
{
    [Key, Column("id_anexo")]
    public int IdAnexo { get; set; }
    [Column("tipo_conteudo"), StringLength(80)]
    public string TipoConteudo { get; set; } = "";
    [Column("conteudo")]
    public byte[] Conteudo { get; set; } = Array.Empty<byte>();
}

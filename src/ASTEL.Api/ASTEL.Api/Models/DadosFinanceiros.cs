using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTEL.Api.Models
{
    public class DadosFinanceiros
    {
        [Key]
        [Column("MatriculaSistel", Order = 0)]
        public int MatriculaSistel { get; set; }

        [Key]
        [Column("MatriculaAstel", Order = 1)]
        public int MatriculaAstel { get; set; }

        [Key]
        [Column("Ano", Order = 2)]
        public int Ano { get; set; }

        [Column("Mes")]
        public float? Mes { get; set; }

        // Propriedade de navegação para DadosCadastrais
        [ForeignKey("MatriculaSistel")]
        public DadosCadastrais DadosCadastrais { get; set; }
    }
}

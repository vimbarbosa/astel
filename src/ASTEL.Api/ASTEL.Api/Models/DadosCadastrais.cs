using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTEL.Api.Models
{
    public class DadosCadastrais
    {
        [Key]
        [Column("MatriculaSistel")]
        public int MatriculaSistel { get; set; }

        [Required]
        [Column("MatriculaAstel")]
        public int MatriculaAstel { get; set; }

        [Required]
        [Column("Nome")]
        [MaxLength(120)]
        public string Nome { get; set; }

        [Column("Endereco")]
        [MaxLength(255)]
        public string Endereco { get; set; }

        [Column("Situacao")]
        public int? Situacao { get; set; } // 1-TITULAR, 2-DEPENDENTE

        [Column("ValorBeneficio")]
        public float? ValorBeneficio { get; set; }

        [Column("EstadoCivil")]
        [MaxLength(50)]
        public string EstadoCivil { get; set; }

        [Column("Telefone")]
        [MaxLength(20)]
        public string Telefone { get; set; }

        [Column("NomeEsposa")]
        [MaxLength(120)]
        public string NomeEsposa { get; set; }

        [Column("CPF")]
        [MaxLength(14)]
        public string CPF { get; set; }

        [Column("RG")]
        [MaxLength(20)]
        public string RG { get; set; }

        [Column("Ativo")]
        public bool? Ativo { get; set; } // 1-true, 0-false

        [Column("DescontoFolha")]
        public bool? DescontoFolha { get; set; } // TRUE OR FALSE

        // Propriedade de navegação para DadosFinanceiros
        public ICollection<DadosFinanceiros> DadosFinanceiros { get; set; }
    }
}

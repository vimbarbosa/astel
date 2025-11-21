using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTEL.Api.Models
{
    [Table("DadosCadastrais")]
    public class DadosCadastrais
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Column("MatriculaSistel")]
        public long? MatriculaSistel { get; set; }

        [Column("MatriculaAstel")]
        public long? MatriculaAstel { get; set; }

        [Required]
        [Column("Nome")]
        [MaxLength(255)]
        public string Nome { get; set; } = string.Empty;

        [Column("Endereco")]
        [MaxLength(500)]
        public string? Endereco { get; set; }

        [Column("Situacao")]
        [MaxLength(255)]
        public string? Situacao { get; set; }

        [Column("ValorBeneficio")]
        public double? ValorBeneficio { get; set; }

        [Column("EstadoCivil")]
        [MaxLength(50)]
        public string? EstadoCivil { get; set; }

        [Column("Telefone")]
        [MaxLength(50)]
        public string? Telefone { get; set; }

        [Column("NomeEsposa")]
        [MaxLength(255)]
        public string? NomeEsposa { get; set; }

        [Column("CPF")]
        [MaxLength(50)]
        public string? CPF { get; set; }

        [Column("RG")]
        [MaxLength(50)]
        public string? RG { get; set; }

        [Column("Ativo")]
        public bool? Ativo { get; set; }

        [Column("DescontoFolha")]
        public bool? DescontoFolha { get; set; }

        public ICollection<DadosFinanceiros> DadosFinanceiros { get; set; }
            = new List<DadosFinanceiros>();
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTEL.Api.Models
{
    [Table("DadosFinanceiros")]
    public class DadosFinanceiros
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Required]
        [Column("IdDadosCadastrais")]
        public long IdDadosCadastrais { get; set; }

        [Column("Ano")]
        public int? Ano { get; set; }

        [Column("Mes")]
        public double? Mes { get; set; }

        [Column("ValorPago")]
        public double? ValorPago { get; set; }

        [ForeignKey("IdDadosCadastrais")]
        public DadosCadastrais? DadosCadastrais { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASTEL.Api.Models
{
    [Table("Importacoes")]
    public class Importacao
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("Arquivo")]
        public string Arquivo { get; set; } = string.Empty;

        [Column("ImportadoEm")]
        public DateTime? ImportadoEm { get; set; }
    }
}


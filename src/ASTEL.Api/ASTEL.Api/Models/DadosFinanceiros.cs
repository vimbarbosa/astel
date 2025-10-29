using ASTEL.Api.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class DadosFinanceiros
{
    [Column("MatriculaSistel")]
    public long MatriculaSistel { get; set; }

    [Column("MatriculaAstel")]
    public long MatriculaAstel { get; set; }

    [Column("Ano")]
    public int Ano { get; set; }

    [Column("Mes")]
    public double? Mes { get; set; }

    [Column("ValorPago")]
    public double? ValorPago { get; set; } // 🔹 Novo campo

    [ForeignKey("MatriculaSistel")]
    public DadosCadastrais DadosCadastrais { get; set; }
}

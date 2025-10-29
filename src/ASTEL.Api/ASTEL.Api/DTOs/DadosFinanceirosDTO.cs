namespace ASTEL.Api.DTOs
{
    public class DadosFinanceirosDTO
    {
        public long MatriculaSistel { get; set; }
        public long MatriculaAstel { get; set; }
        public int Ano { get; set; }
        public double? Mes { get; set; }
        public double? ValorPago { get; set; } // 🔹 Novo campo
    }
}

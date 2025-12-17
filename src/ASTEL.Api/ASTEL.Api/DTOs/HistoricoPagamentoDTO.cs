namespace ASTEL.Api.DTOs
{
    public class HistoricoPagamentoDTO
    {
        public long Id { get; set; }
        public long IdDadosCadastrais { get; set; }
        public int? Ano { get; set; }
        public int? Mes { get; set; }
        public double? ValorPago { get; set; }
    }
}


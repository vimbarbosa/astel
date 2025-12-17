namespace ASTEL.Api.DTOs
{
    public class ImportacaoDTO
    {
        public long Id { get; set; }
        public string Arquivo { get; set; } = string.Empty;
        public DateTime? ImportadoEm { get; set; }
    }
}


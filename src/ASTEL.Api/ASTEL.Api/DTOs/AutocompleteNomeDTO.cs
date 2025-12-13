namespace ASTEL.Api.DTOs
{
    public class AutocompleteNomeDTO
    {
        public long Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public long? MatriculaAstel { get; set; }
    }
}


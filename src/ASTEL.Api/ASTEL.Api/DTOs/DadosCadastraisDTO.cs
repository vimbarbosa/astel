namespace ASTEL.Api.DTOs
{
    public class DadosCadastraisDTO
    {
        public long Id { get; set; }

        public long? MatriculaSistel { get; set; }
        public long? MatriculaAstel { get; set; }

        public string Nome { get; set; }
        public string Endereco { get; set; }
        public string? Situacao { get; set; }
        public double? ValorBeneficio { get; set; }
        public string EstadoCivil { get; set; }
        public string Telefone { get; set; }
        public string NomeEsposa { get; set; }
        public string CPF { get; set; }
        public string RG { get; set; }
        public bool? Ativo { get; set; }
        public bool? DescontoFolha { get; set; }
    }
}

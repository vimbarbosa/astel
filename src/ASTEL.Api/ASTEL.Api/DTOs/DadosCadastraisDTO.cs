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

        //
        // NOVOS CAMPOS
        //

        public string? Logradouro { get; set; }
        public string? CelSkype { get; set; }
        public string? Estado { get; set; }
        public string? Cidade { get; set; }
        public string? TipoEndereco { get; set; }
        public string? Correspondencia { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Email { get; set; }
        public string? CEP { get; set; }
        public string? FormaPagamento { get; set; }
    }
}

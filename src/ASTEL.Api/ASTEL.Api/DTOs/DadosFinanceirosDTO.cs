public class DadosFinanceirosDTO
{
    // Financeiro
    public string? Id { get; set; }
    public int? Ano { get; set; }
    public int? Mes { get; set; }
    public double? ValorPago { get; set; }

    // Cadastro
    public long IdDadosCadastrais { get; set; }
    public long? MatriculaSistel { get; set; }
    public long? MatriculaAstel { get; set; }
    public string Nome { get; set; }
    public string CPF { get; set; }
    public string RG { get; set; }
    public string Endereco { get; set; }
    public string EstadoCivil { get; set; }
    public string Telefone { get; set; }
    public string Situacao { get; set; }
    public bool? Ativo { get; set; }
    public bool? DescontoFolha { get; set; }

    // 🔥 Novos campos
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

    // Flag
    public bool Inadimplente { get; set; }
    
    // Soma total do ValorPago (preenchido no endpoint Filtrar)
    public double? SomaValorPago { get; set; }
    
    // Total de registros retornados (preenchido no endpoint Filtrar)
    public int? TotalRegistros { get; set; }
}

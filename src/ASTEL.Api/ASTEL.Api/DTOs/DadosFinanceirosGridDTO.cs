public class DadosFinanceirosGridDTO
{
    public long Id { get; set; }
    public long IdDadosCadastrais { get; set; }
    public long? MatriculaSistel { get; set; }
    public long MatriculaAstel { get; set; }
    public string Nome { get; set; }
    public string CPF { get; set; }
    public string RG { get; set; }
    public string Endereco { get; set; }
    public string EstadoCivil { get; set; }
    public string Telefone { get; set; }
    public string Situacao { get; set; }
    public bool Ativo { get; set; }
    public int? Ano { get; set; }
    public int? Mes { get; set; }
    public double? ValorPago { get; set; }
    public bool Inadimplente { get; set; }
}

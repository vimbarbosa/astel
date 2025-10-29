namespace ASTEL.Api.DTOs
{
    public class DadosCadastraisCsv
    {
        public int MatriculaSistel { get; set; }
        public int MatriculaAstel { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public int Situacao { get; set; }
        public double ValorBeneficio { get; set; }
        public string EstadoCivil { get; set; }
        public string Telefone { get; set; }
        public string NomeEsposa { get; set; }
        public string CPF { get; set; }
        public string RG { get; set; }
        public bool Ativo { get; set; }
        public bool DescontoFolha { get; set; }
    }
}

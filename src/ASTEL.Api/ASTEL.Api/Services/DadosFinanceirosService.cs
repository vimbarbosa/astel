using ASTEL.Api.Data;
using ASTEL.Api.DTOs;
using ASTEL.Api.Models;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ASTEL.Api.Services
{
    public class DadosFinanceirosService
    {
        private readonly AppDbContext _context;

        public DadosFinanceirosService(AppDbContext context)
        {
            _context = context;
        }

        public class FinanceiroResult
        {
            public DadosFinanceiros Financeiro { get; set; }
            public DadosCadastrais Cadastro { get; set; }
        }

        public async Task<(List<DadosFinanceirosDTO> dados, int totalCount)>
GetFilteredAsync(DateTime? inicio, DateTime? fim, string? nome, string? cpf,
                 long? matriculaAstel, bool? inadimplente,
                 string? cidade, string? estado, string? email, string? telefone,
                 int pageNumber, int pageSize)
        {
            string connStr = "Server=sqlserver,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";
            //string connStr = "Server=sqlserver,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;\r\n";

            // ------------------------- CTE -------------------------
            var cte = @"
;WITH Base AS (
    SELECT 
        f.Id,
        c.Id AS IdDadosCadastrais,
        c.MatriculaSistel,
        c.MatriculaAstel,
        c.Nome,
        c.CPF,
        c.RG,
        c.Endereco,
        c.EstadoCivil,
        c.Telefone,
        c.Situacao,
        c.Ativo,
        c.Logradouro,
        c.CelSkype,
        c.Estado,
        c.Cidade,
        c.TipoEndereco,
        c.Correspondencia,
        c.Numero,
        c.Complemento,
        c.Bairro,
        c.Email,
        c.CEP,
        f.Ano,
        f.Mes,
        f.ValorPago,

        CASE 
            WHEN EXISTS (
                SELECT 1 
                FROM DadosFinanceiros fx
                WHERE fx.IdDadosCadastrais = c.Id
                  AND fx.Ano = YEAR(GETDATE())
                  AND fx.Mes = MONTH(GETDATE())
                  AND fx.ValorPago IS NOT NULL
            ) THEN 0
            ELSE 1
        END AS Inadimplente

    FROM DadosCadastrais c
    LEFT JOIN DadosFinanceiros f
        ON c.Id = f.IdDadosCadastrais
    WHERE C.Ativo = 1
";

            var filters = "";
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(nome))
            {
                filters += " AND c.Nome LIKE @nome";
                parameters.Add(new SqlParameter("@nome", $"%{nome}%"));
            }

            if (!string.IsNullOrWhiteSpace(cpf))
            {
                filters += " AND c.CPF LIKE @cpf";
                parameters.Add(new SqlParameter("@cpf", $"%{cpf}%"));
            }

            if (matriculaAstel.HasValue)
            {
                filters += " AND c.MatriculaAstel = @matriculaAstel";
                parameters.Add(new SqlParameter("@matriculaAstel", matriculaAstel.Value));
            }

            if (!string.IsNullOrWhiteSpace(cidade))
            {
                filters += " AND c.Cidade LIKE @cidade";
                parameters.Add(new SqlParameter("@cidade", $"%{cidade}%"));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                filters += " AND c.Estado LIKE @estado";
                parameters.Add(new SqlParameter("@estado", $"%{estado}%"));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                filters += " AND c.Email LIKE @email";
                parameters.Add(new SqlParameter("@email", $"%{email}%"));
            }

            if (!string.IsNullOrWhiteSpace(telefone))
            {
                filters += " AND c.Telefone LIKE @telefone";
                parameters.Add(new SqlParameter("@telefone", $"%{telefone}%"));
            }

            if (inicio.HasValue)
            {
                filters += " AND (f.Ano IS NULL OR DATEFROMPARTS(f.Ano, f.Mes, 1) >= @inicio)";
                parameters.Add(new SqlParameter("@inicio", inicio.Value));
            }

            if (fim.HasValue)
            {
                filters += " AND (f.Ano IS NULL OR DATEFROMPARTS(f.Ano, f.Mes, 1) <= @fim)";
                parameters.Add(new SqlParameter("@fim", fim.Value));
            }
            var fullCte = cte + filters + "\n)";

            // ------------------------- COUNT -------------------------
            string countSql = fullCte + @"
SELECT COUNT(1)
FROM Base
WHERE 1 = 1
";

            var countParams = parameters
                .Select(p => new SqlParameter(p.ParameterName, p.Value))
                .ToList();

            if (inadimplente.HasValue)
            {
                countSql += " AND Inadimplente = @inadimplente";
                countParams.Add(new SqlParameter("@inadimplente", inadimplente.Value ? 1 : 0));
            }

            int totalCount = await ExecuteCountAsync(connStr, countSql, countParams.ToArray());

            // ------------------------- SELECT FINAL -------------------------
            string finalSql = fullCte + @"
SELECT *
FROM Base
WHERE 1 = 1
";

            var selectParams = parameters
                .Select(p => new SqlParameter(p.ParameterName, p.Value))
                .ToList();

            if (inadimplente.HasValue)
            {
                finalSql += " AND Inadimplente = @inadimplente";
                selectParams.Add(new SqlParameter("@inadimplente", inadimplente.Value ? 1 : 0));
            }

            // 🔥 NOVA ORDENAÇÃO:
            // 1. Não inadimplente primeiro
            // 2. Depois inadimplente
            // 3. Nome A→Z
            finalSql += @"
ORDER BY Inadimplente ASC, Nome ASC
OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;
";

            selectParams.Add(new SqlParameter("@offset", (pageNumber - 1) * pageSize));
            selectParams.Add(new SqlParameter("@limit", pageSize));

            var dtos = new List<DadosFinanceirosDTO>();

            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(finalSql, conn);
            cmd.Parameters.AddRange(selectParams.ToArray());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                dtos.Add(new DadosFinanceirosDTO
                {
                    Id = reader["Id"]?.ToString() ?? "",
                    IdDadosCadastrais = Convert.ToInt64(reader["IdDadosCadastrais"]),
                    MatriculaSistel = reader["MatriculaSistel"] as long?,
                    MatriculaAstel = Convert.ToInt64(reader["MatriculaAstel"]),
                    Nome = reader["Nome"].ToString(),
                    CPF = reader["CPF"].ToString(),
                    RG = reader["RG"].ToString(),
                    Endereco = reader["Endereco"].ToString(),
                    Telefone = reader["Telefone"].ToString(),
                    Situacao = reader["Situacao"]?.ToString(),
                    EstadoCivil = reader["EstadoCivil"]?.ToString(),
                    Ativo = Convert.ToBoolean(reader["Ativo"]),
                    Ano = reader["Ano"] as int?,
                    Mes = reader["Mes"] as int?,
                    ValorPago = reader["ValorPago"] as double?,
                    Inadimplente = Convert.ToBoolean(reader["Inadimplente"]),
                    Logradouro = reader["Logradouro"]?.ToString(),
                    CelSkype = reader["CelSkype"]?.ToString(),
                    Estado = reader["Estado"]?.ToString(),
                    Cidade = reader["Cidade"]?.ToString(),
                    TipoEndereco = reader["TipoEndereco"]?.ToString(),
                    Correspondencia = reader["Correspondencia"]?.ToString(),
                    Numero = reader["Numero"]?.ToString(),
                    Complemento = reader["Complemento"]?.ToString(),
                    Bairro = reader["Bairro"]?.ToString(),
                    Email = reader["Email"]?.ToString(),
                    CEP = reader["CEP"]?.ToString()
                });
            }

            return (dtos, totalCount);
        }


        private async Task<int> ExecuteCountAsync(string connStr, string sql, SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);

            object result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }


        // ----------------------------- CRUD -----------------------------
        public DadosFinanceiros? GetById(long id)
        {
            return _context.DadosFinanceiros
                .Include(f => f.DadosCadastrais)
                .AsNoTracking()
                .FirstOrDefault(f => f.Id == id);
        }

        public void Add(DadosFinanceiros df)
        {
            // Corrige Mes/Ano vindo como float
            df.Mes = Convert.ToInt32(df.Mes);
            df.Ano = Convert.ToInt32(df.Ano);

            df.Id = long.Parse($"{df.IdDadosCadastrais}{df.Ano}{df.Mes}");

            _context.DadosFinanceiros.Add(df);
            _context.SaveChanges();
        }

        public void Update(DadosFinanceiros df)
        {
            df.Mes = Convert.ToInt32(df.Mes);
            df.Ano = Convert.ToInt32(df.Ano);

            _context.DadosFinanceiros.Update(df);
            _context.SaveChanges();
        }

        public bool Delete(long id)
        {
            var item = _context.DadosFinanceiros.Find(id);
            if (item == null)
                return false;

            _context.DadosFinanceiros.Remove(item);
            _context.SaveChanges();
            return true;
        }

        public async Task<List<DadosFinanceirosDTO>> ExportarSemPaginacaoAsync(
            DateTime? inicio, DateTime? fim, string? nome, string? cpf,
            long? matriculaAstel, bool? inadimplente,
            string? cidade, string? estado, string? email, string? telefone)
        {
            var (dados, _) = await GetFilteredAsync(
                inicio, fim, nome, cpf, matriculaAstel, inadimplente,
                cidade, estado, email, telefone,
                pageNumber: 1,
                pageSize: int.MaxValue
            );

            return dados;
        }

        public string GerarCsv(List<DadosFinanceirosDTO> dados)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("Id,IdCadastro,MatriculaSistel,MatriculaAstel,Nome,CPF,RG,Logradouro,Numero,Complemento,Bairro,Cidade,Estado,TipoEndereco,Correspondencia,CEP,Telefone,CelSkype,Email,Situacao,EstadoCivil,Ativo,Ano,Mes,ValorPago,Inadimplente");

            foreach (var d in dados)
            {
                sb.AppendLine(string.Join(",", new string[]
                {
            d.Id?.ToString(),
            d.IdDadosCadastrais.ToString(),
            d.MatriculaSistel?.ToString() ?? "",
            d.MatriculaAstel?.ToString() ?? "",
            Escape(d.Nome),
            Escape(d.CPF),
            Escape(d.RG),
            Escape(d.Logradouro),
            Escape(d.Numero),
            Escape(d.Complemento),
            Escape(d.Bairro),
            Escape(d.Cidade),
            Escape(d.Estado),
            Escape(d.TipoEndereco),
            Escape(d.Correspondencia),
            Escape(d.CEP),
            Escape(d.Telefone),
            Escape(d.CelSkype),
            Escape(d.Email),
            Escape(d.Situacao),
            Escape(d.EstadoCivil),
            d.Ativo?.ToString() ?? "",
            d.Ano?.ToString() ?? "",
            d.Mes?.ToString() ?? "",
            d.ValorPago?.ToString() ?? "",
            d.Inadimplente ? "Sim" : "Não"
                }));
            }

            return sb.ToString();
        }

        private string Escape(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            // Escapa vírgulas e aspas
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        public byte[] GerarExcel(List<DadosFinanceirosDTO> dados)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Financeiro");

            // Cabeçalhos
            string[] headers = new[]
            {
        "Id","IdCadastro","MatriculaSistel","MatriculaAstel","Nome","CPF","RG",
        "Logradouro","Numero","Complemento","Bairro","Cidade","Estado","TipoEndereco",
        "Correspondencia","CEP","Telefone","CelSkype","Email","Situacao","EstadoCivil",
        "Ativo","Ano","Mes","ValorPago","Inadimplente"
    };

            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

            // Dados
            int row = 2;
            foreach (var d in dados)
            {
                ws.Cell(row, 1).Value = d.Id;
                ws.Cell(row, 2).Value = d.IdDadosCadastrais;
                ws.Cell(row, 3).Value = d.MatriculaSistel;
                ws.Cell(row, 4).Value = d.MatriculaAstel;
                ws.Cell(row, 5).Value = d.Nome;
                ws.Cell(row, 6).Value = d.CPF;
                ws.Cell(row, 7).Value = d.RG;
                ws.Cell(row, 8).Value = d.Logradouro;
                ws.Cell(row, 9).Value = d.Numero;
                ws.Cell(row, 10).Value = d.Complemento;
                ws.Cell(row, 11).Value = d.Bairro;
                ws.Cell(row, 12).Value = d.Cidade;
                ws.Cell(row, 13).Value = d.Estado;
                ws.Cell(row, 14).Value = d.TipoEndereco;
                ws.Cell(row, 15).Value = d.Correspondencia;
                ws.Cell(row, 16).Value = d.CEP;
                ws.Cell(row, 17).Value = d.Telefone;
                ws.Cell(row, 18).Value = d.CelSkype;
                ws.Cell(row, 19).Value = d.Email;
                ws.Cell(row, 20).Value = d.Situacao;
                ws.Cell(row, 21).Value = d.EstadoCivil;
                ws.Cell(row, 22).Value = d.Ativo.HasValue ? d.Ativo.Value : 0;
                ws.Cell(row, 23).Value = d.Ano;
                ws.Cell(row, 24).Value = d.Mes;
                ws.Cell(row, 25).Value = d.ValorPago;
                ws.Cell(row, 26).Value = d.Inadimplente ? "Sim" : "Não";

                row++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }


    }
}

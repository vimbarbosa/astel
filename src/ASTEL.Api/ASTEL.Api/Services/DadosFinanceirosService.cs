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

        public async Task<(List<DadosFinanceirosDTO> dados, int totalCount, double somaValorPago)>
            GetFilteredAsync(
                DateTime? inicio, DateTime? fim, string? nome, string? cpf,
                bool? inadimplente,
                string? cidade, string? estado, string? email, string? telefone,
                bool? descontoFolha,
                string? formapagamento,
                int pageNumber, int pageSize)
        {
            string connStr = "Server=sqlserver,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";
            //string connStr = "Server=localhost,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";

            // ------------------------- CTE -------------------------
            // Constrói a lógica de inadimplência baseada nos filtros de data
            string inadimplenteLogic = "";
            
            if (inicio.HasValue || fim.HasValue)
            {
                // Se houver filtro de data, verifica se o último mês filtrado contém pagamento
                // Se não tiver pagamento no último mês filtrado, está inadimplente
                inadimplenteLogic = @"
        CASE 
            WHEN EXISTS (
                SELECT 1 
                FROM DadosFinanceiros fx
                WHERE fx.IdDadosCadastrais = c.Id
                  AND fx.ValorPago IS NOT NULL";
                
                if (inicio.HasValue && fim.HasValue)
                {
                    // Verifica se há pagamento no último mês do período filtrado (fim)
                    inadimplenteLogic += @"
                  AND fx.Ano = YEAR(@fimInadimplente)
                  AND fx.Mes = MONTH(@fimInadimplente)";
                }
                else if (fim.HasValue)
                {
                    // Se só tem fim, verifica o mês de fim
                    inadimplenteLogic += @"
                  AND fx.Ano = YEAR(@fimInadimplente)
                  AND fx.Mes = MONTH(@fimInadimplente)";
                }
                else if (inicio.HasValue)
                {
                    // Se só tem início, verifica o mês de início
                    inadimplenteLogic += @"
                  AND fx.Ano = YEAR(@inicioInadimplente)
                  AND fx.Mes = MONTH(@inicioInadimplente)";
                }
                
                inadimplenteLogic += @"
            ) THEN 0
            ELSE 1
        END AS Inadimplente";
            }
            else
            {
                // Se não houver filtro de data, considera o último mês (comportamento atual)
                inadimplenteLogic = @"
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
        END AS Inadimplente";
            }

            // Constrói a condição ON do LEFT JOIN para filtros de data
            string joinCondition = "ON c.Id = f.IdDadosCadastrais";
            
            if (inicio.HasValue || fim.HasValue)
            {
                if (inicio.HasValue && fim.HasValue)
                {
                    joinCondition += " AND (f.Ano IS NULL OR (DATEFROMPARTS(f.Ano, f.Mes, 1) >= @inicio AND DATEFROMPARTS(f.Ano, f.Mes, 1) <= @fim))";
                }
                else if (inicio.HasValue)
                {
                    joinCondition += " AND (f.Ano IS NULL OR DATEFROMPARTS(f.Ano, f.Mes, 1) >= @inicio)";
                }
                else if (fim.HasValue)
                {
                    joinCondition += " AND (f.Ano IS NULL OR DATEFROMPARTS(f.Ano, f.Mes, 1) <= @fim)";
                }
            }

            var cte = $@"
;WITH BaseCompleta AS (
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
        c.DescontoFolha,                
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
        c.FormaPagamento,
        f.Ano,
        f.Mes,
        f.ValorPago,
{inadimplenteLogic},
        ROW_NUMBER() OVER (PARTITION BY c.Id ORDER BY 
            CASE WHEN f.Ano IS NULL THEN 0 ELSE 1 END DESC,
            f.Ano DESC, 
            f.Mes DESC
        ) AS RowNum

    FROM DadosCadastrais c
    LEFT JOIN DadosFinanceiros f
        {joinCondition}
    WHERE c.Ativo = 1
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
                parameters.Add(new SqlParameter("@inicio", inicio.Value));
                
                // Adiciona parâmetros para a lógica de inadimplência se necessário
                if (inadimplenteLogic.Contains("@inicioInadimplente"))
                {
                    parameters.Add(new SqlParameter("@inicioInadimplente", inicio.Value));
                }
            }

            if (fim.HasValue)
            {
                parameters.Add(new SqlParameter("@fim", fim.Value));
                
                // Adiciona parâmetros para a lógica de inadimplência se necessário
                if (inadimplenteLogic.Contains("@fimInadimplente"))
                {
                    parameters.Add(new SqlParameter("@fimInadimplente", fim.Value));
                }
            }

            if (descontoFolha.HasValue)
            {
                filters += " AND c.DescontoFolha = @descontoFolha";
                parameters.Add(new SqlParameter("@descontoFolha", descontoFolha.Value ? 1 : 0));
            }

            if (!string.IsNullOrWhiteSpace(formapagamento))
            {
                filters += " AND c.FormaPagamento LIKE @formapagamento";
                parameters.Add(new SqlParameter("@formapagamento", $"%{formapagamento}%"));
            }

            // Aplica os filtros na CTE BaseCompleta antes de criar a CTE Base
            var fullCte = cte + filters + @"
),
Base AS (
    SELECT 
        Id,
        IdDadosCadastrais,
        MatriculaSistel,
        MatriculaAstel,
        Nome,
        CPF,
        RG,
        Endereco,
        EstadoCivil,
        Telefone,
        Situacao,
        Ativo,
        DescontoFolha,
        Logradouro,
        CelSkype,
        Estado,
        Cidade,
        TipoEndereco,
        Correspondencia,
        Numero,
        Complemento,
        Bairro,
        Email,
        CEP,
        FormaPagamento,
        Ano,
        Mes,
        ValorPago,
        Inadimplente
    FROM BaseCompleta
    WHERE RowNum = 1
)";

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

            // ------------------------- SOMA VALOR PAGO -------------------------
            // A soma deve respeitar os mesmos filtros da consulta principal
            // Soma apenas registros onde ValorPago IS NOT NULL
            string sumSql = fullCte + @"
SELECT ISNULL(SUM(ValorPago), 0)
FROM Base
WHERE ValorPago IS NOT NULL
";

            var sumParams = parameters
                .Select(p => new SqlParameter(p.ParameterName, p.Value))
                .ToList();

            if (inadimplente.HasValue)
            {
                sumSql += " AND Inadimplente = @inadimplente";
                sumParams.Add(new SqlParameter("@inadimplente", inadimplente.Value ? 1 : 0));
            }

            double somaValorPago = await ExecuteSumAsync(connStr, sumSql, sumParams.ToArray());

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
                    DescontoFolha = reader["DescontoFolha"] != DBNull.Value    // ⬅️ ALTERADO
                        ? Convert.ToBoolean(reader["DescontoFolha"])
                        : (bool?)null,
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
                    CEP = reader["CEP"]?.ToString(),
                    FormaPagamento = reader["FormaPagamento"]?.ToString()
                });
            }

            return (dtos, totalCount, somaValorPago);
        }

        private async Task<double> ExecuteSumAsync(string connStr, string sql, SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);

            object result = await cmd.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return 0.0;
            
            return Convert.ToDouble(result);
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

            // Remove registro existente se já existir (mesmo Id = mesmo IdDadosCadastrais + Ano + Mes)
            var existente = _context.DadosFinanceiros.Find(df.Id);
            if (existente != null)
            {
                _context.DadosFinanceiros.Remove(existente);
                _context.SaveChanges(); // Salva a remoção antes de inserir o novo
            }

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
    bool? inadimplente,
    string? cidade, string? estado, string? email, string? telefone, bool? descontoFolha, string? formapagamento)
        {
            var (dados, _, _) = await GetFilteredAsync(
                inicio, fim, nome, cpf, inadimplente,
                cidade, estado, email, telefone, descontoFolha, formapagamento,
                pageNumber: 1,
                pageSize: int.MaxValue
            );

            return dados;
        }


        public string GerarCsv(List<DadosFinanceirosDTO> dados)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("Id,IdCadastro,MatriculaSistel,MatriculaAstel,Nome,CPF,RG,Logradouro,Numero,Complemento,Bairro,Cidade,Estado,TipoEndereco,Correspondencia,CEP,Telefone,CelSkype,Email,Situacao,EstadoCivil,Ativo,FormaPagamento,Ano,Mes,ValorPago,Inadimplente");

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
            Escape(d.FormaPagamento),
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

        public byte[] GerarExcel(List<DadosFinanceirosDTO> dados, List<string>? columns)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Financeiro");

            // Todas as colunas possíveis
            var allColumns = new Dictionary<string, Func<DadosFinanceirosDTO, object?>>
    {
        { "Id", d => d.Id },
        { "IdCadastro", d => d.IdDadosCadastrais },
        { "MatriculaSistel", d => d.MatriculaSistel },
        { "MatriculaAstel", d => d.MatriculaAstel },
        { "Nome", d => d.Nome },
        { "CPF", d => d.CPF },
        { "RG", d => d.RG },
        { "Logradouro", d => d.Logradouro },
        { "Numero", d => d.Numero },
        { "Complemento", d => d.Complemento },
        { "Bairro", d => d.Bairro },
        { "Cidade", d => d.Cidade },
        { "Estado", d => d.Estado },
        { "TipoEndereco", d => d.TipoEndereco },
        { "Correspondencia", d => d.Correspondencia },
        { "CEP", d => d.CEP },
        { "Telefone", d => d.Telefone },
        { "CelSkype", d => d.CelSkype },
        { "Email", d => d.Email },
        { "Situacao", d => d.Situacao },
        { "EstadoCivil", d => d.EstadoCivil },
        { "Ativo", d => d.Ativo },
        { "DescontoFolha", d => d.DescontoFolha }, // ⬅️ INCLUÍDO
        { "FormaPagamento", d => d.FormaPagamento },
        { "Ano", d => d.Ano },
        { "Mes", d => d.Mes },
        { "ValorPago", d => d.ValorPago },
        { "Inadimplente", d => d.Inadimplente ? "Sim" : "Não" }
    };

            // Se o front não enviar colunas → exporta tudo
            var selectedColumns = columns == null || columns.Count == 0
                ? allColumns.Keys.ToList()
                : columns.Where(c => allColumns.ContainsKey(c)).ToList();

            // Cabeçalhos dinâmicos
            for (int i = 0; i < selectedColumns.Count; i++)
                ws.Cell(1, i + 1).Value = selectedColumns[i];

            ws.Range(1, 1, 1, selectedColumns.Count).Style.Font.Bold = true;

            // Linhas
            int row = 2;
            foreach (var d in dados)
            {
                for (int col = 0; col < selectedColumns.Count; col++)
                {
                    string columnName = selectedColumns[col];
                    var value = allColumns[columnName](d);

                    ws.Cell(row, col + 1).SetValue(value?.ToString() ?? "");
                }
                row++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public byte[] GerarModeloImportacao(List<DadosFinanceirosDTO> dados)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Importação");

            // Cabeçalhos do modelo de importação
            ws.Cell(1, 1).Value = "Matrícula Sistel Nº";
            ws.Cell(1, 2).Value = "ID Cliente";
            ws.Cell(1, 3).Value = "Nome";
            ws.Cell(1, 4).Value = "Ano";
            ws.Cell(1, 5).Value = "Mês";
            ws.Cell(1, 6).Value = "Valor Pago";

            // Formatação do cabeçalho
            ws.Range(1, 1, 1, 6).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 6).Style.Fill.BackgroundColor = XLColor.LightGray;

            // Preenche os dados (Nome e Matricula Astel já vêm preenchidos)
            int row = 2;
            foreach (var d in dados)
            {
                ws.Cell(row, 1).Value = d.MatriculaSistel?.ToString() ?? "";
                ws.Cell(row, 2).Value = d.MatriculaAstel?.ToString() ?? "";
                ws.Cell(row, 3).Value = d.Nome ?? "";
                ws.Cell(row, 4).Value = ""; // Ano - deixar vazio para preencher
                ws.Cell(row, 5).Value = ""; // Mês - deixar vazio para preencher
                ws.Cell(row, 6).Value = ""; // Valor Pago - deixar vazio para preencher
                row++;
            }

            // Ajusta largura das colunas
            ws.Column(1).Width = 18; // Matrícula Sistel
            ws.Column(2).Width = 12; // ID Cliente
            ws.Column(3).Width = 30; // Nome
            ws.Column(4).Width = 8;  // Ano
            ws.Column(5).Width = 8;  // Mês
            ws.Column(6).Width = 15; // Valor Pago

            // Formata coluna de valor como número
            ws.Column(6).Style.NumberFormat.Format = "#,##0.00";

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<List<HistoricoPagamentoDTO>> GetHistoricoPagamentoPorUsuarioAsync(long idDadosCadastrais)
        {
            string connStr = "Server=sqlserver,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";
            //string connStr = "Server=localhost,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";

            var sql = @"
                SELECT TOP (1000) 
                    [Id],
                    [IdDadosCadastrais],
                    [Ano],
                    [Mes],
                    [ValorPago]
                FROM [ASTEL].[dbo].[DadosFinanceiros]
                WHERE IdDadosCadastrais = @IdDadosCadastrais
                ORDER BY [Ano] DESC, [Mes] DESC";

            var historico = new List<HistoricoPagamentoDTO>();

            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@IdDadosCadastrais", idDadosCadastrais));

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                historico.Add(new HistoricoPagamentoDTO
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    IdDadosCadastrais = Convert.ToInt64(reader["IdDadosCadastrais"]),
                    Ano = reader["Ano"] as int?,
                    Mes = reader["Mes"] as int?,
                    ValorPago = reader["ValorPago"] as double?
                });
            }

            return historico;
        }

        public async Task<List<HistoricoPagamentoDTO>> GetDadosFinanceirosPorCadastroAsync(
            long idDadosCadastrais, 
            DateTime? dataInicio = null, 
            DateTime? dataFim = null)
        {
            string connStr = "Server=sqlserver,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";
            //string connStr = "Server=localhost,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";

            var sql = @"
                SELECT 
                    [Id],
                    [IdDadosCadastrais],
                    [Ano],
                    [Mes],
                    [ValorPago]
                FROM [ASTEL].[dbo].[DadosFinanceiros]
                WHERE IdDadosCadastrais = @IdDadosCadastrais";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@IdDadosCadastrais", idDadosCadastrais)
            };

            if (dataInicio.HasValue)
            {
                sql += " AND DATEFROMPARTS([Ano], [Mes], 1) >= @DataInicio";
                parameters.Add(new SqlParameter("@DataInicio", dataInicio.Value));
            }

            if (dataFim.HasValue)
            {
                sql += " AND DATEFROMPARTS([Ano], [Mes], 1) <= @DataFim";
                parameters.Add(new SqlParameter("@DataFim", dataFim.Value));
            }

            sql += " ORDER BY [Ano] DESC, [Mes] DESC";

            var registros = new List<HistoricoPagamentoDTO>();

            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters.ToArray());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                registros.Add(new HistoricoPagamentoDTO
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    IdDadosCadastrais = Convert.ToInt64(reader["IdDadosCadastrais"]),
                    Ano = reader["Ano"] as int?,
                    Mes = reader["Mes"] as int?,
                    ValorPago = reader["ValorPago"] as double?
                });
            }

            return registros;
        }

    }
}

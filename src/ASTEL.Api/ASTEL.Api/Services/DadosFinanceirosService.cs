using ASTEL.Api.Data;
using ASTEL.Api.DTOs;
using ASTEL.Api.Models;
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
                 int pageNumber, int pageSize)
        {
            //string connStr = "Server=localhost,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";
            string connStr = "Server=sqlserver-2022,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;\r\n";

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
    WHERE 1 = 1
";

            var filters = "";
            var parameters = new List<SqlParameter>();

            // ------------------------- FILTROS -------------------------
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
                    Inadimplente = Convert.ToBoolean(reader["Inadimplente"])
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
    }
}

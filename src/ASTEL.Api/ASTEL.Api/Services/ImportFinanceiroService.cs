using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text;

namespace ASTEL.Api.Services
{
    public class ImportFinanceiroService
    {
        private readonly string _connectionString;

        public ImportFinanceiroService(IConfiguration configuration)
        {
            //_connectionString = configuration.GetConnectionString("DefaultConnection")
            //    ?? "Server=localhost,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=sqlserver-2022,1433;Database=ASTEL;User Id=sa;Password=stel@123;TrustServerCertificate=True;";
        }

        public async Task<byte[]> ExportCsvAsync()
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
        SELECT 
            MatriculaSistel AS [Matrícula Sistel Nº],
            MatriculaAstel AS [ID Cliente],
            Ano AS [Ano],
            Mes AS [Mês],
            ValorPago AS [Valor Pago]
        FROM DadosFinanceiros
        ORDER BY MatriculaSistel, Ano, Mes;
    ";

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var sb = new StringBuilder();
            // Cabeçalho atualizado
            sb.AppendLine("Matrícula Sistel Nº;ID Cliente;Ano;Mês;Valor Pago (R$)");

            while (await reader.ReadAsync())
            {
                long matriculaSistel = reader.GetInt64(0);
                long matriculaAstel = reader.GetInt64(1);
                int ano = reader.GetInt32(2);
                double mes = reader.GetDouble(3);
                string valorPago = reader.IsDBNull(4)
                    ? ""
                    : reader.GetDouble(4).ToString("F2", CultureInfo.InvariantCulture);

                sb.AppendLine($"{matriculaSistel};{matriculaAstel};{ano};{mes.ToString(CultureInfo.InvariantCulture)};{valorPago}");
            }

            var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            return utf8WithBom.GetBytes(sb.ToString());
        }



        public async Task<string> ImportCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return "Nenhum arquivo CSV foi enviado.";

            var dataTable = CriarEstruturaTabela();
            using var reader = new StreamReader(file.OpenReadStream(), DetectEncoding(file), true);

            string? headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
                return "Arquivo CSV vazio ou inválido.";

            var headers = headerLine
                .Split(';')
                .Select((h, i) => new { Header = h.Trim().ToLower(), Index = i })
                .Where(x => !string.IsNullOrEmpty(x.Header))
                .GroupBy(x => x.Header)
                .Select(g => g.First())
                .ToDictionary(x => x.Header, x => x.Index);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var cols = line.Split(';');
                if (cols.Length < headers.Count)
                    continue;

                var row = PreencherDataRow(dataTable, headers, cols);
                if (row != null)
                    dataTable.Rows.Add(row);
            }

            if (dataTable.Rows.Count == 0)
                return "Nenhum registro válido encontrado no arquivo CSV.";

            await InserirOuAtualizarEmLoteAsync(dataTable);
            return $"{dataTable.Rows.Count} registros de dados financeiros processados com sucesso!";
        }

        public async Task<byte[]> ExportCadastraisCsvAsync()
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
        SELECT
            MatriculaSistel AS [Matrícula Sistel Nº],
            MatriculaAstel AS [ID Cliente],
            Nome AS [Nome],
            Endereco AS [Endereço],
            Situacao AS [Situação],
            ValorBeneficio AS [Valor do Benefício],
            EstadoCivil AS [Estado Civil],
            Telefone AS [Telefone],
            NomeEsposa AS [Nome da Esposa],
            CPF AS [CPF],
            RG AS [RG],
            Ativo AS [Ativo],
            DescontoFolha AS [Desconto em Folha]
        FROM DadosCadastrais
        ORDER BY MatriculaSistel;
    ";

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Matrícula Sistel Nº;ID Cliente;Nome;Endereço;Situação;Valor do Benefício;Estado Civil;Telefone;Nome da Esposa;CPF;RG;Ativo;Desconto em Folha");

            while (await reader.ReadAsync())
            {
                string matriculaSistel = reader["Matrícula Sistel Nº"]?.ToString() ?? "";
                string matriculaAstel = reader["ID Cliente"]?.ToString() ?? "";
                string nome = reader["Nome"]?.ToString() ?? "";
                string endereco = reader["Endereço"]?.ToString() ?? "";
                string situacao = reader["Situação"]?.ToString() ?? "";
                string valorBeneficio = reader["Valor do Benefício"]?.ToString() ?? "";
                string estadoCivil = reader["Estado Civil"]?.ToString() ?? "";
                string telefone = reader["Telefone"]?.ToString() ?? "";
                string nomeEsposa = reader["Nome da Esposa"]?.ToString() ?? "";
                string cpf = reader["CPF"]?.ToString() ?? "";
                string rg = reader["RG"]?.ToString() ?? "";
                string ativo = (reader["Ativo"] is bool b1) ? (b1 ? "Sim" : "Não") : "";
                string desconto = (reader["Desconto em Folha"] is bool b2) ? (b2 ? "Sim" : "Não") : "";

                sb.AppendLine($"{matriculaSistel};{matriculaAstel};{nome};{endereco};{situacao};{valorBeneficio};{estadoCivil};{telefone};{nomeEsposa};{cpf};{rg};{ativo};{desconto}");
            }

            var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            return utf8WithBom.GetBytes(sb.ToString());
        }


        private DataTable CriarEstruturaTabela()
        {
            var table = new DataTable();
            table.Columns.AddRange(new[]
            {
                new DataColumn("MatriculaSistel", typeof(long)),
                new DataColumn("MatriculaAstel", typeof(long)),
                new DataColumn("Ano", typeof(int)),
                new DataColumn("Mes", typeof(double)),
                new DataColumn("ValorPago", typeof(double))
            });
            return table;
        }


        private async Task InserirOuAtualizarEmLoteAsync(DataTable dataTable)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var createTempTable = @"
        IF OBJECT_ID('tempdb..#TempDadosFinanceiros') IS NOT NULL DROP TABLE #TempDadosFinanceiros;
        CREATE TABLE #TempDadosFinanceiros (
            MatriculaSistel BIGINT,
            MatriculaAstel BIGINT,
            Ano INT,
            Mes FLOAT,
            ValorPago FLOAT
        );
    ";
            await using (var createCmd = new SqlCommand(createTempTable, connection))
                await createCmd.ExecuteNonQueryAsync();

            using (var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "#TempDadosFinanceiros",
                BatchSize = 1000
            })
            {
                foreach (DataColumn col in dataTable.Columns)
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);

                await bulkCopy.WriteToServerAsync(dataTable);
            }

            var mergeSql = @"
                    MERGE INTO DadosFinanceiros AS Target
                    USING #TempDadosFinanceiros AS Source
                    ON Target.MatriculaSistel = Source.MatriculaSistel
                       AND Target.MatriculaAstel = Source.MatriculaAstel
                       AND Target.Ano = Source.Ano
                    WHEN MATCHED THEN
                        UPDATE SET 
                            Target.Mes = Source.Mes,
                            Target.ValorPago = Source.ValorPago
                    WHEN NOT MATCHED BY TARGET THEN
                        INSERT (MatriculaSistel, MatriculaAstel, Ano, Mes, ValorPago)
                        VALUES (Source.MatriculaSistel, Source.MatriculaAstel, Source.Ano, Source.Mes, Source.ValorPago);
    ";

            await using (var mergeCmd = new SqlCommand(mergeSql, connection))
                await mergeCmd.ExecuteNonQueryAsync();
        }


        private DataRow? PreencherDataRow(DataTable table, Dictionary<string, int> headers, string[] cols)
        {
            try
            {
                var row = table.NewRow();

                string? Get(string key)
                {
                    key = key.ToLowerInvariant();
                    if (!headers.ContainsKey(key))
                        return null;
                    int index = headers[key];
                    return index < cols.Length ? cols[index].Trim() : null;
                }

                string? matriculaSistelTexto = NormalizeScientificNotation(Get("matrícula sistel nº"));
                string? matriculaAstelTexto = NormalizeScientificNotation(Get("id cliente"));

                long matriculaSistel = ParseLong(matriculaSistelTexto);
                long matriculaAstel = ParseLong(matriculaAstelTexto);
                int ano = ParseInt(Get("ano"));
                double mes = ParseDouble(Get("mês") ?? Get("mes"));
                double valorPago = ParseDouble(Get("valor pago") ?? Get("valor pago (r$)"));

                row["MatriculaSistel"] = matriculaSistel;
                row["MatriculaAstel"] = matriculaAstel;
                row["Ano"] = ano;
                row["Mes"] = mes;
                row["ValorPago"] = valorPago;

                return row;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar linha: {string.Join(';', cols)}");
                Console.WriteLine($"   Detalhes: {ex.Message}");
                return null;
            }
        }


        private static Encoding DetectEncoding(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            char[] buffer = new char[1024];
            reader.Read(buffer, 0, buffer.Length);
            stream.Position = 0;

            if (buffer.Any(c => c == '�'))
                return Encoding.GetEncoding("ISO-8859-1");

            return Encoding.UTF8;
        }

        private static long ParseLong(string? value)
        {
            if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return 0;
        }

        private static int ParseInt(string? value)
        {
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return 0;
        }

        private static double ParseDouble(string? value)
        {
            if (double.TryParse(value?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return 0.0;
        }

        private static string NormalizeScientificNotation(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // Tenta detectar padrão tipo "1.23E+14"
            if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^[0-9]+(\.[0-9]+)?[eE][\+\-]?[0-9]+$"))
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
                    return num.ToString("0", CultureInfo.InvariantCulture); // converte pra inteiro sem notação científica
            }

            return value;
        }
    }
}
